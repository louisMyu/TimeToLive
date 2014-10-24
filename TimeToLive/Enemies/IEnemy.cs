using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    public interface IEnemy
    {
        int GetHealth();
        void AddToHealth(int amount);
        void ApplyLinearForce(Vector2 angle, float amount);
        void CleanBody();
        TimeSpan GetDamageAmount();
        List<Texture2D> GetExplodedParts();
        //take care of any collision logic that the enemy needs to do, like exploding
        void DoCollision(Player p);
        //this should create and add the item to the object manager
        void DropItem(); 
    }
}
