using System;
using GameData;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TextEffects;
using Sprites;
using Cameras;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace GameClient
{
    public static class Helpers
    {
        public static GraphicsDevice GraphicsDevice { get; set; }
    }
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        IHubProxy proxy;
        HubConnection connection;
        private bool connected;
        private bool joined;

        SpriteFont ScoreFont;
        public enum gamestates { login, game, scoreboard };
        gamestates currentState = gamestates.login;
        private PlayerData playerData;
        public Player player;
        Vector2 worldCoords;
        private Rectangle worldRect;
        private FollowCamera followCamera;
        SpriteFont scoreBoardfontF, timerF, scorePointsF;
        int playerScore;
        PlayerData data;
        private string InGameMessage = string.Empty;


        #region new code
        //this is for checking the log in i think
        private List<PlayerData> allLogedInPlayers = new List<PlayerData>();
        //importent i think this is how they are turning the log in on or off i dont think we need it htough 
        private GetGameInputComponent loginKey;

        //for gamertag and password
        string gamerTag = string.Empty;
        string password = string.Empty;

        //for group message i think
        private FadeText groupMessage;

        private bool loggedIn = false;
        private bool loggedInFailed = false;


        //for other players
        private List<Player> OtherPlayers = new List<Player>();
        #endregion



        Menu menu;
        string[] menuOptions = new string[] { "Fast", "Normal", "Strong" };

        Texture2D backGround;
        SoundEffect[] sounds;

        KeyboardState oldState, newState;


        //Used with the player sprite
        Vector2 origin;
        Vector2 scale;
        Vector2 center;
        Vector2 mousePos;
        Vector2 direction;

        private string message;
        private string errorMessage;

        private string timerMessage = "Time to start:  ";
        private string GameTimerMessage = "Game over in: ";

        static string name;

        

        Texture2D collectable;

        private bool Connected;
        public SpriteFont GameFont { get; private set; }

        public SpriteFont KeyboardFont;
        private string validatePlayerMessage;
        private string chatMessage;
        private string playerValidationMessage;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //connection = new HubConnection("http://localhost:5864/");
            connection = new HubConnection("http://testingcg2016t2.azurewebsites.net");
            proxy = connection.CreateHubProxy("GameHub");
            message = "Connecting..";
            connection.StateChanged += Connection_StateChanged;
            connection.Start();

           // List<PlayerData> list = GetScores(5);


            IsMouseVisible = true;
            Helpers.GraphicsDevice = GraphicsDevice;
            loginKey = new GetGameInputComponent(this);// used to create login keyboard

            base.Initialize();
        }

        private void Connection_StateChanged(StateChange state)
        {
            switch (state.NewState)
            {
                case ConnectionState.Connected:
                    connected = true;
                    message = "Connected to Game Hub ";
                    subscribeToMessages();
                    getPlayerData();
                    getCollectableData();
                    break;

                
            }
        }

        private void clientChat()
        {
            Action<string, string> SendMessageRecieved = recieved_a_message;
            proxy.On("broadcastMessage", SendMessageRecieved);

            connection.Start().Wait();

            Console.Write("Enter your Name: ");// Application for entering your name and sending it to other clients
            name = Console.ReadLine();

            proxy.Invoke("Send", new object[] { name, "Has joined" });

            Console.ReadKey();
            connection.Stop();
        }


        private static void recieved_a_message(string sender, string message) //Recieve incoming chat messages 
        {
            Console.WriteLine("{0} : {1}", sender, message);
        }

        private static void Connection_Received(string obj)
        {
            Console.WriteLine("Message Recieved {0}", obj);
        }

        private void subscribeToMessages()
        {
            Action<ErrorMess> err = ShowError;
            proxy.On("error", err); //ms shows error message

            Action<int, int> joined = cJoined;
            proxy.On("joined", joined);//ms join message

            Action<PlayerData> recievePlayer = clientRecievePlayer;
            proxy.On("recievePlayer", recievePlayer); //ms recive player message

            Action<double> recieveCountDown = clientRecieveStartCount;//ms countdown method
            proxy.On("recieveCountDown", recieveCountDown); //ms  recive countdown message

            Action<double> recieveGameCountdown = clientRecieveGameCount; //coundown for game method
            proxy.On("recieveGameCount", recieveGameCountdown);//ms recive game countdown message 

            #region new code
            //message for validate player

            Action<PlayerData> ValidatePlayer = valid_Player;
            proxy.On("PlayerValidated", ValidatePlayer);

            //message for spawing in the player

            // Action<Joined> AllPlayersStartingPositions = valid_Positions;
            //  proxy.On("PlayersStartingPositions", AllPlayersStartingPositions);

            //message for moving new postions
            Action<MoveMessage> AllPlayersPositions = valid_NewPositions;
            proxy.On("PlayersStartingPositions", AllPlayersPositions);
            //message for seni
            Action<string> SendGroupMessage = valid_GroupMessage;
            proxy.On("ShowGroupMessage", SendGroupMessage);
            #endregion

            proxy.Invoke("join");
            // all other messages from Server go here
        }

        #region new code
        //for gorup messaging
        private void valid_GroupMessage(string textMessage)
        {
            groupMessage = new FadeText(this, Vector2.Zero, textMessage);
        }
        //for new postions
        private void valid_NewPositions(MoveMessage newPosition)
        {
            foreach (Player op in OtherPlayers)
            {
                if (op.id.ToString() == newPosition.playerID)
                    op.position = new Vector2(newPosition.NewX, newPosition.NewY);
            }
        }
        #endregion

   

        #region Action delegates for incoming server messages
        private void clientRecieveGameCount(double count)
        {
            GameTimerMessage = "Game over in: " + count.ToString(); //ms the format for the game over message
        }

        private void clientRecieveStartCount(double count)
        {
            timerMessage = "Time to start: " + count.ToString(); //ms the format for the time to start message
        }

        private void clientRecievePlayer(PlayerData obj)
        {
            if (player != null)
            {
                player.PlayerInfo = playerData;
            }
        }

        private void ShowError(ErrorMess em)
        {
            message = em.message;
        }
        #endregion

        private void cJoined(int worldX, int roldY)
        {
            worldCoords = new Vector2(worldX, roldY);
            // Setup Camera
            worldRect = new Rectangle(new Point(0, 0), worldCoords.ToPoint());
            followCamera = new FollowCamera(this, Vector2.Zero, worldCoords);
            joined = true;
            // Setup Player
            //SetupPlayer();
            proxy.Invoke("getPlayer", new object());

        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameFont = Content.Load<SpriteFont>("GameFont");
            collectable = Content.Load<Texture2D>("collectable"); //ms collectable texture
            Services.AddService<SpriteFont>(GameFont);
            Services.AddService<SpriteBatch>(spriteBatch);


            LoadAssets();//Loads all content in the game content folder


            ScoreFont = LoadedGameContent.Fonts["GameFont"];

            KeyboardFont = Content.Load<SpriteFont>("keyboardfont");

            backGround = Content.Load<Texture2D>("Space");
            //backGround = LoadedGameContent.Textures



            menu = new Menu(new Vector2(300, 250), menuOptions, KeyboardFont, GetMenuTextureArray()); //create the menu

            menu.Active = true; //set menu active


            //sounds[0] = Content.Load<SoundEffect>("sounds/footsteps");

            //player = new Player(new Texture2D(""), new SoundEffect(1, 1,), Vector2.Zero, 3, 0, 1f);
            //sounds = Content.Load<SoundEffect>("footsteps-2");


            player = new Player(this, "oldman", new Vector2(500, 500), 1, 8, 1);

            new FadeTextManager(this);

            // TODO: use this.Content to load your game content here
        }

        private Texture2D[] GetMenuTextureArray()
        {
            string[] _tex;

            _tex = new string[3];
            _tex[0] = "key";
            _tex[1] = "collectable";
            _tex[2] = "dragon";

            Texture2D[] Tex;
            Tex = new Texture2D[_tex.Length];

            for (int i = 0; i < _tex.Length; i++)
            {
                Tex[i] = LoadedGameContent.Textures[_tex[i]];
            }

            return Tex;
        }

        private void LoadAssets()
        {
            //LoadedGameContent.Sounds.Add("backing", Content.Load<SoundEffect>("Backing Track wav"));
            //LoadedGameContent.Sounds.Add("cannon fire", Game.Content.Load<SoundEffect>("cannon fire"));
            //LoadedGameContent.Sounds.Add("Impact", Game.Content.Load<SoundEffect>("Impact"));
            LoadedGameContent.Textures.Add("dragon", Content.Load<Texture2D>("dragon"));
            LoadedGameContent.Textures.Add("oldman", Content.Load<Texture2D>("oldman"));
            LoadedGameContent.Textures.Add("key", Content.Load<Texture2D>("key"));
            LoadedGameContent.Textures.Add("collectable", Content.Load<Texture2D>("collectable"));
            //LoadedGameContent.Textures.Add("background", Game.Content.Load<Texture2D>("background"));
            //LoadedGameContent.Textures.Add("Player", Game.Content.Load<Texture2D>("Player"));
            LoadedGameContent.Fonts.Add("GameFont", Content.Load<SpriteFont>("GameFont"));

            //_audioPlayer = LoadedGameContent.Sounds["backing"].CreateInstance();
            //_audioPlayer.Volume = 0.2f;
            //_audioPlayer.IsLooped = true;
            //_audioPlayer.Play();

        }

        protected override void UnloadContent()
        {
        }


        protected override void Update(GameTime gameTime)
        {
            newState = Keyboard.GetState(); //set the current keyboardState

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (!connected) return;



            if (currentState == gamestates.login)
            {
                CheckLogedInPlayers();

                if (InputEngine.IsKeyPressed(Keys.F10) && !loginKey.Visible)
                {
                    loginKey.Visible = true;
                    //Clears anything written before this point
                    InputEngine.ClearState();
                }

                if (loginKey.Done)
                {
                    gamerTag = loginKey.Name;
                    password = loginKey.Password;
                    loginKey.Clear();
                    InputEngine.ClearState();

                    //checks to see if the connection is connected
                    if (connection.State == ConnectionState.Connected)
                        if (gamerTag != null && password != null)
                        {
                            getPlayer();
                            subscribeToMessages();
                            CheckLogedInPlayers();
                            //getPlayerData();                
                        }

                }

                if (loggedIn == true)
                {
                    if (allLogedInPlayers.Count >= 2)         
                            currentState = gamestates.game;
                }

            }
            else if (currentState == gamestates.game)
            {
                if (player != null)
                {
                    player.Update(gameTime);
                    //player.position = Vector2.Clamp(player.position, Vector2.Zero,
                    //    GraphicsDevice.Viewport.Bounds.Size.ToVector2() -
                    //    new Vector2(player._skin.Width / 2,
                    //    player._skin.Height / 2));

                    if (followCamera != null)
                    {
                        followCamera.Follow(player);

                    }

                }
            }
            else if (currentState == gamestates.scoreboard)
            {

            }



            //player.Update(gameTime);


            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            
            spriteBatch.Begin();

            if (currentState == gamestates.scoreboard)
            {



                GraphicsDevice.Clear(Color.CornflowerBlue);
                Vector2 v = ScoreFont.MeasureString("string this");
                Vector2 Base = new Vector2(GraphicsDevice.Viewport.Width / 2, 100);
                Base += Base + new Vector2(0, ScoreFont.MeasureString(data.GamerTagScore).Y + 10);

                foreach (var x in GetScores(5))
                {
                    spriteBatch.DrawString(ScoreFont, x.GamerTagScore, new Vector2(graphics.PreferredBackBufferHeight / 2, graphics.PreferredBackBufferWidth / 2), Color.White);
                }
            }

            if (currentState == gamestates.login)
            {
                GraphicsDevice.Clear(Color.Black);
                string helperMessage = "Press F10";
                spriteBatch.DrawString(GameFont, helperMessage, new Vector2(500, 20), Color.White);

                if (loggedIn != false || loggedInFailed != false)
                    if (playerData != null)
                        spriteBatch.DrawString(GameFont, validatePlayerMessage, new Vector2(200, 50), Color.White);
                //else
                //spriteBatch.DrawString(GameFont, errorMessage, new Vector2(200, 50), Color.White);

                int count = 0;

                if (allLogedInPlayers != null)
                {

                    foreach (PlayerData player in allLogedInPlayers)
                    {
                        string playerMessage = "Player " + player.GamerTag + " is Connected";

                        if (player.GamerTag != playerData.GamerTag)
                            spriteBatch.DrawString(GameFont, playerMessage, new Vector2(200, 60 + count), Color.White);

                        count += 30;
                    }
                }

                //if (allLogedInPlayers.Count >= 2) ;
                //    //btnSubmit.Draw(spriteBatch);
            }

            if (currentState == gamestates.game)
            {


                GraphicsDevice.Clear(Color.CornflowerBlue);

                spriteBatch.DrawString(GameFont,
                    message,
                    new Vector2(200, 20), Color.White
                    );
                if (playerData != null)
                {
                    spriteBatch.DrawString(GameFont,
                        playerData.FirstName + " is ok ",
                        new Vector2(20, 20), Color.White
                        );
                    spriteBatch.Draw(backGround, worldRect, Color.White);

                    
                    spriteBatch.DrawString(KeyboardFont, InGameMessage, new Vector2(10, 10), Color.White);


                    spriteBatch.DrawString(GameFont, timerMessage, new Vector2(20, 20), Color.Red); //ms drawing the game timer message
                   
                    spriteBatch.DrawString(GameFont, GameTimerMessage, new Vector2(GraphicsDevice.Viewport.Height / 2, 20), Color.White); //ms drawing the game countdown message
                   
                player.Draw(spriteBatch);

                }


                spriteBatch.End();
                // TODO: Add your drawing code here



                base.Draw(gameTime);

            }
        }

        private List<PlayerData> GetScores(int count)
        {
            using (TestDbContext db = new TestDbContext())
            {
                return db.ScoreBoard.Take(count).ToList();
            }
        }

        private void typeMessage()
        {
            loginKey.Visible = true;
            //Clears anything written before this point
            InputEngine.ClearState();

            if (loginKey.Done)
            {
                chatMessage = loginKey.Name;
                loginKey.Clear();
                InputEngine.ClearState();

                //checks to see if the connection is connected
                if (chatMessage != null)
                {
                    sendAllClientMessage(chatMessage);
                }

            }
        }

        private void sendAllClientMessage(string textMessage)
        {
            proxy.Invoke("SendGroupMessage", new string[] { textMessage });
        }

        private void valid_Player(PlayerData player)
        {
            loggedIn = true;
            playerData = player;

            validatePlayerMessage = "PlayerValidated GamerTag is " + player.GamerTag;

            getPlayerData();
        }

        private void getPlayerData()
        {
            proxy.Invoke<PlayerData>("getPlayer",
                                new string[] { "popple" })
                .ContinueWith(t =>
                {
                    playerData = t.Result;
                });
        }

        private void getCollectableData()
        {
            int WorldX = GraphicsDevice.Viewport.Width;
            int WorldY = GraphicsDevice.Viewport.Height;
            int count = 10;
            proxy.Invoke<List<CollectableData>>("GetCollectables",
                                new object[] { count, WorldX, WorldY })
                .ContinueWith(t =>
                {
                    CreateGameCollecables(t.Result);
                });
        }

        private void CheckLogedInPlayers()
        {
            Action<List<PlayerData>> AllLogedInPlayers = ShowPlayers;
            proxy.On("PlayersValidated", AllLogedInPlayers);
        }

        private void getPlayer()
        {
            proxy.Invoke("ValidatePlayer", new string[] { gamerTag, password });
        }

        private void ShowPlayers(List<PlayerData> LogedInPlayers)
        {
            loggedIn = true;

            //if(LogedInPlayers.Contains(playerData))

            allLogedInPlayers = LogedInPlayers;
        }

        private void CreateGameCollecables(List<CollectableData> result)
        {

            foreach (CollectableData c in result)
            {
                new FadeText(this, Vector2.Zero,
                    "Delivered " + c.CollectableName +
                        " X: " + c.X.ToString() + " Y: " + c.X.ToString());
               
                spriteBatch.Draw(collectable);
            
            }
        }
    }
}
