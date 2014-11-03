using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    class GraphicalLine
    {
        private Texture2D blank;
        private Vector2 point1;
        private Vector2 point2;

        public GraphicalLine() 
        {
            blank = TextureBank.GetTexture("Line");
            point1 = new Vector2();
            point2 = new Vector2();
        }
        public void Update(Vector2 p1, Vector2 p2)
        {
            point1.X = p1.X;
            point1.Y = p1.Y;
            point2.X = p2.X;
            point2.Y = p2.Y;
        }

        public void DrawLine(SpriteBatch batch,
                float width, Color color)
        {
            float angle = (float)Math.Atan2(point1.Y - point2.Y, point1.X - point2.X);
            float length = Vector2.Distance(point1, point2);
            batch.Draw(blank, point1, null, color,
                angle, new Vector2(blank.Width, blank.Height), new Vector2(length/ blank.Height, width),
                SpriteEffects.None, 0);
        }
    }
}
