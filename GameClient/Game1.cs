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

namespace GameClient
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        IHubProxy proxy;
        HubConnection connection;
        private bool connected;
        private bool joined;

        private PlayerData playerData;
        Player player;
        Vector2 worldCoords;
        private Rectangle worldRect;
        private FollowCamera followCamera;

        private string message;
        private string errorMessage;
        private string timerMessage="";
        private string GameTimerMessage="";

        static string name;

        Texture2D collectable;
        private bool Connected;
        public SpriteFont GameFont { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

      
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            connection = new HubConnection("http://localhost:5864/");
            proxy = connection.CreateHubProxy("GameHub");
            message = "Connecting............";
            connection.StateChanged += Connection_StateChanged;
            connection.Start();
            base.Initialize();
        }

        private void Connection_StateChanged(StateChange state)
        {
            switch(state.NewState)
            {
                case ConnectionState.Connected:
                    connected = true;
                    message = "Connected to Game Hub ";
                    subscribeToMessages();
                    getPlayerData();
                    getCollectableData();
                    break;

                case ConnectionState.Disconnected:
                    message = "Disconnected.....";
                    if (state.OldState == ConnectionState.Connected)
                        message = "Lost Connection....";
                    Connected = false;
                    break;

                case ConnectionState.Connecting:
                    message = "Connecting.....";
                    Connected = false;
                    break;
            
             
                    break;
                default:
                    Console.WriteLine("{0}", state.NewState);
                    break;

            }
        }

        private void clientChat()
        {
            Action<string, string> SendMessageRecieved = recieved_a_message;
            proxy.On("broadcastMessage", SendMessageRecieved);

            Action<int, int> RecieveInts = recieve_ints;
            proxy.On("newPosition", RecieveInts);

            connection.Start().Wait();
            // 
            Console.Write("Enter your Name: ");
            name = Console.ReadLine();

            proxy.Invoke("Send", new object[] { name, "Has joined" });
            Random r = new Random();

            proxy.Invoke("SendNewPosition", new object[] { r.Next(0, 200), r.Next(0, 400) });

            Console.ReadKey();
            connection.Stop();
        }

        private static void recieve_ints(int x, int y)
        {
            Console.WriteLine("X: {0}, Y: {0}", x, y);
        }

        private static void recieved_a_message(string sender, string message)
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
            proxy.On("error", err);

            Action<int, int> joined = cJoined;
            proxy.On("joined", joined);

            Action<PlayerData> recievePlayer = clientRecievePlayer;
            proxy.On("recievePlayer", recievePlayer);

            Action<double> recieveCountDown = clientRecieveStartCount;
            proxy.On("recieveCountDown", recieveCountDown);

            Action<double> recieveGameCountdown = clientRecieveGameCount;
            proxy.On("recieveGameCount", recieveGameCountdown);

            proxy.Invoke("join");
            // all other messages from Server go here
        }

        private void clientRecieveGameCount(double count)
        {
            GameTimerMessage = "Game over in: " + count.ToString();
        }

        private void clientRecieveStartCount(double count)
        {
            timerMessage = "Time to start" + count.ToString();
        }

        private void clientRecievePlayer(PlayerData obj)
        {
            if (player != null)
            {
                player.PlayerInfo = playerData;
            }
        }
        #region Action delegates for incoming server messages
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
            collectable = Content.Load<Texture2D>("collectable");
            Services.AddService<SpriteFont>(GameFont);
            Services.AddService<SpriteBatch>(spriteBatch);


            new FadeTextManager(this);

            // TODO: use this.Content to load your game content here
        }

     
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

       
        protected override void Update(GameTime gameTime)
        {
            

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (!connected) return;

            if (player != null)
            {
                player.Update(gameTime);
                player.Position = Vector2.Clamp(player.Position,
                    Vector2.Zero,
                    (worldCoords - new Vector2(player.SpriteWidth, player.SpriteHeight)));
                followCamera.Follow(player);
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.DrawString(GameFont,
                message,
                new Vector2(200, 20), Color.White
                );
            if (playerData != null )
            {
                spriteBatch.DrawString(GameFont,
                    playerData.FirstName + " is ok ",
                    new Vector2(20, 20), Color.White
                    );

                
                spriteBatch.DrawString(GameFont, timerMessage, new Vector2(20, 50), Color.Red);              
                spriteBatch.DrawString(GameFont, GameTimerMessage, new Vector2(20, GraphicsDevice.Viewport.Width / 2), Color.White);
              

               // spriteBatch.Begin();
                spriteBatch.DrawString(GameFont, timerMessage, new Vector2(20, 20), Color.Red);
               // spriteBatch.End();
                //
                //spriteBatch.Begin();
                spriteBatch.DrawString(GameFont, GameTimerMessage, new Vector2(20, GraphicsDevice.Viewport.Width / 2), Color.White);
               // spriteBatch.End();

            }
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void getPlayerData()
        {
            proxy.Invoke<PlayerData>("getPlayer", 
                                new string[] { "popple" })
                .ContinueWith(t => {
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
                .ContinueWith(t => {
                    CreateGameCollecables(t.Result);
                });
        }

        private void CreateGameCollecables(List<CollectableData> result)
        {
            
            foreach (CollectableData c in result)
            {
                new FadeText(this, Vector2.Zero, 
                    "Delivered " + c.CollectableName + 
                        " X: " + c.X.ToString() + " Y: " + c.X.ToString());
            }
        }
    }
}
