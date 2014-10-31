#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace TimeToLive
{
    class CustomMenuScreen : MenuScreen
    {
        #region Initialization

        private CustomMenuEntry gameMenuEntry;
        private CustomMenuEntry optionMenuEntry;
        private bool loaded = false;
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public CustomMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
         
            gameMenuEntry = new CustomMenuEntry("Play Game");
            optionMenuEntry = new CustomMenuEntry("Options");

            // Hook up menu event handlers.
            gameMenuEntry.Selected += PlayGameMenuEntrySelected;
            optionMenuEntry.Selected += OptionsMenuEntrySelected;

            IsPopup = true;
            menuEntries.Add(gameMenuEntry);
            menuEntries.Add(optionMenuEntry);
        }
        public void LoadBounds()
        {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            Rectangle playGameRec = new Rectangle((int)(ScreenManager.NativeResolution.X * 0.1), (int)(ScreenManager.NativeResolution.Y - ScreenManager.NativeResolution.Y * .1 - 50), 200, 50);
            Rectangle optionRec = new Rectangle((int)(ScreenManager.NativeResolution.X - ScreenManager.NativeResolution.X * 0.1 - 200), (int)(ScreenManager.NativeResolution.Y - ScreenManager.NativeResolution.Y * .1 - 50), 200, 50);
            gameMenuEntry.SetBounds(playGameRec, ScreenManager.ResolutionTransformationMaxtrix);
            optionMenuEntry.SetBounds(optionRec, ScreenManager.ResolutionTransformationMaxtrix);
        }

        #endregion

        #region Handle Input

        protected override Rectangle GetMenuEntryHitBounds(MenuEntry entry)
        {
            if (entry is CustomMenuEntry)
            {
                //we should return the scaled values to represent actual pixels on the screen, instead of the native representation
                //that is used throughout the code
                return ((CustomMenuEntry)entry).RealBounds;
            }
            return new Rectangle();
        }
        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen(GameplayScreen.GameState.Countdown));
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            //app needs to exit here
        }
        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel()
        {
            //app needs to exit here
        }
        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected override void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            //Vector2 position = new Vector2(0f, 175f);
            Vector2 playGamePosition = new Vector2(gameMenuEntry.NativeBounds.X, gameMenuEntry.NativeBounds.Y);
            playGamePosition.X += gameMenuEntry.NativeBounds.Width / 2;
            playGamePosition.Y += gameMenuEntry.NativeBounds.Height / 2;
            Vector2 optionsPosition = new Vector2(optionMenuEntry.NativeBounds.X, optionMenuEntry.NativeBounds.Y);
            optionsPosition.X += optionMenuEntry.NativeBounds.Width / 2;
            optionsPosition.Y += optionMenuEntry.NativeBounds.Height / 2;

            if (ScreenState == ScreenState.TransitionOn)
                playGamePosition.X -= transitionOffset * 256;
            else
                playGamePosition.X -= transitionOffset * 512;

            // set the entry's position
            gameMenuEntry.Position = playGamePosition;

            if (ScreenState == ScreenState.TransitionOn)
                optionsPosition.X += transitionOffset * 256;
            else
                optionsPosition.X += transitionOffset * 512;
            optionMenuEntry.Position = optionsPosition;
        }
        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime, Matrix scale)
        {
            if (!loaded)
            {
                LoadBounds();
            }
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                                            null, scale);

            // Draw each menu entry in turn.

            bool isSelected = IsActive;

            gameMenuEntry.Draw(this, isSelected, gameTime);
            optionMenuEntry.Draw(this, isSelected, gameTime);
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(ScreenManager.NativeResolution.X / 2, ScreenManager.NativeResolution.Y / 2);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }
        #endregion
    }
}
