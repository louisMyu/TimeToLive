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
    public class Rifle : Weapon
    {
        private const int CHARGE_TIME = 20;
        [DataMember]
        public string shotString1 { get; set; }
        [DataMember]
        public string shotString2 { get; set; }
        [DataMember]
        [IgnoreDataMember]
        private List<Line> m_BulletLines = new List<Line>();
        [IgnoreDataMember]
        private SpriteInfo m_SavedShotInfo;
        [IgnoreDataMember]
        private SpriteInfo m_CurrentShotInfo;

        [DataMember]
        public SpriteInfo SavedShotInfo { get { return m_SavedShotInfo; } set { m_SavedShotInfo = value; } }
        [DataMember]
        public SpriteInfo CurrentShotInfo { get { return m_CurrentShotInfo; } set { m_CurrentShotInfo = value; } }

        private AnimationManager m_FireAnimation;
        private SoundEffectInstance m_ChargeSound;
        public Rifle() : base()
        {
            Name = "Rifle";
            Spread = (float)Math.PI / 6;
            NumberOfBullets = 1;
            FireRate = 15;
            shotString1 = "rifle1";
            shotString2 = "rifle2";
            m_SightRange = 400;
            Knockback = 250f;
            BulletsExist = false;
        }

        public override void LoadContent()
        {
            LoadTextures();
            for (int i = 0; i < NumberOfBullets; ++i)
            {
                m_BulletLines.Add(new Line());
            }
        }
        //foreach line of the shotgun i need to update the lines based on the player center,
        //and rotate it and give it length, then update the graphical lines
        public override void Update(Vector2 playerCenter, Vector2 playerVelocity, float rotationAngle, int accuracy, bool shotFired, TimeSpan elapsedTime)
        {
            base.Update(playerCenter, playerVelocity, rotationAngle, accuracy, shotFired, elapsedTime);
            if (!Firing)
            {
                //float accuracyInRadians = WEAPON_RANDOM.Next(0, accuracy) * ((float)Math.PI / 180);
                //TODO: add a random so its either plus or minus accuracy
                float centerVector = rotationAngle;
                if (NumberOfBullets > 1)
                {
                    float leftAngle = centerVector - (Spread / (NumberOfBullets - 1));
                    LeftAngle = leftAngle;
                }
                else
                {
                    LeftAngle = centerVector;
                }
                
                foreach (Line line in m_BulletLines)
                {
                    line.Update(playerCenter, LeftAngle, SightRange);
                }
                m_CurrentShotInfo = new SpriteInfo(playerCenter, playerVelocity, rotationAngle, NumberOfBullets, LeftAngle);
            }
            //firing a shot, save the state
            if (!Firing && shotFired && CanFire())
            {
                Firing = true;
                m_FireAnimation.SpriteInfo = m_CurrentShotInfo;
                CanDamage = false;
                if (m_FireAnimation.CanStartAnimating())
                {
                    m_FireAnimation.Finished = false;
                }
                if (m_ChargeSound != null)
                {
                    m_ChargeSound.Stop();
                    m_ChargeSound.Dispose();
                }
                m_ChargeSound = SoundBank.GetSoundInstance("SoundRifleCharge");
                m_ChargeSound.Play();
            }
            if (m_FireAnimation.Animating && m_FireAnimation.FrameCounter == CHARGE_TIME)
            {
                if (m_ShotSound != null)
                {
                    m_ShotSound.Stop();
                    m_ShotSound.Dispose();
                }
                if (m_ChargeSound != null)
                {
                    m_ChargeSound.Stop();
                    m_ChargeSound.Dispose();
                }
                m_ShotSound = SoundBank.GetSoundInstance("SoundRifleShot");
                m_ShotSound.Play();
            }
        }
        //returns true if enemy died
        public override bool CheckCollision(GameObject ob)
        {
            if (!CanDamage)
            {
                return false;
            }
            foreach (Line line in m_BulletLines)
            {
                Vector2 check = line.Intersects(ob.m_Bounds);
                if (check.X != -1)
                {
                    Vector2 intersectingAngle = new Vector2(line.P2.X - line.P1.X, line.P2.Y - line.P1.Y);
                    IEnemy enemy;
                    if ((enemy = ob as IEnemy) != null)
                    {
                        enemy.ApplyLinearForce(intersectingAngle, Knockback);
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
            if (m_FireAnimation.CanDraw() && Firing)
            {
                m_FireAnimation.DrawAnimationFrame(_spriteBatch);
                //if frame is at 5
                if (m_FireAnimation.FrameCounter == 20)
                {
                    CanDamage = true;
                }
                foreach (Line line in m_BulletLines)
                {
                    line.Draw(_spriteBatch);
                }
                if (m_FireAnimation.FrameCounter == 40)
                {
                    CanDamage = false;
                }
            }
            else if (Firing)
            {
                Firing = false;
                m_ElapsedFrames = FireRate;
            }
        }
        public override void LoadWeapon()
        {
            LoadTextures();

            m_BulletLines = new List<Line>();
            for (int i = 0; i < NumberOfBullets; ++i)
            {
                m_BulletLines.Add(new Line());
            }
        }
        protected override void LoadTextures()
        {
            AnimationInfo[] array = new AnimationInfo[2];
            array[0] = new AnimationInfo(TextureBank.GetTexture(shotString1), CHARGE_TIME);
            array[1] = new AnimationInfo(TextureBank.GetTexture(shotString2), -1);
            m_FireAnimation = new AnimationManager(array, m_SavedShotInfo, 60);
        }
        public override void ExplodeEnemy(Vector2 intersectingAngle, IEnemy enemy, Vector2 pos)
        {
            List<Texture2D> gibTextures = enemy.GetExplodedParts();
            float spreadAngle = 180;
            float singleAngle = (spreadAngle / (float)gibTextures.Count);
            float startingPoint = singleAngle * gibTextures.Count / 2;
            for (int i = 0; i < gibTextures.Count; ++i)
            {
                ExplodedPart gib = new ExplodedPart();
                gib.LoadContent(gibTextures[i], pos);
                Vector2 halfAngle = Utilities.RadiansToVector2(Utilities.DegreesToRadians(-30));
                gib.ApplyLinearForce(intersectingAngle - (halfAngle) + (i * 2 * halfAngle), Knockback);
                //should be randomixed
                gib.ApplyTorque(5000f);
                UI.ActiveGibs.Add(gib);
            }
        }

        public override void SetWeaponStats(int level)
        {
            if (WeaponStatistics == null)
            {
                WeaponStatistics = new WeaponStats();
            }
            WeaponStatistics.WeaponLevel = level;
            switch (level)
            {
                case 1:
                    WeaponStatistics.WeaponDamage = 10;
                    break;
                case 2:
                    WeaponStatistics.WeaponDamage = 15;
                    break;
                case 3:
                    WeaponStatistics.WeaponDamage = 20;
                    break;
            }
        }
        public override void UpgradeWeaponLevel()
        {
            SetWeaponStats(++WeaponStatistics.WeaponLevel);
        }
    }
}
