
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TimeToLive
{
    class CustomMenuEntry : MenuEntry
    {
        public Rectangle Bounds;
        public static Texture2D testTexture;
        public CustomMenuEntry(string text)
            : base(text)
        {

        }
        public void SetBounds(Rectangle rec)
        {
            Bounds = rec;
            if (testTexture == null)
            {
                testTexture = TextureBank.GetTexture("blank");
            }
        }
        public override int GetHeight()
        {
            return Bounds.Width;
        }
        public override int GetWidth()
        {
            return Bounds.Height;
        }
        public override void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // Modify the alpha to fade text out during transitions.
            Color color = Color.White;
            color *= screen.TransitionAlpha;

            // Draw text, centered on the middle of each line.
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            Vector2 textSize = font.MeasureString(text);
            Vector2 origin = textSize / 2;
            spriteBatch.Draw(testTexture, Bounds, Color.Black);
            spriteBatch.DrawString(font, text, position, color, 0,
                                   origin, 1.0f, SpriteEffects.None, 0);
            
        }
    }
}
