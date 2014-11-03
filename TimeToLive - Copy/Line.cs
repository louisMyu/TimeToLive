using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    

    public class Line
    {
        public Vector2 P1 = new Vector2();
        public Vector2 P2 = new Vector2();
        private GraphicalLine m_WeaponLine;

        //i should remove the graphics device later, or 
        //separate the graphical line from the line class
        public Line()
        {
            m_WeaponLine = new GraphicalLine();
        }
        public Line(Vector2 one, Vector2 two)
        {
            P1 = one;
            P2 = two;
        }

        public void Update(Vector2 playerCenter, float leftAngle, int weaponLength)
        {
            UpdateFromRotation(playerCenter, leftAngle, weaponLength);
            m_WeaponLine.Update(P1, P2);
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            m_WeaponLine.DrawLine(_spriteBatch, 0.1f, Color.Black);
        }
        //checks if there is an intersection and if there is will return
        //the point that is closest is P1
        //else return a -1 -1 Vector2
        public Vector2 Intersects(Rectangle rec)
        {
            Vector2 result = new Vector2(-1, -1);
            List<Vector2> pointsOfIntersection = new List<Vector2>();

            pointsOfIntersection.Add(this.Intersects(new Line(new Vector2(rec.Left, rec.Bottom), new Vector2(rec.Right, rec.Bottom))));
            pointsOfIntersection.Add(this.Intersects(new Line(new Vector2(rec.Right, rec.Bottom), new Vector2(rec.Right, rec.Top))));
            pointsOfIntersection.Add(this.Intersects(new Line(new Vector2(rec.Right, rec.Top), new Vector2(rec.Left, rec.Top))));
            pointsOfIntersection.Add(this.Intersects(new Line(new Vector2(rec.Left, rec.Top), new Vector2(rec.Left, rec.Bottom))));
            
            float smallestLength = float.MaxValue;
            foreach (Vector2 point in pointsOfIntersection)
            {
                if (point.X != -1 && point.Y != -1)
                {
                    float length = Vector2.Distance(this.P1, point);
                    if (length < smallestLength)
                    {
                        smallestLength = length;
                        result = point;
                    }
                }
            }
            return result;
        }

        //checks if there is an intersection and if there is will return
        //the point that is closest is P1
        //else return a -1 -1 Vector2
        //http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        public Vector2 Intersects(Line line)
        {
            Vector2 result = new Vector2(-1, -1);

            float s1_x, s1_y, s2_x, s2_y;
            s1_x = P1.X - P2.X; s1_y = P1.Y - P2.Y;
            s2_x = line.P1.X - line.P2.X; s2_y = line.P1.Y - line.P2.Y;

            float s, t;
            s = (-s1_y * (P2.X - line.P2.X) + s1_x * (P2.Y - line.P2.Y)) / (-s2_x * s1_y + s1_x * s2_y);
            t = (s2_x * (P2.Y - line.P2.Y) - s2_y * (P2.X - line.P2.X)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                result.X = P2.X + (t * s1_x);
                result.Y = P2.Y + (t * s1_y);
            }
            return result;
        }

        public void UpdateFromRotation(Vector2 p1, float rotation, float length)
        {
            P1.X = p1.X;
            P1.Y = p1.Y;

            //get the second point of the line from the rotation angle of the player
            P2.X = (float)(p1.X + (Math.Cos(rotation) * length));
            P2.Y = (float)(p1.Y + (Math.Sin(rotation) * length));
        }
    }
}
