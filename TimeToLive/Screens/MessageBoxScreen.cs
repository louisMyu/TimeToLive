#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace TimeToLive
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class MessageBoxScreen : GameScreen
    {
        #region Fields

        string message;
        Texture2D gradientTexture;
        Rectangle OKButton;
        Rectangle CancelButton;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message)
            : this(message, true)
        { }


        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message, bool includeUsageText)
        {
            const string usageText = "Are you sure?";
            
            if (includeUsageText)
                this.message = message + usageText;
            else
                this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            OKButton = new Rectangle();
            CancelButton = new Rectangle();
            EnabledGestures = GestureType.Tap;
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>("GSMgradient");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(Input input)
        {
            ////We pass in our ControllingPlayer, which may either be null (to
            ////accept input from any player) or a specific index. If we pass a null
            ////controlling player, the InputState helper returns to us which player
            ////actually provided the input. We pass that through to our Accepted and
            ////Cancelled events, so they can tell which player triggered them.
            //if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            //{
            //    // Raise the accepted event, then exit the message box.
            //    if (Accepted != null)
            //        Accepted(this, new PlayerIndexEventArgs(playerIndex));

            //    ExitScreen();
            //}
            //else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            //{
            //    // Raise the cancelled event, then exit the message box.
            //    if (Cancelled != null)
            //        Cancelled(this, new PlayerIndexEventArgs(playerIndex));

            //    ExitScreen();
            //}
            //Rectangle rotatedOK = new Rectangle(OKButton.X, OKButton.Y, OKButton.Height, OKButton.Width);
            //Rectangle rotatedCancel = new Rectangle(CancelButton.X, CancelButton.Y, CancelButton.Height, CancelButton.Width);
            TouchCollection state = input.TouchState;
            foreach (TouchLocation touch in state)
            {
                if (OKButton.Contains(touch.Position.X, touch.Position.Y))
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {

                    }
                    else if (touch.State == TouchLocationState.Released)
                    {
                        Accepted(this, new EventArgs());
                        ExitScreen();
                    }
                }
                if (CancelButton.Contains(touch.Position.X, touch.Position.Y))
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                    }
                    else if (touch.State == TouchLocationState.Released)
                    {
                        Cancelled(this, new EventArgs());
                        ExitScreen();
                    }
                }
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 40;
            const int vPad = 32;

            //Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
            //                                              (int)textPosition.Y - vPad,
            //                                              (int)textSize.X + hPad * 2,
            //                                              (int)textSize.Y + vPad * 2);
            Rectangle backgroundRectangle = new Rectangle((int)(viewportSize.X / 2 - textSize.Y / 2),
                                                    (int)(viewportSize.Y / 2 - textSize.X / 2), 
                                                    (int)textSize.Y + vPad * 2, 
                                                    (int)textSize.X + hPad * 2);
            Vector2 OKTextSize = font.MeasureString("Ok");
            Vector2 CancelTextSize = font.MeasureString("Cancel");
            OKButton = new Rectangle((int)backgroundRectangle.X + 5, (int)backgroundRectangle.Y + (backgroundRectangle.Height/2) - 100, (int)OKTextSize.Y + 5 * 2, (int)OKTextSize.X + 25 * 2);
            CancelButton = new Rectangle((int)OKButton.X, (int)OKButton.Y + OKButton.Height + 75, (int)50, (int)100);
            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);
            spriteBatch.Draw(gradientTexture, OKButton, Color.Green);
            spriteBatch.Draw(gradientTexture, CancelButton, Color.Red);
            // Draw the message box text.
            Vector2 measuredString = font.MeasureString(message);
            Vector2 stringCenter = new Vector2(measuredString.X / 2, measuredString.Y / 2);
            spriteBatch.DrawString(font, message, new Vector2(backgroundRectangle.X + backgroundRectangle.Width - textSize.Y/2, backgroundRectangle.Y + backgroundRectangle.Height/2), color, Utilities.DegreesToRadians(90f), stringCenter, new Vector2(1, 1),
                                    SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "OK", new Vector2(OKButton.X + OKButton.Width/2, OKButton.Y+OKButton.Height/2), color, Utilities.DegreesToRadians(90f), OKTextSize/2, new Vector2(1, 1),
                                    SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Cancel", new Vector2(CancelButton.X+CancelButton.Width/2, CancelButton.Y+CancelButton.Height/2), color, Utilities.DegreesToRadians(90f), CancelTextSize/2, new Vector2(1, 1),
                                    SpriteEffects.None, 0);
            spriteBatch.End();
        }


        #endregion
    }
}
