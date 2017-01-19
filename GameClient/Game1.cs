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
        private List<PlayerData> Players_Logged_In = new List<PlayerData>();
        //importent i think this is how they are turning the log in on or off i dont think we need it htough 
        private GetGameInputComponent login_Key;

        //for gamertag and password
        string gamerTag = string.Empty;
        string password = string.Empty;

        //for group message i think
        private FadeText Chat_Message;

        private bool Logged_In = false;
        private bool Logged_In_Failed = false;


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
        private string validation_PlayerMessage;
        private string chatMessage;
        private string playerValidationMessage;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            //connection = new HubConnection("http://localhost:5864/"); //localhost connection
            connection = new HubConnection("http://testingcg2016t2.azurewebsites.net"); //azure connection
            proxy = connection.CreateHubProxy("GameHub");
            message = "Connecting..";
            connection.StateChanged += Connection_StateChanged;
            connection.Start();

           // List<PlayerData> list = GetScores(5);


            IsMouseVisible = true;
            Helpers.GraphicsDevice = GraphicsDevice;
            login_Key = new GetGameInputComponent(this);// used to create login keyboard

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

            #region validate the player when logging in to match the hard coded tags

            Action<PlayerData> Validation_Player = validatePlayer;
            proxy.On("PlayerIsValid", Validation_Player);

            #endregion

            //message for spawing in the player

            #region starting for players joining

            Action<Joined> PlayerStart_Pos = valid_Player_Pos;
            proxy.On("Player_Start_Pos", PlayerStart_Pos);

            #endregion

            //message for moving new postions
            #region validating the new player positions when they move on screen

            Action<MoveMessage> Player_Pos = valid_Player_New_Pos;
            proxy.On("PlayerStart_Pos_new", Player_Pos);

            #endregion

            //message for seni
            #region message for group chat

            Action<string> Send_Message = validation_Group_Message;
            proxy.On("Show_Message", Send_Message);

            #endregion

            #endregion
            proxy.Invoke("join");
            // all other messages from Server go here
        }

        #region new code

        #region player spawn in code

        private void valid_Player_Pos(Joined Other_Players)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region new code

        #region groupmessage
        //for gorup messaging
        private void validation_Group_Message(string TextMessage)
        {
            Chat_Message = new FadeText(this, Vector2.Zero, TextMessage);
        }

        #endregion


        #region validate the new movement poistions 
        //for new postions
        private void valid_Player_New_Pos(MoveMessage newPos)
        {
            foreach (Player op in OtherPlayers)
            {
                if (op.id.ToString() == newPos.playerID)
                    op.position = new Vector2(newPos.NewX, newPos.NewY);
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


        #region join message
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
        #endregion

        #endregion

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


            #region new code
            if (currentState == gamestates.login)
            {
                CheckLogedInPlayers();

                if (InputEngine.IsKeyPressed(Keys.Enter) && !login_Key.Visible)
                {
                    login_Key.Visible = true;
                    
                    InputEngine.ClearState();
                }

                if (login_Key.Complete)
                {
                    gamerTag = login_Key.Name;
                    password = login_Key.Password;

                    login_Key.Clear();

                    InputEngine.ClearState();

                    
                    if (connection.State == ConnectionState.Connected)
                        if (gamerTag != null && password != null)
                        {
                            get_Player();
                            subscribeToMessages();
                            CheckLogedInPlayers();
                                          
                        }

                }

                if (Logged_In == true)
                {
                    if (Players_Logged_In.Count >= 2)
                        currentState = gamestates.game;
                }

            }
            #endregion

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

            #region new code
            if (currentState == gamestates.login)
            {
                GraphicsDevice.Clear(Color.Gray);
                string Message = "Press The Enter Key to continue.";
                spriteBatch.DrawString(GameFont, Message, new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2), Color.LightSkyBlue);

                if (Logged_In != false || Logged_In_Failed != false)
                    if (playerData != null)
                        spriteBatch.DrawString(GameFont, validation_PlayerMessage, new Vector2(200, 50), Color.White);
               

                

                if (Players_Logged_In != null)
                {

                    foreach (PlayerData player in Players_Logged_In)
                    {
                        string playerMessage = "Player:  " + player.GamerTag + " has connected to game";

                        if (player.GamerTag != playerData.GamerTag)
                            spriteBatch.DrawString(GameFont, playerMessage, new Vector2(100, GraphicsDevice.Viewport.Height / 2), Color.Gray);

                        
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
                #endregion

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

        #region new code
        private void typeMessage()
        {
            login_Key.Visible = true;
            //Clears anything written before this point
            InputEngine.ClearState();

            if (login_Key.Complete)
            {
                chatMessage = login_Key.Name;
                login_Key.Clear();
                InputEngine.ClearState();

                //checks to see if the connection is connected
                if (chatMessage != null)
                {
                    sendAllClientMessage(chatMessage);
                }

            }
        }
        #endregion

        #region new code
        private void sendAllClientMessage(string textMessage)
        {
            proxy.Invoke("send_Message", new string[] { textMessage });
        }

        private void validatePlayer(PlayerData player)
        {
            Logged_In = true;
            playerData = player;

            validation_PlayerMessage = "Player has been validated Gamertag is: " + player.GamerTag;

            getPlayerData();
        }
        #endregion

        private void getPlayerData()
        {
            proxy.Invoke<PlayerData>("getPlayer",
                                new string[] { "" })
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

        
        #region new code
        private void CheckLogedInPlayers()
        {
            Action<List<PlayerData>> All_Players_Logged_In = Show_Players;
            proxy.On("PlayersValidated", All_Players_Logged_In);
        }
        //
        private void get_Player()
        {
            proxy.Invoke("ValidatePlayer", new string[] { gamerTag, password });
        }
        //
        private void Show_Players(List<PlayerData> LogedInPlayers)
        {
            Logged_In = true;
            Players_Logged_In = LogedInPlayers;
        }
#endregion

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
