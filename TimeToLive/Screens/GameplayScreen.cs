#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using TimeToLive;
using FarseerPhysics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
#endregion

namespace TimeToLive
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        public enum GameState
        {
            Countdown,
            Playing,
            Dying,
            MainScreen
        }
        #region Fields
        
        ContentManager content;
        private PhysicsManager m_PhysicsManager;
        public Player m_Player;
        public ObjectManager GlobalObjectManager;
        private UI UserInterface = new UI();
        public bool SlowMotion = false;
        private TouchCollection TouchesCollected;
        private bool isLoaded;
        private bool isUpdated;
        private bool isPaused = false;
        private TimeSpan m_CountdownTime;
        Song m_song;
        Random random = new Random();
        private GameState m_GameState;
        private bool songPlaying = false;
        RenderTarget2D backgroundTexture;
        private bool m_BackGroundDrawnOnce = false;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(GameState state)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            GlobalObjectManager = new ObjectManager();
            isLoaded = false;
            isUpdated = false;
            m_GameState = state;
            m_CountdownTime = TimeSpan.FromSeconds(5.5);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = ScreenManager.Game.Content;

            //gameFont = content.Load<SpriteFont>("GSMgamefont");

            m_PhysicsManager = new PhysicsManager();
            m_PhysicsManager.Init();
            ConvertUnits.SetDisplayUnitToSimUnitRatio(5);

             Vector2 playerPosition = new Vector2(Game1.GameWidth / 2, Game1.GameHeight / 2);
             m_Player = new Player(m_PhysicsManager);
            m_Player.Init(content, playerPosition);

            //init object manager and set objects for it
            GlobalObjectManager.Init(m_Player, content, m_PhysicsManager);
            SoundBank.SetContentManager(content);
            m_Player.LoadContent(m_PhysicsManager);
            UserInterface.LoadContent(content, Game1.GameWidth, Game1.GameHeight);
            GlobalObjectManager.LoadContent();

            m_song = SoundBank.GetSong("PUNCHING-Edit");
            UserInterface.SetTimeToDeath(m_Player.TimeToDeath);

            Zombie.LoadTextures();
            Slime.LoadTextures();
            Anubis.LoadTextures();

            
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            backgroundTexture = new RenderTarget2D(ScreenManager.GraphicsDevice, (int)ScreenManager.NativeResolution.X, (int)ScreenManager.NativeResolution.Y, false,
                                            SurfaceFormat.Color, DepthFormat.None, ScreenManager.GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            m_PhysicsManager.Update(0f);

            isLoaded = true;
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            //content.Unload();
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (ScreenState == ScreenState.Active && isPaused)
            {
                isPaused = false;
            }
            if (IsActive || m_GameState == GameState.MainScreen)
            {
                TimeSpan customElapsedTime = gameTime.ElapsedGameTime;
                if (isPaused)
                {
                    return;
                }
                switch (m_GameState)
                {
                    case GameState.MainScreen:
                        if (!songPlaying)
                        {

                        }
                        //put some wandering enemies here
                        break;
                    case GameState.Countdown:
                        if (!songPlaying)
                        {
                            songPlaying = true;
                            MediaPlayer.Play(m_song);
                            MediaPlayer.IsRepeating = true;
                        }
                        m_CountdownTime -= customElapsedTime;
                        if (m_CountdownTime < TimeSpan.FromSeconds(1))
                        {
                            m_GameState = GameState.Playing;
                        }
                        UserInterface.Update(customElapsedTime);
                        break;
                    case GameState.Playing:
                        //if (SlowMotion)
                        //{
                        //    customElapsedTime = new TimeSpan((long)(customElapsedTime.Ticks * 0.5));
                        //}
                        if (m_Player.TimeToDeath < TimeSpan.FromSeconds(0))
                        {
                            //SlowMotion = true;
                            //ResetGame();
                            m_Player.TimeToDeath = TimeSpan.FromTicks(0);
                            UserInterface.SetTimeToDeath(m_Player.TimeToDeath);
                            m_GameState = GameState.Dying;
                            m_Player.SetPlayerToDyingState();
                            return;
                        }
                        m_Player.TimeToDeath -= gameTime.ElapsedGameTime;
                        // TODO: Add your update logic here
                        UserInterface.ProcessInput(m_Player, TouchesCollected);
                        UserInterface.Update(customElapsedTime);
                        UserInterface.SetTimeToDeath(m_Player.TimeToDeath);
                        //check if a game reset or zombie hit and save state and do the action here,
                        //so that the game will draw the zombie intersecting the player
                        GlobalObjectManager.Update(m_Player, customElapsedTime);
                        m_Player.Update(customElapsedTime);
                        m_Player.CheckCollisions(UserInterface);

                        m_Player.CheckWeaponHits();
                        //cleanup dead objects
                        GlobalObjectManager.CleanUpDeadObjects(customElapsedTime);

                        m_PhysicsManager.Update((float)1.0f / 60f);
                        break;
                    case GameState.Dying:
                        if (m_Player.isDead)
                        {
                            MediaPlayer.Stop();
                            PushDeathScreen();
                            return;
                        }
                        m_Player.Update(customElapsedTime);
                        break;
                }
                isUpdated = true;

            }
        }
        private void ResetGame()
        {
            GlobalObjectManager.ResetGame();
            m_CountdownTime = TimeSpan.FromSeconds(5.5);
            m_GameState = GameState.Countdown;
            m_Player.ResetPlayer();
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(Input input)
        {
            if (input.IsNewKeyPress(Buttons.Back))
            {
                isPaused = true;
                //ScreenManager.Game.Exit();
                //return;
                //this should actually create a menu overlay with the game underneathe
                //this should involve adding a new menu screen for the pause
                ScreenManager.AddScreen(new PauseMenu(), null);
            }
            TouchesCollected = input.TouchState;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime, Matrix scale)
        {
            SpriteBatch _spriteBatch = ScreenManager.SpriteBatch;

            //make sure the game has loaded and has updated at least one frame
            if (!isLoaded || !isUpdated)
            {
                return;
            }
            
            switch (m_GameState)
            {
                case GameState.Dying:
                case GameState.Playing:
                    ScreenManager.GraphicsDevice.SetRenderTarget(backgroundTexture);
                    _spriteBatch.Begin();
                    UserInterface.DrawBakedGibs(_spriteBatch, m_PhysicsManager);
                    _spriteBatch.End();
                    ScreenManager.GraphicsDevice.SetRenderTarget(null);
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                                            null, scale);
                    _spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), UserInterface.BackGroundHueColor);
                    _spriteBatch.End();
                    if (UserInterface.TimeAlmostOut)
                    {
                        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                                            UserInterface.m_SkullLeftEyePointLight, scale);
                        UserInterface.DrawSkullBackground(_spriteBatch);
                        _spriteBatch.End();
                    }
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                                            null, scale);
                    GlobalObjectManager.DrawSlimeTrails(_spriteBatch);
                    GlobalObjectManager.DrawPowerUps(_spriteBatch);
                    UserInterface.DrawActiveGibs(_spriteBatch);
                    GlobalObjectManager.Draw(_spriteBatch);
                    m_Player.Draw(_spriteBatch);
                    GlobalObjectManager.DrawExplosions(_spriteBatch);
                    UserInterface.DrawStatusTexts(_spriteBatch);
                    UserInterface.DrawDeathTimer(_spriteBatch);
                    _spriteBatch.End();
                    break;
                case GameState.Countdown:
                    if (!m_BackGroundDrawnOnce)
                    {
                        ScreenManager.GraphicsDevice.SetRenderTarget(backgroundTexture);
                        _spriteBatch.Begin();
                        UserInterface.DrawBackground(_spriteBatch);
                        UserInterface.DrawBakedGibs(_spriteBatch, m_PhysicsManager);
                        _spriteBatch.End();
                        ScreenManager.GraphicsDevice.SetRenderTarget(null);
                        m_BackGroundDrawnOnce = true;
                    }
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                        null, scale);
                    _spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), UserInterface.BackGroundHueColor);
                    //GlobalObjectManager.Draw(_spriteBatch);
                    //m_Player.Draw(_spriteBatch);
                    UserInterface.Draw(_spriteBatch, m_Player);
                    UserInterface.DrawCountdown(_spriteBatch, m_CountdownTime);
                    _spriteBatch.End();
                    break;
                case GameState.MainScreen:
                    _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                        null, scale);
                    _spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), Color.White);
                    _spriteBatch.End();
                    break;
            }
            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 && !isPaused)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }
        //push the death screen
        private void PushDeathScreen()
        {
            isPaused = true;
            DeathMenu deathMenu = new DeathMenu(UnPauseGame);
            ScreenManager.AddScreen(deathMenu, null);
        }
        private void UnPauseGame()
        {
            m_GameState  = GameState.MainScreen;
            isPaused = false;
        }
        private void PauseGame()
        {
            isPaused = true;
        }
        #endregion
    }
}
