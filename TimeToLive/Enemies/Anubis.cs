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
    [KnownType(typeof(Anubis))]
    [KnownType(typeof(GameObject))]
    [DataContract]
    public class Anubis : GameObject, IEnemy
    {
        public enum MotionState
        {
            Locked,
            Dead,
            Attacking
        }
        //100 frames of spin attacking
        private const int ATTACK_TIME = 100;
        [IgnoreDataMember]
        private int m_FramesLeftAttacking;
        [DataMember]
        public Vector2 bodyPosition { get; set; }
        [IgnoreDataMember]
        static private Texture2D m_Texture = null;
        [IgnoreDataMember]
        private float m_Speed = 0.6f;
        [DataMember]
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        [DataMember]
        public int LifeTotal { get; set; }

        private MotionState m_State;
        [DataMember]
        public MotionState State { get; set; }


        public Anubis()
            : base()
        {
            LifeTotal = 80;

        }

        public void LoadContent()
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
                m_Texture = TextureBank.GetTexture("Face");
            }
            //TODO load exploded textures here
        }
        //moves a set amount per frame toward a certain location
        public override void Move(Microsoft.Xna.Framework.Vector2 loc, TimeSpan elapsedTime)
        {
            //should really just use the Sim's position for everything instead of converting from one to another


            switch (m_State)
            {
                case MotionState.Locked:
                    m_Direction = loc;
                    RotationAngle = (float)Math.Atan2(loc.Y, loc.X);
                    m_Speed = 1.0f;
                    break;
            }

            m_Direction = Vector2.Normalize(m_Direction);
            Vector2 amount = m_Direction * m_Speed;
            base.Move(amount, elapsedTime);



            m_Bounds.X = (int)Position.X - Width / 2;
            m_Bounds.Y = (int)Position.Y - Height / 2;
        }
        public override void Update(Player player, TimeSpan elapsedTime)
        {
            Vector2 playerPosition = player.Position;
            ObjectManager.GetCell(Position).Remove(this);
            //get a normalized direction toward the point that was passed in, probably the player
            Vector2 vec = new Vector2(playerPosition.X - Position.X, playerPosition.Y - Position.Y);
            float temp = vec.LengthSquared();

            if (temp <= (275.0f * 275.0f) && m_State != MotionState.Attacking)
            {
                if (vec.LengthSquared() <= (80.0f * 80.0f))
                {
                    m_State = MotionState.Attacking;
                }
                else
                {
                    m_State = MotionState.Locked;
                    m_FramesLeftAttacking = ATTACK_TIME;
                }
            }
            else if (m_State == MotionState.Attacking)
            {
                if (m_FramesLeftAttacking <= 0)
                {
                    m_State = MotionState.Locked;
                }
            }

            if (m_State == MotionState.Locked)
            {
                Move(vec, elapsedTime);
            }
            else if (m_State == MotionState.Attacking)
            {
                //will require a separate animation manager object for this specific animation
                //IDEAL: rotation to a chainsaw rec and check for collision with the player
                //for now it is just rotating the sprite itself
                RotationAngle += (float)(2*Math.PI*0.02);
                --m_FramesLeftAttacking;
            }
            ObjectManager.GetCell(Position).Add(this);

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_Texture, Position, null, Color.White, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
        }
        #region IEnemy
        public void DropItem()
        {
            PowerUp p = new CheatPowerUp(CheatPowerUp.CheatTypes.Wrath);
            p.Position = Position;
            p.LoadContent();
            ObjectManager.PowerUpItems.Add(p);
            ObjectManager.GetCell(p.Position).Add(p);
        }
        public void CleanBody()
        {

        }
        public List<Texture2D> GetExplodedParts()
        {
            return ExplodedParts;
        }
        public void ApplyLinearForce(Vector2 angle, float amount)
        {
            Vector2 impulse = Vector2.Normalize(angle) * amount;
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
            return TimeSpan.FromSeconds(10);
        }
        protected override void LoadExplodedParts()
        {
            ExplodedParts.Add(TextureBank.GetTexture("AnubisPart1"));
            ExplodedParts.Add(TextureBank.GetTexture("AnubisPart2"));
            ExplodedParts.Add(TextureBank.GetTexture("AnubisPart3"));
        }
        public void DoCollision(Player player)
        {
            if (m_State == MotionState.Attacking)
            {
                Vector2 dirOfPlayer = new Vector2(player.Position.X - Position.X, player.Position.Y - Position.Y);
                dirOfPlayer *= 700f;
                player.ApplyLinearForce(dirOfPlayer);
            }
        }
        #endregion

    }
}
