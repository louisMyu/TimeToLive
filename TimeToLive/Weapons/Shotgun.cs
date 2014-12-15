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
    public class Shotgun : Weapon
    {
        [DataMember]
        public string blastString { get; set; }
        [DataMember]
        public string blast2String { get; set; }
        [DataMember]
        public string blast3String { get; set; }
        [DataMember]
        public string blast4String { get; set; }
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

        private SoundEffectInstance m_ReloadSound;
        private AnimationManager m_FireAnimation;

        private int m_ShotgunDamage;
        public int ShotgunDamage
        {
            get { return m_ShotgunDamage; }
            set { m_ShotgunDamage = value; }
        }
        public Shotgun() : base()
        {
            Name = "Shotgun";
            Spread = (float)Math.PI / 6;
            NumberOfBullets = 3;
            FireRate = 15;
            blastString = "Shotgun-Blast-1";
            blast2String = "Shotgun-Blast-2";
            blast3String = "Shotgun-Blast-3";
            blast4String = "Shotgun-Blast-4";
            SightRange = 100;
            Knockback = 1000f;
            BulletsExist = false;
        }

        public override void LoadContent()
        {
            LoadTextures();
            for (int i = 0; i < NumberOfBullets; ++i)
            {
                m_BulletLines.Add(new Line());
            }
            m_ShotgunDamage = 5;
            //m_ShotgunDamage = WeaponStatistics.WeaponDamage;
        }
        //foreach line of the shotgun i need to update the lines based on the player center,
        //and rotate it and give it length, then update the graphical lines
        public override void Update(Vector2 gunMountPoint, Vector2 playerVelocity, float rotationAngle, int accuracy, bool shotFired, PhysicsManager manager, TimeSpan elapsedTime)
        {
            base.Update(gunMountPoint, playerVelocity, rotationAngle, accuracy, shotFired, manager, elapsedTime);
            if (!Firing)
            {
                //float accuracyInRadians = WEAPON_RANDOM.Next(0, accuracy) * ((float)Math.PI / 180);
                ////TODO: add a random so its either plus or minus accuracy
                //float centerVector = rotationAngle - accuracyInRadians;

                float leftAngle = rotationAngle - (Spread / (NumberOfBullets - 1));
                LeftAngle = leftAngle;
                foreach (Line line in m_BulletLines)
                {
                    line.Update(gunMountPoint, leftAngle, SightRange);
                    leftAngle += (float)(Spread / (NumberOfBullets - 1));
                }
                m_CurrentShotInfo = new SpriteInfo(gunMountPoint, playerVelocity, rotationAngle, NumberOfBullets, leftAngle);
            }
            //firing a shot, save the state
            if (!Firing && shotFired && CanFire())
            {
                if (m_ShotSound != null)
                {
                    m_ShotSound.Stop();
                    m_ShotSound.Dispose();
                }
                if (m_ReloadSound != null)
                {
                    m_ReloadSound.Stop();
                    m_ReloadSound.Dispose();
                }
                m_ShotSound = SoundBank.GetSoundInstance("SoundShotgun");
                m_ShotSound.Play();
                Firing = true;
                m_FireAnimation.SpriteInfo = m_CurrentShotInfo;
                CanDamage = true;
                if (m_FireAnimation.CanStartAnimating())
                    m_FireAnimation.Finished = false;
            }
            //if i delete this here the show will not follow the player
            if (m_CurrentShotInfo != null)
            {
                float leftAngle = rotationAngle - (Spread / (NumberOfBullets - 1));
                m_CurrentShotInfo.Position = gunMountPoint;
                LeftAngle = leftAngle;
                foreach (Line line in m_BulletLines)
                {
                    line.Update(gunMountPoint, leftAngle, SightRange);
                    leftAngle += (float)(Spread / (NumberOfBullets - 1));
                }
            }
        }
        //returns true if enemy died
        public override bool CheckCollision(GameObject ob, PhysicsManager manager)
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
                    intersectingAngle.Normalize();
                    IEnemy enemy;
                    if ((enemy = ob as IEnemy) != null && enemy.GetHealth() > 0)
                    {
                        enemy.AddToHealth(-m_ShotgunDamage);
                        if (enemy.GetHealth() <= 0)
                        {
                            ExplodeEnemy(intersectingAngle, enemy, ob.Position, manager);
                            enemy.DropItem();
                            return true;
                        }
                        enemy.ApplyLinearForce(intersectingAngle, 200);
                    }
                }
            }
            return false;
        }
        public override void DrawWeapon(SpriteBatch _spriteBatch, Vector2 position, float rot)
        {
            
        }

        public override void DrawBlast(SpriteBatch _spriteBatch, Vector2 position, float rot)
        {
            //i need to redo the animation design to firing a weapon
            //make it based on time and fire time is over reset the value
            if (m_FireAnimation.CanDraw() && Firing)
            {
                m_FireAnimation.DrawAnimationFrame(_spriteBatch);
                foreach (Line line in m_BulletLines)
                {
                    line.Draw(_spriteBatch);
                }
                //if frame is at 12
                if (m_FireAnimation.FrameCounter == 12)
                {
                    CanDamage = false;
                }
            }
            else if (Firing)
            {
                Firing = false;
                m_ElapsedFrames = FireRate;
                if (m_ReloadSound != null)
                {
                    m_ReloadSound.Stop();
                    m_ReloadSound.Dispose();
                }
                m_ReloadSound = SoundBank.GetSoundInstance("SoundShotgunReload");
                m_ReloadSound.Play();
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
            AnimationInfo[] array = new AnimationInfo[4];
            array[0] = new AnimationInfo(TextureBank.GetTexture(blastString), 5);
            array[1] = new AnimationInfo(TextureBank.GetTexture(blast2String), 9);
            array[2] = new AnimationInfo(TextureBank.GetTexture(blast3String), 12);
            array[3] = new AnimationInfo(TextureBank.GetTexture(blast4String), -1);
            m_FireAnimation = new AnimationManager(array, m_SavedShotInfo, 15);
        }
        public override void ExplodeEnemy(Vector2 intersectingAngle, IEnemy enemy, Vector2 pos, PhysicsManager manager)
        {
            List<Texture2D> gibTextures = enemy.GetExplodedParts();
            for (int i = 0; i < gibTextures.Count; ++i)
            {
                float randomTorque = (-1 + (Weapon.WEAPON_RANDOM.Next(2)*2))*(500000 + (500000f*(float)Weapon.WEAPON_RANDOM.NextDouble()));
                float randomDegree = -45f + (90f * (float)Weapon.WEAPON_RANDOM.NextDouble());
                float randomForce = Knockback + (1500f * (float)Weapon.WEAPON_RANDOM.NextDouble());
                ExplodedPart gib = new ExplodedPart();
                gib.LoadContent(gibTextures[i], pos, manager);
                //so we dont divide by zero
                if (intersectingAngle.X == 0) intersectingAngle.X += 0.000001f;
                float originalDegrees = (float)Math.Atan2(intersectingAngle.Y, intersectingAngle.X);
                float newDegrees = Utilities.NormalizeRadians(originalDegrees) + Utilities.DegreesToRadians(randomDegree);
                Vector2 change = new Vector2((float)Math.Cos(newDegrees), (float)Math.Sin(newDegrees));
                gib.ApplyLinearForce(change, randomForce);
                //should be randomixed
                gib.ApplyTorque(randomTorque);
                UI.ActiveGibs.Add(gib);
            }
        }
        public override void ApplyKickback(Player player)
        {
            Vector2 temp = new Vector2((float)Math.Cos(player.RotationAngle), (float)Math.Sin(player.RotationAngle)) * -100;
            player.ApplyLinearForce(temp);
        }
        #region WeaponStat overrides

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
        #endregion
    }
}
