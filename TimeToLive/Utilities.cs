using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    class Utilities
    {
        public static float DegreesToRadians(float degrees)
        {
            return degrees *  (float)Math.PI / 180.0f ;
        }

        public static float RadiansToDegrees(float radians)
        {
            return radians * (180.0f) / (float)Math.PI;
        }

        public static bool PointIntersectsRectangle(Vector2 point, Rectangle rec)
        {
            return (point.X >= rec.Left && point.Y >= rec.Top && point.X <= rec.Right && point.Y <= rec.Bottom);
        }

        //The amount of scaling that needs to be applied to a sprite with width and height of baseVec
        //so that it matches the width and height for result
        public static Vector2 GetSpriteScaling(Vector2 result, Vector2 baseVec)
        {
            Vector2 temp = new Vector2();
            temp.X = result.X / baseVec.X;
            temp.Y = result.Y / baseVec.Y;
            return temp;
        }
        public static Vector2 RadiansToVector2(float radians)
        {
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }
        public static float Vector2ToRadians(Vector2 vec)
        {
            return (float)Math.Atan(vec.Y / vec.X);
        }
        public static Vector2 rotateVec2(Vector2 original, float angle)
        {
            float newX = (float)(original.X * Math.Cos(angle) - original.Y * Math.Sin(angle));
            float newY = (float)(original.Y * Math.Sin(angle) + original.Y * Math.Cos(angle));
            return new Vector2(newX, newY);
        }
        public static float NormalizeDegrees(float degrees)
        {
            while (degrees < 0)
            {
                degrees += 360;
            }
            while (degrees > 360)
            {
                degrees -= 360;
            }
            return degrees;
        }
        public static float NormalizeRadians(float radians)
        {
            while (radians < 0)
            {
                radians += 2*(float)Math.PI;
            }
            while (radians > 2 * Math.PI)
            {
                radians -= 2 * (float)Math.PI;
            }
            return radians;
        }
        public static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    #region Upgrade Menu Widget Tree shit
    //0_o
    public class WidgetTree
    {
        private List<WidgetTree> Children;
        private List<Rectangle> HitableObjects;
        private Dictionary<Rectangle, DrawableArea> DrawableAreas;
        private Rectangle BaseContainer;
        public WidgetTree(Rectangle baseArea)
        {
            Children = null;
            HitableObjects = new List<Rectangle>();
            DrawableAreas = new Dictionary<Rectangle, DrawableArea>();
            BaseContainer = baseArea;
        }
        public void AddHitArea(Rectangle rec)
        {

            HitableObjects.Add(rec);
        }
        public void AddDrawArea(Rectangle area, DrawableArea tex)
        {
            DrawableAreas.Add(area, tex);
        }
        public void UpdatePositions(Vector2 delta)
        {
            BaseContainer.X += (int)delta.X;
            BaseContainer.Y += (int)delta.Y;
            for (int x = 0; x < HitableObjects.Count; ++x)
            {
                Rectangle rec = HitableObjects[x];
                rec.X += (int)delta.X;
                rec.Y += (int)delta.Y;
                HitableObjects[x] = rec;
            }
            if (Children != null)
            {
                foreach (WidgetTree child in Children)
                {
                    child.UpdatePositions(delta);
                }
            }
        }
        public void AddWidgetTree(WidgetTree widgetTree)
        {
            if (Children == null)
            {
                Children = new List<WidgetTree>();
            }
            Children.Add(widgetTree);
        }
        //returns an empty rectangle on false
        public Rectangle CheckCollision(Point p)
        {
            if (Children != null)
            {
                foreach (WidgetTree tree in Children)
                {
                    Rectangle temp;
                    temp = tree.CheckCollision(p);
                    if (temp.Width > 0)
                    {
                        return temp;
                    }
                }
            }
            foreach (Rectangle rec in HitableObjects)
            {
                Rectangle trueRec = new Rectangle(rec.X - (rec.Width/2) + BaseContainer.X, rec.Y - (rec.Height/2) + BaseContainer.Y, rec.Width, rec.Height);
                if (trueRec.Contains(p))
                {
                    return rec;
                }
            }
            return new Rectangle();
        }
        //this color parameter needs to be removed in the future
        public void StartDrawWidgets(SpriteBatch _spriteBatch, Rectangle where)
        {
            Queue<WidgetTree> queue = new Queue<WidgetTree>();
            queue.Enqueue(this);
            while (queue.Count > 0) {
                WidgetTree child = queue.Dequeue();
                child.DrawWidgets(_spriteBatch, where);
                if (child.Children != null) 
                {
                    foreach (WidgetTree widgetTree in child.Children)
                    {
                        queue.Enqueue(widgetTree);
                    }
                }
            }
        }
        //TODO: Remove this color parameter
        private void DrawWidgets(SpriteBatch _spriteBatch, Rectangle where)
        {
            Rectangle temp = new Rectangle();
            foreach (KeyValuePair<Rectangle, DrawableArea> entry in DrawableAreas)
            {
                temp = entry.Key;
                temp.X += (BaseContainer.X + where.X);
                temp.Y += (BaseContainer.Y + where.Y);
                entry.Value.Draw(_spriteBatch, temp);
            }
        }
    }
    public class ColorString : DrawableArea
    {
        public SpriteFont Font;
        public string Text;
        public Color Color;
        public ColorString(SpriteFont f, string t, Color c)
        {
            Font = f;
            Text = t;
            Color = c;
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle pos)
        {
            Vector2 measuredString = Font.MeasureString(Text);
            Vector2 stringCenter = new Vector2(measuredString.X / 2, measuredString.Y / 2);
            spriteBatch.DrawString(Font, Text, new Vector2(pos.X, pos.Y), Color, Utilities.DegreesToRadians(90f), stringCenter, new Vector2(1,1),
                                    SpriteEffects.None, 0);
        }
        public void SetText(string s)
        {
            Text = s;
        }
    }
    public class ColorTexture : DrawableArea
    {
        public Texture2D Texture;
        public Color Color;
        public ColorTexture(Texture2D tex, Color c)
        {
            Texture = tex;
            Color = c;
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle rec)
        {
            spriteBatch.Draw(Texture, rec, null, Color, Utilities.DegreesToRadians(90f), new Vector2((Texture.Width / 2), (Texture.Height / 2)),
                                    SpriteEffects.None, 0);
        }
    }
    public interface DrawableArea
    {
        void Draw(SpriteBatch sprite, Rectangle position);
    }
#endregion
}
