using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
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
    class Shroom : GameObject, IEnemy
    {
        private AnimationTimer m_BlinkingTimer;
        float[] m_BlinkingIntervals = new float[2];
        string[] m_BlinkingTextures = new string[2];
        const string BlinkAnimationName = "BlinkingAnimation";

        double pufftime;
        double currentPuffTime;
        private TimeSpan DAMAGE_AMOUNT = TimeSpan.FromSeconds(5);
        public enum MotionState
        {
            Wandering,
            Locked,
            Dead
        }
        [DataMember]
        public Vector2 bodyPosition { get; set; }
        [IgnoreDataMember]
        private Texture2D m_Texture = null;
        [IgnoreDataMember]
        private float m_Speed = 2.5f;
        [DataMember]
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        [IgnoreDataMember]
        public Body _circleBody;

        [DataMember]
        public int LifeTotal { get; set; }

        [DataMember]
        public MotionState State { get; set; }
        public Shroom(PhysicsManager manager)
            : base(manager)
        {
            LifeTotal = 40;
            m_BlinkingIntervals[0] = 2500;
            m_BlinkingIntervals[1] = 1000;
            m_BlinkingTextures[0] = "ShroomEyeClosed";
            m_BlinkingTextures[1] = "ShroomEyeOpen";
            m_BlinkingTimer = new AnimationTimer(m_BlinkingIntervals, BlinkAnimationName, HandleAnimation, true);
            circleRadius = 65;
            pufftime = 5;
        }
        private void HandleAnimation(object o, AnimationTimerEventArgs e)
        {
            switch (e.AnimationName)
            {
                case BlinkAnimationName:
                    m_Texture = TextureBank.GetTexture(m_BlinkingTextures[e.FrameIndex]);
                    break;
            }
        }

        public override void LoadContent()
        {
            m_Direction = new Vector2(0, 0);
            foreach (string s in m_BlinkingTextures)
            {
                TextureBank.GetTexture(s);
            }
            m_Texture = TextureBank.GetTexture(m_BlinkingTextures[0]);

            Width = m_Texture != null ? m_Texture.Width : 0;
            Height = m_Texture != null ? m_Texture.Height : 0;

            if (m_Texture != null)
            {
                m_Bounds.Width = Width;
                m_Bounds.Height = Height;
                m_Bounds.X = (int)Position.X - Width / 2;
                m_Bounds.Y = (int)Position.Y - Height / 2;
                m_Origin.X = Width / 2;
                m_Origin.Y = Height / 2;
            }
            Fixture fixture;
            _circleBody = m_PhysicsManager.GetBody(ConvertUnits.ToSimUnits(35 / 2f), 0.5f, ConvertUnits.ToSimUnits(Position), out fixture);
            _circleBody.BodyType = BodyType.Dynamic;
            _circleBody.Mass = 5f;
            _circleBody.LinearDamping = 3f;
            _circleBody.Restitution = .5f;
            
            circleCenter = Position;
            circleCenter.Y += circleRadius;

            puffExplosion = new Puff(m_PhysicsManager);
        }

        private Puff puffExplosion;
        private Vector2 circleCenter;
        private float circleRadius;
        bool backAndForth = true;
        float moveTime;
        float circleTime = 0;
        //moves a set amount per frame toward a certain location
        public override void Move(Microsoft.Xna.Framework.Vector2 loc, TimeSpan elapsedTime)
        {
            circleTime += (float)elapsedTime.TotalMilliseconds;
            //should really just use the Sim's position for everything instead of converting from one to another
            Vector2 simPosition = ConvertUnits.ToDisplayUnits(_circleBody.Position);
            if (float.IsNaN(simPosition.X) || float.IsNaN(simPosition.Y))
            {
                return;
            }
            else
            {
                this.Position = simPosition;
            }
            moveTime += (float)elapsedTime.TotalSeconds;
            if (backAndForth)
            {
                circleCenter.X += 1;
            }
            else
            {
                circleCenter.X -= 1;
            }
            if (moveTime > 5)
            {
                backAndForth = !backAndForth;
                moveTime = 0;
            }
            float speedScale = (float)(0.001 * 2 * Math.PI) / Speed;
            float angle = circleTime * speedScale;
            if (angle > Math.PI * 2) circleTime = 0;
            Vector2 newPos = new Vector2();
            newPos.X = circleCenter.X + (float)Math.Sin(angle) * circleRadius;
            newPos.Y = circleCenter.Y + (float)Math.Cos(angle) * circleRadius;
            Position = newPos;
            if (!float.IsNaN(this.Position.X) && !float.IsNaN(this.Position.Y))
            {
                _circleBody.Position = ConvertUnits.ToSimUnits(this.Position);
            }

            m_Bounds.X = (int)Position.X - Width / 2;
            m_Bounds.Y = (int)Position.Y - Height / 2;

            
        }
        
        public void DoPuff()
        {
            puffExplosion.Position = Position;
            puffExplosion.Trigger();
        }
        public override void Update(Player player, TimeSpan elapsedTime)
        {
            ObjectManager.GetCell(Position).Remove(this);
            Move(player.Position, elapsedTime);
            ObjectManager.GetCell(Position).Add(this);

            currentPuffTime += elapsedTime.TotalSeconds;
            if (currentPuffTime > pufftime)
            {
                DoPuff();
                currentPuffTime = 0;
            }
            puffExplosion.Update(player, elapsedTime);

            bodyPosition = _circleBody.Position;
            m_BlinkingTimer.Update(elapsedTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_Texture, Position, null, Color.White, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
            puffExplosion.Draw(spriteBatch);
        }

        public static void LoadTextures()
        {
            
        }
        #region IEnemy
        public void DropItem()
        {
            PowerUp p = new WeaponPowerUp(WeaponPowerUp.WeaponType.Rifle, m_PhysicsManager);
            p.Position = Position;
            p.LoadContent();
            ObjectManager.PowerUpItems.Add(p);
            ObjectManager.GetCell(p.Position).Add(p);
        }
        public void CleanBody()
        {
            m_PhysicsManager.ReturnBody(_circleBody);
        }
        public void ApplyLinearForce(Vector2 angle, float amount)
        {
            Vector2 impulse = Vector2.Normalize(angle) * amount;
            _circleBody.ApplyLinearImpulse(impulse);
        }
        public void AddToHealth(int amount)
        {
            LifeTotal += amount;
        }
        public List<Texture2D> GetExplodedParts()
        {
            return ExplodedParts;
        }
        protected override void LoadExplodedParts()
        {
            ExplodedParts.Add(TextureBank.GetTexture("SlimePart1"));
            ExplodedParts.Add(TextureBank.GetTexture("SlimePart2"));
            ExplodedParts.Add(TextureBank.GetTexture("SlimePart3"));
        }
        public int GetHealth()
        {
            return LifeTotal;
        }
        public TimeSpan GetDamageAmount()
        {
            return DAMAGE_AMOUNT;
        }
        public void DoCollision(Player player)
        {
            ObjectManager.RemoveObject(this);
        }
        #endregion


        class Puff : GameObject 
        {
            const string m_AnimationName = "PuffAnimation";
            AnimationTimer animationTimer;
            string[] textures;
            float[] intervals;
            public bool canDraw;
            private void HandleAnimation(object o, AnimationTimerEventArgs e)
            {
                Texture = TextureBank.GetTexture(textures[e.FrameIndex]);
                m_Bounds.Width = Texture.Width;
                m_Bounds.Height = Texture.Height;
                Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            }


            public Puff(PhysicsManager manager)
                : base(manager)
            {
                textures = new string[12];
                intervals = new float[12];
                textures[0] = "puffAnimation\\PuffAnimation1";
                textures[1] = "puffAnimation\\PuffAnimation2";
                textures[2] = "puffAnimation\\PuffAnimation3";
                textures[3] = "puffAnimation\\PuffAnimation4";
                textures[4] = "puffAnimation\\PuffAnimation5";
                textures[5] = "puffAnimation\\PuffAnimation6";
                textures[6] = "puffAnimation\\PuffAnimation7";
                textures[7] = "puffAnimation\\PuffAnimation8";
                textures[8] = "puffAnimation\\PuffAnimation9";
                textures[9] = "puffAnimation\\PuffAnimation10";
                textures[10] = "puffAnimation\\PuffAnimation11";
                textures[11] = "puffAnimation\\PuffAnimation12";
                intervals[0] = 40;
                intervals[1] = 50;
                intervals[2] = 50;
                intervals[3] = 50;
                intervals[4] = 50;
                intervals[5] = 60;
                intervals[6] = 50;
                intervals[7] = 50;
                intervals[8] = 60;
                intervals[9] = 60;
                intervals[10] = 60;
                intervals[11] = 90;
                canDraw = false;
            }

            public override void Update(Player player, TimeSpan elapsedTime)
            {
                if (animationTimer != null)
                {
                    animationTimer.Update(elapsedTime);
                    if (animationTimer.Done)
                    {
                        animationTimer.IntervalOcccured -= HandleAnimation;
                        animationTimer = null;
                        canDraw = false;
                        ObjectManager.GetCell(Position).Remove(this);
                    }
                }

            }
            public void Trigger()
            {
                animationTimer = new AnimationTimer(intervals, m_AnimationName, HandleAnimation, false);
                canDraw = true;
                Texture = TextureBank.GetTexture(textures[0]);
                ObjectManager.GetCell(Position).Add(this);
                Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                if (canDraw)
                {
                    base.Draw(spriteBatch);
                }
            }
        }
    }
}
