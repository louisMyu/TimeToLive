using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
namespace TimeToLive
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        public static int GameWidth;
        public static int GameHeight;
        private ScreenManager m_ScreenManager;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            m_ScreenManager = new ScreenManager(this);
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
            GameWidth = GraphicsDevice.Viewport.Width;
            GameHeight = GraphicsDevice.Viewport.Height;

            //m_World = new World(new Vector2(0, 0));
            //ConvertUnits.SetDisplayUnitToSimUnitRatio(5);

            //Player p = Player.Load(Content);
            //if (p == null)
            //{
            //    Vector2 playerPosition = new Vector2(GameWidth / 2, GameHeight / 2);
            //    m_Player.Init(Content, playerPosition);
            //}
            //else
            //{
            //    m_Player = p;
            //}
            ////init object manager and set objects for it
            //GlobalObjectManager.Init(m_Player, Content, m_World);
            TextureBank.SetContentManager(Content);
            m_ScreenManager.AddScreen(new BackgroundScreen(), null);
            //m_ScreenManager.AddScreen(new MainMenuScreen(), null);
            m_ScreenManager.AddScreen(new CustomMenuScreen(), null);
            m_ScreenManager.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            m_ScreenManager.LoadContent(GraphicsDevice, _spriteBatch);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            m_ScreenManager.UnloadContent();
            ObjectManager.AllGameObjects.Clear();
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            try
            {
                m_ScreenManager.Update(gameTime);
                base.Update(gameTime);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            //switch (CurrentGameState) {
            //    case GameState.Playing:
            //        // TODO: Add your drawing code here
            //        _spriteBatch.Begin();
            //        UserInterface.DrawBackground(_spriteBatch);
            //        GlobalObjectManager.Draw(_spriteBatch);
            //        m_Player.Draw(_spriteBatch);
            //        UserInterface.Draw(_spriteBatch, m_Player);
            //        _spriteBatch.End();
            //        break;
            //    case GameState.Menu:
            //        _spriteBatch.Begin();
            //        m_Menu.Draw(_spriteBatch, m_Player);
            //        _spriteBatch.End();
            //        break;
            //}
            m_ScreenManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
