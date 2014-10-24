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
    public class Wolf : GameObject, IEnemy
    {
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
        static private Texture2D m_Texture = null;
        [IgnoreDataMember]
        private float m_Speed = 1.5f;
        [DataMember]
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        [IgnoreDataMember]
        public Body _circleBody;

        [DataMember]
        public int LifeTotal { get; set; }

        [DataMember]
        public MotionState State { get; set; }
        public Wolf()
            : base()
        {
            LifeTotal = 40;

        }
        private WolfHand Lefthand;
        private WolfHand Righthand;

        private const string m_MoveHandsString = "MoveHandsAnimation";
        private AnimationTimer MoveHandsTimer;
        private float[] moveHandsIntervals;
        public void LoadContent(World world)
        {
            m_Direction = new Vector2(0, 0);
            m_Texture = TextureBank.GetTexture("WolfBody");
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

            _circleBody = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(35 / 2f), 1f, ConvertUnits.ToSimUnits(Position));
            _circleBody.BodyType = BodyType.Dynamic;
            _circleBody.Mass = 5f;
            _circleBody.LinearDamping = 3f;
            _circleBody.Restitution = .5f;

            Lefthand = new WolfHand(WolfHand.LeftOrRightHand.Left, this);
            Righthand = new WolfHand(WolfHand.LeftOrRightHand.Right, this);
            Lefthand.LoadContent();
            Righthand.LoadContent();

            moveHandsIntervals = new float[5];
            moveHandsIntervals[0] = 1000;
            moveHandsIntervals[1] = 1000;
            moveHandsIntervals[2] = 1000;
            moveHandsIntervals[3] = 1000;
            moveHandsIntervals[4] = 1000;
            MoveHandsTimer = new AnimationTimer(moveHandsIntervals, m_MoveHandsString, AnimationHandler, false);

        }
        private void AnimationHandler(object o, AnimationTimerEventArgs e)
        {

            switch (e.AnimationName)
            {
                case m_MoveHandsString:
                    Vector2 amount = m_Direction;
                    amount.Normalize();
                    //in here i would move the hands a certain amount in the direction that they are moving
                    //depending on which animation interval that im in
                    switch (e.FrameIndex)
                    {
                        case 0:
                            amount *= 5;
                            break;
                        case 1:
                            amount *= 10;
                            break;
                        case 2:
                            amount *= 5;
                            break;
                        case 3:
                            amount *= 10;
                            break;
                        case 4:
                            amount *= 10;
                            break;
                    }
                    Lefthand.Move(amount, TimeSpan.FromSeconds(0));
                    Righthand.Move(amount, TimeSpan.FromSeconds(0));
                    break;
            }
        }
        //moves a set amount per frame toward a certain location 
        public override void Move(Microsoft.Xna.Framework.Vector2 loc, TimeSpan elapsedTime)
        {

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
            //get the wolf direction
            GetDirection();
            RotationAngle = (float)Math.Atan2(m_Direction.Y, m_Direction.X);
            if (!float.IsNaN(this.Position.X) && !float.IsNaN(this.Position.Y))
            {
                _circleBody.Position = ConvertUnits.ToSimUnits(this.Position);
            }

            m_Bounds.X = (int)Position.X - Width / 2;
            m_Bounds.Y = (int)Position.Y - Height / 2;
        }
        private void GetDirection()
        {

        }
        public override void Update(Player player, TimeSpan elapsedTime)
        {
            ObjectManager.GetCell(Position).Remove(this);
            Move(player.Position, elapsedTime);
            ObjectManager.GetCell(Position).Add(this);

            bodyPosition = _circleBody.Position;
            Lefthand.Update(this);
            Righthand.Update(this);
            MoveHandsTimer.Update(elapsedTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_Texture, ConvertUnits.ToDisplayUnits(_circleBody.Position), null, Color.White, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
            Lefthand.Draw(spriteBatch);
            Righthand.Draw(spriteBatch);
        }

        public static void LoadTextures()
        {
            
        }
        #region IEnemy
        public void CleanBody()
        {
            if (_circleBody != null)
            {
                GameplayScreen.m_World.RemoveBody(_circleBody);
            }
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
        public void DropItem()
        {
            PowerUp p = new WeaponPowerUp(WeaponPowerUp.WeaponType.Repeater);
            p.Position = Position;
            p.LoadContent();
            ObjectManager.PowerUpItems.Add(p);
            ObjectManager.GetCell(p.Position).Add(p);
        }
        #endregion
        #region Save/Load
        public override void Save()
        {
        }
        public override void Load(World world)
        {
            if (m_Texture == null)
            {
                m_Texture = TextureBank.GetTexture("WolfBody");
            }
            _circleBody = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(35 / 2f), 1f, ConvertUnits.ToSimUnits(Position));
            _circleBody.BodyType = BodyType.Dynamic;
            _circleBody.Mass = 0.2f;
            _circleBody.LinearDamping = 2f;
            _circleBody.Position = bodyPosition;
        }
        #endregion
        class WolfHand : GameObject, IEnemy
        {
            private TimeSpan DAMAGE_AMOUNT = TimeSpan.FromSeconds(5);
            [DataMember]
            public int LifeTotal { get; set; }
            public enum LeftOrRightHand
            {
                Left,
                Right
            }
            private LeftOrRightHand WhichHand;
            public WolfHand(LeftOrRightHand which, Wolf body)
            {
                if (which == LeftOrRightHand.Left)
                {
                    Texture = TextureBank.GetTexture("kevinZombie");
                }
                else
                {
                    Texture = TextureBank.GetTexture("kevinZombie");
                }
                WhichHand = which;
                Position = body.Position;
            }

            public void Update(Wolf wolfBody)
            {
                RotationAngle = wolfBody.RotationAngle;
                Vector2 tempPos;
                switch (WhichHand)
                {
                    case LeftOrRightHand.Left:
                        Position = wolfBody.Position;
                        tempPos = new Vector2((float)Math.Cos(RotationAngle - Math.PI / 2), (float)Math.Sin(RotationAngle - Math.PI / 2));
                        Position += Vector2.Normalize(tempPos) * 55;
                        break;
                    case LeftOrRightHand.Right:
                        Position = wolfBody.Position;
                        tempPos = new Vector2((float)Math.Cos(RotationAngle + Math.PI / 2), (float)Math.Sin(RotationAngle + Math.PI / 2));
                        Position += Vector2.Normalize(tempPos) * 55;
                        break;
                }
            }
            private void SetTexture(Texture2D tex)
            {
                Texture = tex;
            }
            public int GetHealth()
            {
                throw new NotImplementedException();
            }

            public void AddToHealth(int amount)
            {
                LifeTotal += amount;
            }

            public void ApplyLinearForce(Vector2 angle, float amount)
            {
                throw new NotImplementedException();
            }

            public void CleanBody()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetDamageAmount()
            {
                return DAMAGE_AMOUNT;
            }

            public List<Texture2D> GetExplodedParts()
            {
                throw new NotImplementedException();
            }

            public void DoCollision(Player p)
            {
                ObjectManager.RemoveObject(this);
            }
            public void DropItem()
            {
            }
        }
    }

}
