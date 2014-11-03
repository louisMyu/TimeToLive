using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    //public struct Line
    //{
    //    public Vector2 P1 = new Vector2();
    //    public Vector2 P2 = new Vector2();

    //    public Line(Vector2 one, Vector2 two)
    //    {
    //        P1 = one;
    //        P2 = two;
    //    }
    //    //checks if there is an intersection and if there is will return
    //    //the point that is closest is P1
    //    //else return a -1 -1 Vector2
    //    //public Vector2 Intersects(Rectangle rec)
    //    //{
    //    //    Vector2 result = new Vector2(-1, -1);
    //    //    float s1_x, s1_y, s2_x, s2_y;

    //    //    s1_x = P1.X - P2.X; s1_y = P1.Y - P2.Y;
    //    //    s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;

    //    //    return result;

    //    //}

    //    //checks if there is an intersection and if there is will return
    //    //the point that is closest is P1
    //    //else return a -1 -1 Vector2
    //    //http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
    //    public Vector2 Intersects(Line line)
    //    {
    //        Line l = new Line(new Vector2(0, 0), new Vector2(4, 4));
    //        line = l;

    //        Vector2 result = new Vector2(-1, -1);

    //        float s1_x, s1_y, s2_x, s2_y;
    //        s1_x = P1.X - P2.X; s1_y = P1.Y - P2.Y;
    //        s2_x = line.P1.X - line.P2.X; s2_y = line.P1.Y - line.P2.Y;

    //        float s, t;
    //        s = (-s1_y * (P2.X - line.P2.X) + s1_x * (P2.Y - line.P2.Y)) / (-s2_x * s1_y + s1_x * s2_y);
    //        t = (s2_x * (P2.Y - line.P2.Y) - s2_y * (P2.X - line.P2.X)) / (-s2_x * s1_y + s1_x * s2_y);

    //        if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
    //        {
    //            result.X = P2.X + (t * s1_x);
    //            result.Y = P2.Y + (t * s1_y);
    //        }
    //            return result;
    //    }
    //}
}
