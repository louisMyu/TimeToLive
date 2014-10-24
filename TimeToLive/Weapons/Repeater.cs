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
    class Repeater : Weapon
    {
        [IgnoreDataMember]
        private SpriteInfo m_SavedShotInfo;
        [IgnoreDataMember]
        private SpriteInfo m_CurrentShotInfo;

        [DataMember]
        public SpriteInfo SavedShotInfo { get { return m_SavedShotInfo; } set { m_SavedShotInfo = value; } }
        [DataMember]
        public SpriteInfo CurrentShotInfo { get { return m_CurrentShotInfo; } set { m_CurrentShotInfo = value; } }

        private List<Bullet> m_Bullets = new List<Bullet>();
        public Repeater() : base()
        {
            Name = "Plasma";
            NumberOfBullets = 1;
            FireRate = 5;
            m_SightRange = 400;
            Knockback = 250f;
            Firing = false;
        }

        public override void LoadContent()
        {
            LoadTextures();
        }
        //foreach line of the shotgun i need to update the lines based on the player center,
        //and rotate it and give it length, then update the graphical lines
        public override void Update(Vector2 playerCenter, Vector2 playerVelocity, float rotationAngle, int accuracy, bool shotFired, TimeSpan elapsedTime)
        {
            base.Update(playerCenter, playerVelocity, rotationAngle, accuracy, shotFired, elapsedTime);
            //float accuracyInRadians = WEAPON_RANDOM.Next(0, accuracy) * ((float)Math.PI / 180);
            //TODO: add a random so its either plus or minus accuracy
            float centerVector = rotationAngle;

            m_CurrentShotInfo = new SpriteInfo(playerCenter, playerVelocity, rotationAngle, NumberOfBullets, LeftAngle);
            
            m_Bullets.RemoveAll(x => x.CanDelete);
            foreach (Bullet b in m_Bullets)
            {
                b.Update(elapsedTime);
            }
            //firing a shot, save the state
            if (shotFired && CanFire())
            {
                Bullet temp = new Bullet(m_CurrentShotInfo, 40);
                temp.LoadContent();
                m_Bullets.Add(temp);
                m_ElapsedFrames = FireRate;

                if (m_ShotSound != null)
                {
                    m_ShotSound.Stop();
                    m_ShotSound.Dispose();
                }
                //m_ShotSound = SoundBank.GetSoundInstance("SoundRepeaterShot");
                //m_ShotSound.Play();
            }
            if (m_Bullets.Count > 0)
            {
                BulletsExist = true;
            }
            else
            {
                BulletsExist = false;
            }
        }
        //returns true if enemy died
        public override bool CheckCollision(GameObject ob)
        {
            bool hit = false;

            foreach (Bullet b in m_Bullets)
            {
                hit = b.CheckCollision(ob);
                if (hit)
                {
                    b.CanDelete = true;
                    IEnemy enemy;
                    if ((enemy = ob as IEnemy) != null)
                    {
                        enemy.AddToHealth(-10);
                        if (enemy.GetHealth() <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override void ApplyKickback(Player p)
        {
            
        }
        public override void DrawWeapon(SpriteBatch _spriteBatch, Vector2 position, float rot)
        {

        }

        public override void DrawBlast(SpriteBatch _spriteBatch, Vector2 position, float rot)
        {
            foreach (Bullet b in m_Bullets)
            {
                b.Draw(_spriteBatch);
            }
            //if (m_FireAnimation.CanDraw())
            //{
            //}
            //else if (Firing)
            //{
            //    Firing = false;
            //    m_ElapsedFrames = FireRate;
            //}
        }
        public override void LoadWeapon()
        {
            LoadTextures();

            //m_BulletLines = new List<Line>();
            //for (int i = 0; i < NumberOfBullets; ++i)
            //{
            //    m_BulletLines.Add(new Line(content));
            //}
        }
        protected override void LoadTextures()
        {
        }
        public override void ExplodeEnemy(Vector2 intersectingAngle, IEnemy enemy, Vector2 pos)
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
        public override void SetWeaponStats(int level)
        {
            throw new NotImplementedException();
        }
        public override void UpgradeWeaponLevel()
        {
            throw new NotImplementedException();
        }

    }
}
