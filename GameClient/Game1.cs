using System;
using GameData;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using TextEffects;

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
        private string message;
        private PlayerData playerData;
        Vector2 worldCoords;

        public SpriteFont GameFont { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
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
            }
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
            // all other messages from Server go here
        }

        private void clientRecieveGameCount(double obj)
        {
            throw new NotImplementedException();
        }

        private void clientRecieveStartCount(double obj)
        {
            throw new NotImplementedException();
        }

        private void clientRecievePlayer(PlayerData obj)
        {
            throw new NotImplementedException();
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
            Joined = true;
            // Setup Player
            SetupPlayer();
            proxy.Invoke("getPlayer", new object[] { "Sarah", "Treanor" });

        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameFont = Content.Load<SpriteFont>("GameFont");
            Services.AddService<SpriteFont>(GameFont);
            Services.AddService<SpriteBatch>(spriteBatch);
            new FadeTextManager(this);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (!connected) return;
                
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
