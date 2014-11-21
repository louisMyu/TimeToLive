using Microsoft.Xna.Framework;
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
    class Plasma : Weapon
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
        public Plasma() : base()
        {
            Name = "Plasma";
            Spread = (float)Math.PI / 6;
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
        public override void Update(Vector2 playerCenter, Vector2 playerVelocity, float rotationAngle, int accuracy, bool shotFired, PhysicsManager manager, TimeSpan elapsedTime)
        {
            base.Update(playerCenter, playerVelocity, rotationAngle, accuracy, shotFired, manager, elapsedTime);
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
                Bullet temp = new Bullet(m_CurrentShotInfo, 10, manager);
                temp.LoadContent();
                m_Bullets.Add(temp);
                m_ElapsedFrames = FireRate;

                if (m_ShotSound != null)
                {
                    m_ShotSound.Stop();
                    m_ShotSound.Dispose();
                }
                m_ShotSound = SoundBank.GetSoundInstance("SoundPlasmaShot");
                m_ShotSound.Play();
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
        public override bool CheckCollision(GameObject ob, PhysicsManager manager)
        {
            bool hit = false;
            foreach (Bullet b in m_Bullets)
            {
                hit = b.CheckCollision(ob);
                if (hit)
                {
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
        public override void ExplodeEnemy(Vector2 intersectingAngle, IEnemy enemy, Vector2 pos, PhysicsManager manager)
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
                gib.LoadContent(gibTextures[i], pos, manager);
                Vector2 halfAngle = Utilities.RadiansToVector2(Utilities.DegreesToRadians(-30));
                gib.ApplyLinearForce(intersectingAngle - (halfAngle) + (i * 2 * halfAngle), Knockback * 1.5f);
                //shoul be randomixed
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
                    WeaponStatistics.WeaponDamage = 5;
                    break;
                case 2:
                    WeaponStatistics.WeaponDamage = 7;
                    break;
                case 3:
                    WeaponStatistics.WeaponDamage = 10;
                    break;
            }
        }
        public override void UpgradeWeaponLevel()
        {
            SetWeaponStats(++WeaponStatistics.WeaponLevel);
        }
    }
    public class Bullet : GameObject
    {
        private enum BulletState
        {
            Flying,
            Contact
        }

        private BulletState m_State;
        public int Velocity { get; set; }
        private Vector2 m_Heading;
        private Vector2 m_InitialPosition;
        private Vector2 playerVelocity;

        const string m_AnimationName = "PlasmaShotTravelAnimation";
        AnimationTimer animationTimer;
        string[] textures;
        float[] intervals;

        private AnimationTimer PlasmaHitAnimationTimer;
        private static Random r = new Random();
        private Texture2D plasmaHitTexture = TextureBank.GetTexture(hitTextures[0]);
        private const string m_HitAnimation = "PlastShotHitAnimation";
        private static string[] hitTextures = { "plasmaShot\\plasmaShotHit01", "plasmaShot\\plasmaShotHit02", "plasmaShot\\plasmaShotHit03", "plasmaShot\\plasmaShotHit04", 
                                                  "plasmaShot\\plasmaShotHit05", "plasmaShot\\plasmaShotHit06", "plasmaShot\\plasmaShotHit07", "plasmaShot\\plasmaShotHit08", 
                                                  "plasmaShot\\plasmaShotHit09", "plasmaShot\\plasmaShotHit10", "plasmaShot\\plasmaShotHit11", "plasmaShot\\plasmaShotHit12" };
        private static float[] hitIntervals = { 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15};
    
        private void HandlePlasmaHitAnimation(object o, AnimationTimerEventArgs e)
        {
            Texture = TextureBank.GetTexture(hitTextures[e.FrameIndex]);
        }

        private void HandleAnimation(object o, AnimationTimerEventArgs e)
        {
            Texture = TextureBank.GetTexture(textures[e.FrameIndex]);
            m_Bounds.Width = Texture.Width;
            m_Bounds.Height = Texture.Height;
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
        }

        public Bullet(SpriteInfo info, int vel, PhysicsManager manager)
            : base(manager)
        {
            m_State = BulletState.Flying;
            Position = info.Position;
            m_InitialPosition = Position;
            //maybe this should just be a random rotation
            RotationAngle = info.Rotation;
            m_Heading = new Vector2((float)Math.Cos(RotationAngle), (float)Math.Sin(RotationAngle));
            Velocity = vel;
            playerVelocity = info.PlayerVelocity;
            textures = new string[8];
            intervals = new float[8];
            textures[0] = "plasmaShot\\plasmaShot1test";
            textures[1] = "plasmaShot\\plasmaShot2test";
            textures[2] = "plasmaShot\\plasmaShot3test";
            textures[3] = "plasmaShot\\plasmaShot4test";
            textures[4] = "plasmaShot\\plasmaShot5test";
            textures[5] = "plasmaShot\\plasmaShot4test";
            textures[6] = "plasmaShot\\plasmaShot3test";
            textures[7] = "plasmaShot\\plasmaShot2test";

            intervals[0] = 20;
            intervals[1] = 20;
            intervals[2] = 20;
            intervals[3] = 20;
            intervals[4] = 20;
            intervals[5] = 20;
            intervals[6] = 20;
            intervals[7] = 20;
        }

        public override void LoadContent()
        {
            int currentFrame = r.Next(8);
            animationTimer = new AnimationTimer(intervals, m_AnimationName, HandleAnimation, true, currentFrame);
            Texture = TextureBank.GetTexture(textures[currentFrame]);
            base.LoadContent();
        }
        public void Update(TimeSpan elapsedTime)
        {
            switch (m_State)
            {
                case BulletState.Flying:
                    if (animationTimer != null)
                    {
                        animationTimer.Update(elapsedTime);
                        if (animationTimer.Done)
                        {
                            animationTimer.IntervalOcccured -= HandleAnimation;
                            animationTimer = null;
                            CanDelete = true;
                            return;
                        }
                    }
                    float t = Vector2.DistanceSquared(Position, m_InitialPosition);
                    if (t > 250000)
                    {
                        animationTimer.IntervalOcccured -= HandleAnimation;
                        animationTimer = null;
                        CanDelete = true;
                        return;
                    }
                    Move((Velocity * m_Heading) + playerVelocity, elapsedTime);
                    m_Bounds.X = (int)Position.X - Width / 2;
                    m_Bounds.Y = (int)Position.Y - Height / 2;
                break;
            
            
                case BulletState.Contact:
                    PlasmaHitAnimationTimer.Update(elapsedTime);
                    if (PlasmaHitAnimationTimer.Done)
                    {
                        CanDelete = true;
                    }
                break;
            }
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            //base.Draw(_spriteBatch);
            _spriteBatch.Draw(Texture, Position, null, Color.White, RotationAngle, Origin, 1.0f, SpriteEffects.None, 0f);
        }

        public bool CheckCollision(GameObject ob)
        {
            if (Bounds.Intersects(ob.Bounds) && m_State == BulletState.Flying)
            {
                PlasmaHitAnimationTimer = new AnimationTimer(hitIntervals, m_HitAnimation, HandlePlasmaHitAnimation, false);
                Texture = TextureBank.GetTexture(hitTextures[0]);
                //play the bullet explosion animation here, make sure to remove bounds, it should be fast enough that nothing clips
                m_State = BulletState.Contact;
                return true;
            }
            return false;
        }
    }
}
