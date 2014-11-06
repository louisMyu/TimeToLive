using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics;
using System.Runtime.Serialization;

namespace TimeToLive
{
    [KnownType(typeof(Zombie))]
    [KnownType(typeof(GameObject))]
    [DataContract]
    public class Zombie : GameObject, IEnemy
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
        private float m_Speed = 1.0f;
        [DataMember]
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        [DataMember]
        public int LifeTotal { get; set; }

        private MotionState m_State;
        [DataMember]
        public MotionState State { get; set; }


        public Zombie() : base()
        {
            LifeTotal = 40;
            
        }
        public void LoadContent(World world)
        {
            m_State = MotionState.Locked;
            RotationAngle = (float)GameObject.RANDOM_GENERATOR.NextDouble();
            m_Direction.X = (float)Math.Cos(RotationAngle);
            m_Direction.Y = (float)Math.Sin(RotationAngle);

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
            LoadExplodedParts();
        }
        public static void LoadTextures()
        {
            if (m_Texture == null)
            {
                m_Texture = TextureBank.GetTexture("kevinZombie");
            }
            TextureBank.GetTexture("ZombieBody");
            TextureBank.GetTexture("ZombieHead");
        }
        //moves a set amount per frame toward a certain location
        public override void Move(Microsoft.Xna.Framework.Vector2 loc, TimeSpan elapsedTime)
        {
            //should really just use the Sim's position for everything instead of converting from one to another
            switch (m_State)
            {
                case MotionState.Wandering:
                    if (RANDOM_GENERATOR.Next(150) % 150 == 1)
                    {
                        RotationAngle = (float)RANDOM_GENERATOR.NextDouble() * MathHelper.Pi * 2;
                        m_Direction.X = (float)Math.Cos(RotationAngle);
                        m_Direction.Y = (float)Math.Sin(RotationAngle);
                    }
                    break;

                case MotionState.Locked:
                    m_Direction = loc;
                    RotationAngle = (float)Math.Atan2(loc.Y, loc.X);
                    m_State = MotionState.Locked;
                    m_Speed = 2.0f;
                    break;
            }

            m_Direction = Vector2.Normalize(m_Direction);
            Vector2 amount = m_Direction * m_Speed;
            if (CurrentKickbackAmount.LengthSquared() > 0)
            {
                amount += CurrentKickbackAmount;
                CurrentKickbackAmount.X = 0;
                CurrentKickbackAmount.Y = 0;
            }
            base.Move(amount, elapsedTime);

            m_Bounds.X = (int)Position.X - Width / 2;
            m_Bounds.Y = (int)Position.Y - Height / 2;
        }
        public override void Update(Player player, TimeSpan elapsedTime)
        {
            //get a normalized direction toward the point that was passed in, probably the player
            Vector2 vec = new Vector2(player.Position.X - Position.X, player.Position.Y - Position.Y);
            if (vec.LengthSquared() <= (275.0f * 275.0f))
            {
                m_State = MotionState.Locked;
            }
            ObjectManager.GetCell(Position).Remove(this);
            Move(vec, elapsedTime);
            ObjectManager.GetCell(Position).Add(this);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_Texture, Position, null, Color.White, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
        }
        #region IEnemy
        public void CleanBody()
        {
        }
        public void AddToHealth(int amount)
        {
            LifeTotal += amount;
        }
        public int GetHealth()
        {
            return LifeTotal;
        }
        public TimeSpan GetDamageAmount()
        {
            return DAMAGE_AMOUNT;
        }

        public List<Texture2D> GetExplodedParts()
        {
            return ExplodedParts;
        }
        protected override void LoadExplodedParts()
        {
            ExplodedParts.Add(TextureBank.GetTexture("ZombieBody"));
            ExplodedParts.Add(TextureBank.GetTexture("ZombieHead"));
        }
        private Vector2 CurrentKickbackAmount;
        public void ApplyLinearForce(Vector2 angle, float amount)
        {
            CurrentKickbackAmount = angle * amount;
        }
        public void DoCollision(Player player)
        {
            ObjectManager.RemoveObject(this);
            //this should cause an explosion

        }
        public void DropItem()
        {
            PowerUp p = new WeaponPowerUp(WeaponPowerUp.WeaponType.Plasma);
            p.Position = Position;
            p.LoadContent();
            ObjectManager.PowerUpItems.Add(p);
            ObjectManager.GetCell(p.Position).Add(p);
        }
        #endregion
        #region Save/Load
        #endregion
    }
}
