using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    [DataContract]
    public abstract class Weapon
    {
        public WeaponStats WeaponStatistics;
        public string Name;
        protected SoundEffectInstance m_ShotSound;
        [DataMember]
        public float Knockback { get; set; }
        [IgnoreDataMember]
        protected int m_SightRange;
        [DataMember]
        public int SightRange { get { return m_SightRange; } set { m_SightRange = value; } }
        [DataMember]
        public float LeftAngle
        {
            get;
            set;
        }
        [DataMember]
        public bool Firing
        {
            get;
            set;
        }
        [IgnoreDataMember]
        private float m_Spread;//spread of the bullets
        [DataMember]
        public float Spread
        {
            get
            {
                if (NumberOfBullets <= 1)
                {
                    return 0;
                }
                return m_Spread;
            }
            set
            {
                m_Spread = value;
            }
        }
        [DataMember]
        public int NumberOfBullets
        {
            get;
            set;
        }
        [DataMember]
        //number of frames to wait between the end of one animation to the next
        public int FireRate
        {
            get;
            set;
        }
        //if bullets exist without the weapon firing
        public bool BulletsExist { get; set; }
        protected static Random WEAPON_RANDOM = new Random();
        [DataMember]
        public int m_ElapsedFrames { get; set; }
        [DataMember]
        public bool CanDamage { get; set; }
        public Weapon() 
        {
            m_ElapsedFrames = FireRate;
        }
        public abstract void LoadContent();
        //this should be called every update if it exists for the player
        public virtual void Update(Vector2 playerCenter, Vector2 playerVelocity, float rotationAngle, int accuracy, bool isFireDown, TimeSpan elapsedTime)
        {
            //decrement unless its ready to fire or is being fired
            if (m_ElapsedFrames > 0 && !Firing)
            {
                m_ElapsedFrames -= 1;
            }
        }
        //returns true if enemy died
        public virtual bool CheckCollision(GameObject ob)
        {
            return false;
        }

        public abstract void DrawWeapon(SpriteBatch _spritebatch, Vector2 position, float rot);

        public abstract void DrawBlast(SpriteBatch _spriteBatch, Vector2 position, float rot);
        public virtual bool CanFire()
        {
            if (m_ElapsedFrames == 0)
            {
                return true;
            }
            return false;
        }

        public abstract void LoadWeapon();
        protected abstract void LoadTextures();
        public virtual void ExplodeEnemy(Vector2 intersectingAngle, IEnemy enemy, Vector2 pos)
        {
            List<Texture2D> gibTextures = enemy.GetExplodedParts();
            float shotgunSpreadAngle = 60;
            float singleAngle = (shotgunSpreadAngle / (float)gibTextures.Count);
            float singleAngleRadians = Utilities.DegreesToRadians(singleAngle);
            Vector2 singleAngleVec = Utilities.RadiansToVector2(singleAngleRadians);
            float startingPoint = singleAngle * gibTextures.Count / 2;
            for (int i = 0; i < gibTextures.Count; ++i)
            {
                ExplodedPart gib = new ExplodedPart();
                gib.LoadContent(gibTextures[i], pos);
                Vector2 halfAngle = Utilities.RadiansToVector2(Utilities.DegreesToRadians(-30));
                gib.ApplyLinearForce(intersectingAngle - (halfAngle) + (i * 2 * halfAngle), Knockback * 1.5f);
                //shoul be randomixed
                gib.ApplyTorque(5000f);
                UI.ActiveGibs.Add(gib);
            }
        }
        public abstract void ApplyKickback(Player p);
        public abstract void SetWeaponStats(int level);
        public abstract void UpgradeWeaponLevel();
    }
    public class WeaponStats
    {
        public int WeaponDamage;
        public int WeaponLevel = 0;
    }
}
