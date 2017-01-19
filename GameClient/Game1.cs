using System;
using GameData;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TextEffects;
using GameClassLibrary;
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

        #region Declarations

        static Random r = new Random();
        bool gameStarted = true;

        string clientID;
        Color playerColor = Color.White;
        Color enemyColor = Color.Red;

        enum currentDisplay { Selection, Game, Score };
        currentDisplay currentState = currentDisplay.Selection;

        enum endGameStatuses { Win, Lose, Draw }
        endGameStatuses gameOutcome = endGameStatuses.Draw;

        Player player;
        Player Enemy;
        private Character playerData;


        Menu menu;
        string[] menuOptions = new string[] { "Fast", "Normal", "Strong" };

        Vector2 startVector = new Vector2(50, 250);

        Bullet newBullet;

        Texture2D backgroundTexture;
        Texture2D[] textures;
        Texture2D textureCollectable;
        Texture2D textureSuperCollectable;
        Texture2D[] textureBarrier;
        Texture2D texHealth;
        string message;

        KeyboardState oldState, newState;

        public List<Bullet> Bullets = new List<Bullet>();
        List<Collectable> Collectables = new List<Collectable>();
        List<Barrier> Barriers = new List<Barrier>();
        List<Collectable> pickUp = new List<Collectable>();
        List<Barrier> destroyBarrier = new List<Barrier>();
        List<Bullet> destroyBullets = new List<Bullet>();




        //static IHubProxy proxy;
        //HubConnection connection = new HubConnection("http://localhost:5553/");

        #endregion



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







        private string errorMessage;

        private string timerMessage = "Time to start:  ";
        private string GameTimerMessage = "Game over in: ";

        static string name;

        

        

        private bool Connected;
        public SpriteFont GameFont { get; private set; }

        public SpriteFont KeyboardFont;
        private string validation_PlayerMessage;
        private string chatMessage;
        private string playerValidationMessage;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {
            oldState = Keyboard.GetState();

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

            Action<Character> Validation_Player = validatePlayer;
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
                if (op.PlayerChar.ToString() == newPos.playerID)
                    op._position = new Vector2(newPos.NewX, newPos.NewY);
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
                player.PlayerChar = playerData;
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

            LoadGameContent();

            #region Loading Textures

            backgroundTexture = LoadedGameContent.Textures["background"]; //load all textures
            textures = new Texture2D[] { LoadedGameContent.Textures["circle"], LoadedGameContent.Textures["square"], LoadedGameContent.Textures["triangle"] };
            textureBarrier = new Texture2D[] { LoadedGameContent.Textures["barrier"], LoadedGameContent.Textures["barrier_broken"] };
            textureCollectable = LoadedGameContent.Textures["Collectable"];
            textureSuperCollectable = LoadedGameContent.Textures["SuperCollectable"];
            texHealth = LoadedGameContent.Textures["healthBar"];

            #endregion

            #region Settings
            textureBarrier = new Texture2D[] { LoadedGameContent.Textures["barrier"], LoadedGameContent.Textures["barrier_broken"] };

            for (int i = 0; i < 4; i++) //create barriers 
            {
                Barriers.Add(new Barrier(clientID, textureBarrier, new Vector2(r.Next(50, graphics.GraphicsDevice.Viewport.Width - 50), r.Next(50, graphics.GraphicsDevice.Viewport.Height - 50)), playerColor));
            }
            GameFont = LoadedGameContent.Fonts["message"];
            menu = new Menu(new Vector2(300, 250), menuOptions, GameFont, textures); //create the menu

            menu.Active = true; //set menu active

            #endregion
            //backGround = LoadedGameContent.Textures






           


            new FadeTextManager(this);

        }


        #region Methods

        private List<PlayerData> GetScores(int count)
        {
            using (TestDbContext db = new TestDbContext())
            {
                return db.ScoreBoard.Take(count).ToList();
            }
        }


        private void CreateGameCollecables(List<CollectableData> result)
        {

            //foreach (CollectableData c in result)
            //{
            //    new FadeText(this, Vector2.Zero,
            //        "Delivered " + c.CollectableName +
            //            " X: " + c.X.ToString() + " Y: " + c.X.ToString());

            //    spriteBatch.Draw(Collectables);

            //}
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


        private void getPlayerData()
        {
            proxy.Invoke<Character>("getPlayer",
                                new string[] { "" })
                .ContinueWith(t =>
                {
                    playerData = t.Result;
                });
        }

        public bool OutsideScreen(Sprite obj)
        {
            if (!obj.Rectangle.Intersects(Window.ClientBounds))
            {
                return true;
            }
            else
                return false;
        }

        private void LoadGameContent()
        {
            //Load Game Sounds
            //LoadedGameContent.Sounds.Add("", Content.Load<SoundEffect>(""));


            //load game Textures
            LoadedGameContent.Textures.Add("triangle", Content.Load<Texture2D>("textures/triangle"));
            LoadedGameContent.Textures.Add("circle", Content.Load<Texture2D>("textures/circle"));
            LoadedGameContent.Textures.Add("square", Content.Load<Texture2D>("textures/square"));
            LoadedGameContent.Textures.Add("barrier", Content.Load<Texture2D>("textures/barrier"));
            LoadedGameContent.Textures.Add("barrier_broken", Content.Load<Texture2D>("textures/barrier_broken"));
            LoadedGameContent.Textures.Add("Collectable", Content.Load<Texture2D>("textures/triangle"));
            LoadedGameContent.Textures.Add("SuperCollectable", Content.Load<Texture2D>("textures/triangle"));
            LoadedGameContent.Textures.Add("healthBar", Content.Load<Texture2D>("textures/triangle"));
            LoadedGameContent.Textures.Add("background", Content.Load<Texture2D>("textures/background"));
            LoadedGameContent.Textures.Add("dragon", Content.Load<Texture2D>("dragon"));
            LoadedGameContent.Textures.Add("oldman", Content.Load<Texture2D>("oldman"));
            LoadedGameContent.Textures.Add("key", Content.Load<Texture2D>("key"));



            //Load game fonts
            LoadedGameContent.Fonts.Add("message", Content.Load<SpriteFont>("fonts/message"));
            LoadedGameContent.Fonts.Add("scoreFont", Content.Load<SpriteFont>("fonts/scoreFont"));
            LoadedGameContent.Fonts.Add("GameFont", Content.Load<SpriteFont>("GameFont"));


            //Set music to play if you have music
            //_audioPlayer = LoadedGameContent.Sounds["backing"].CreateInstance();
            //_audioPlayer.Volume = 0.2f;
            //_audioPlayer.IsLooped = true;
            //_audioPlayer.Play();
        }

        private Player createPlayer(string id, string type, Color c)
        {
            Player temp = null;
            if (type != null)
            {

                switch (type.ToUpper()) //check for type and create the character
                {
                    case "FAST":
                        currentState = currentDisplay.Game;
                        temp = new Player(new Character(id, textures[0], 7, 3), texHealth, startVector, c, this);
                        break;
                    case "NORMAL":
                        currentState = currentDisplay.Game;
                        temp = new Player(new Character(id, textures[1], 5, 4), texHealth, startVector, c, this);
                        break;
                    case "STRONG":
                        currentState = currentDisplay.Game;
                        temp = new Player(new Character(id, textures[2], 3, 5), texHealth, startVector, c, this);
                        break;
                    default:
                        break;
                }
            }


            return temp;
        }
        #endregion

        

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
            if (currentState == currentDisplay.Selection)
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
                        currentState = currentDisplay.Game;
                }

            }
            #endregion




            //player.Update(gameTime);



            #region Select Character

            if (currentState == currentDisplay.Selection)
            {
                menu.CheckMouse();

                player = createPlayer(clientID, menu.MenuAction, playerColor);
                Enemy = new Player(new Character("Emily", textures[0], 7, 3), LoadedGameContent.Textures["square"], new Vector2(10, 10), playerColor, this);
                //if (player != null)
                //{
                //    proxy.Invoke("SendPlayer", menu.MenuAction);

                //    sendBarriers(Barriers);
                //}

                menu.MenuAction = null; //reset the selection
            }

            #endregion
            #region GameLogic 

            if (currentState == currentDisplay.Game) //if the game is running
            {
                if (gameStarted)
                {
                    player.Move(newState); //check for the player movement
                    //proxy.Invoke("UpdatePosition", player._position);

                    #region Collision
                    foreach (var item in Bullets) //check if bullet hit a barrier and destroy it
                    {
                        foreach (var bar in Barriers)
                        {
                            if (item.CollisiionDetection(bar.Rectangle))
                            {
                                if (item.createdPlayerID != bar.createdClientID)
                                {
                                    bar.GotHit(item);
                                    item.IsVisible = false;
                                    destroyBullets.Add(item);
                                    if (!bar.IsVisible)
                                        destroyBarrier.Add(bar);

                                }
                            }
                        }
                        if (item.CollisiionDetection(Enemy.Rectangle))
                            Enemy.PlayerChar.GotShoot(item);

                        if (item.CollisiionDetection(player.Rectangle))
                            player.PlayerChar.GotShoot(item);
                    }

                    foreach (var item in Collectables)
                    {
                        if (player.CollisiionDetection(item.Rectangle))
                        {
                            pickUp.Add(item);
                            item.IsVisible = false;
                            player.Collect(item);
                        }

                        if (Enemy.CollisiionDetection(item.Rectangle))
                        {
                            pickUp.Add(item);
                            item.IsVisible = false;
                            Enemy.Collect(item);
                        }
                    }

                    #endregion

                    #region fireing player controls
                    if (newState.IsKeyDown(Keys.Right) && oldState != newState && gameStarted)
                    {
                        newBullet = player.PlayerChar.Shoot(player._position, new Vector2(1, 0), playerColor); //create a bullet
                        if (newBullet != null)
                        {
                            Bullets.Add(newBullet); //add the new bullet to the list
                            //proxy.Invoke("NewBullet", newBullet._position, newBullet.flyDirection);
                        }

                    }
                    if (newState.IsKeyDown(Keys.Left) && oldState != newState && gameStarted)
                    {
                        newBullet = player.PlayerChar.Shoot(player._position, new Vector2(-1, 0), playerColor); //create a bullet
                        if (newBullet != null)
                        {
                            Bullets.Add(newBullet); //add the new bullet to the list
                            //proxy.Invoke("NewBullet", newBullet._position, newBullet.flyDirection);
                        }

                    }
                    if (newState.IsKeyDown(Keys.Up) && oldState != newState && gameStarted)
                    {
                        newBullet = player.PlayerChar.Shoot(player._position, new Vector2(0, -1), playerColor); //create a bullet
                        if (newBullet != null)
                        {
                            Bullets.Add(newBullet); //add the new bullet to the list
                            //proxy.Invoke("NewBullet", newBullet._position, newBullet.flyDirection);
                        }

                    }
                    if (newState.IsKeyDown(Keys.Down) && oldState != newState && gameStarted)
                    {
                        newBullet = player.PlayerChar.Shoot(player._position, new Vector2(0, 1), playerColor); //create a bullet
                        if (newBullet != null)
                        {
                            Bullets.Add(newBullet); //add the new bullet to the list
                            //proxy.Invoke("NewBullet", newBullet._position, newBullet.flyDirection);
                        }

                    }
                    //Bullets.Add(new Bullet(player.PlayerChar._texture, player.PlayerChar.strength, player.Position, player.FireDirection));

                    #endregion


                    foreach (var item in Bullets)
                    {
                        item.Update(); //update the Bullets
                        if (OutsideScreen(item))
                        {
                            destroyBullets.Add(item);
                        }
                    }

                    foreach (var item in destroyBarrier)
                    {
                        Barriers.Remove(item);
                    }
                    foreach (var item in pickUp)
                    {
                        Collectables.Remove(item);
                    }
                    foreach (var item in destroyBullets)
                    {
                        Bullets.Remove(item);
                    }

                    destroyBarrier.Clear();
                    pickUp.Clear();
                    destroyBullets.Clear();

                    if (Collectables.Count == 1)
                        currentState = currentDisplay.Score;
                    if (Enemy.PlayerChar.Health <= 0)
                        currentState = currentDisplay.Score;
                    if (player.PlayerChar.Health <= 0)
                        currentState = currentDisplay.Score;

                    if (currentState == currentDisplay.Score)
                    {
                        gameStarted = false;
                        //proxy.Invoke("StartGame", gameStarted);
                        if (player.score > Enemy.score)
                            gameOutcome = endGameStatuses.Win;
                        if (player.score < Enemy.score)
                            gameOutcome = endGameStatuses.Lose;
                        if (player.score == Enemy.score)
                            gameOutcome = endGameStatuses.Draw;
                    }

                }
                currentState = currentDisplay.Game;
            }

            #endregion


            if (newState.IsKeyDown(Keys.Escape) && oldState != newState) // go back to the character selection
                Exit();

            base.Update(gameTime);

            oldState = newState;
        }



        protected override void Draw(GameTime gameTime)
        {
            { 
            #region dump
            //spriteBatch.Begin();

            //if (currentState == currentDisplay.)
            //{

                

                //    GraphicsDevice.Clear(Color.CornflowerBlue);
                //    Vector2 v = ScoreFont.MeasureString("string this");
                //    Vector2 Base = new Vector2(GraphicsDevice.Viewport.Width / 2, 100);
                //    Base += Base + new Vector2(0, ScoreFont.MeasureString(data.GamerTagScore).Y + 10);

                //    foreach (var x in GetScores(5))
                //    {
                //        spriteBatch.DrawString(ScoreFont, x.GamerTagScore, new Vector2(graphics.PreferredBackBufferHeight / 2, graphics.PreferredBackBufferWidth / 2), Color.White);
                //    }
                //}

                //#region new code
                //if (currentState == gamestates.login)
                //{
                //    GraphicsDevice.Clear(Color.Gray);
                //    string Message = "Press The Enter Key to continue.";
                //    spriteBatch.DrawString(GameFont, Message, new Vector2(GraphicsDevice.Viewport.Width/2, GraphicsDevice.Viewport.Height/2), Color.LightSkyBlue);

                //    if (Logged_In != false || Logged_In_Failed != false)
                //        if (playerData != null)
                //            spriteBatch.DrawString(GameFont, validation_PlayerMessage, new Vector2(200, 50), Color.White);




                //    if (Players_Logged_In != null)
                //    {

                //        foreach (PlayerData player in Players_Logged_In)
                //        {
                //            string playerMessage = "Player:  " + player.GamerTag + " has connected to game";

                //            if (player.GamerTag != playerData.GamerTag)
                //                spriteBatch.DrawString(GameFont, playerMessage, new Vector2(100, GraphicsDevice.Viewport.Height / 2), Color.Gray);


                //        }
                //    }

                //    //if (allLogedInPlayers.Count >= 2) ;
                //    //    //btnSubmit.Draw(spriteBatch);
                //}

                //if (currentState == gamestates.game)
                //{


                //    GraphicsDevice.Clear(Color.CornflowerBlue);

                //    spriteBatch.DrawString(GameFont,
                //        message,
                //        new Vector2(200, 20), Color.White
                //        );
                //    if (playerData != null)
                //    {
                //        spriteBatch.DrawString(GameFont,
                //            playerData.FirstName + " is ok ",
                //            new Vector2(20, 20), Color.White
                //            );
                //        spriteBatch.Draw(backGround, worldRect, Color.White);


                //        spriteBatch.DrawString(KeyboardFont, InGameMessage, new Vector2(10, 10), Color.White);


                //        spriteBatch.DrawString(GameFont, timerMessage, new Vector2(20, 20), Color.Red); //ms drawing the game timer message

                //        spriteBatch.DrawString(GameFont, GameTimerMessage, new Vector2(GraphicsDevice.Viewport.Height / 2, 20), Color.White); //ms drawing the game countdown message

                //    player.Draw(spriteBatch);

                //    }
                //    #endregion

                //    spriteBatch.End();
                //    // TODO: Add your drawing code here



                //    base.Draw(gameTime);
                #endregion
                GraphicsDevice.Clear(Color.CornflowerBlue);

                if (currentState == currentDisplay.Selection)
                    menu.Draw(spriteBatch); //draw the menu
                #region Draw the Game
                if (currentState == currentDisplay.Game) //if game is started
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f); //draw the background
                    if (Enemy != null)
                        spriteBatch.DrawString(GameFont, "Score: " + Enemy.score.ToString(), new Vector2(700, 0), enemyColor);
                    spriteBatch.DrawString(GameFont, "Score: " + player.score.ToString(), new Vector2(0, 0), playerColor);
                    spriteBatch.End();

                    if (Enemy != null)
                        Enemy.Draw(spriteBatch);

                    player.Draw(spriteBatch, GameFont); //draw the player

                    foreach (var item in Collectables)
                    {
                        item.Draw(spriteBatch); // draw the Collectabels at layer 0
                    }

                    foreach (var item in Bullets)
                    {
                        item.Draw(spriteBatch); // draw the Bullets
                    }

                    foreach (var item in Barriers)
                    {
                        item.Draw(spriteBatch); //draw the Barriers at layer 1
                    }
                }

                #endregion


                base.Draw(gameTime);


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

        private void validatePlayer(Character player)
        {
            Logged_In = true;
            playerData = player;

            validation_PlayerMessage = "Player has been validated Gamertag is: " + player._texture.ToString();

            getPlayerData();
        }
        #endregion

       


        
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

    }
}
