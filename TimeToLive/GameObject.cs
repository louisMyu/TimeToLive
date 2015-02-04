using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TimeToLive
{

    public class GameObject
    {
        protected PhysicsManager m_PhysicsManager;
        protected static Random RANDOM_GENERATOR = new Random(69);
        private Vector2 m_Position;
        public Vector2 Position { get { return m_Position; } set { m_Position = value; } }
        
        public Texture2D Texture;
        private Texture2D OutlineTexture;

        protected List<Texture2D> ExplodedParts = new List<Texture2D>();
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle m_Bounds;
        public Rectangle Bounds { get { return m_Bounds; } set { m_Bounds = value; } }
        public Vector2 m_Origin = new Vector2();
        public Vector2 Origin { get { return m_Origin; } set { m_Origin = value; } }
        public Vector2 m_Direction = new Vector2();
        public Vector2 Direction { get { return m_Direction; } set { m_Direction = value; } }
        private float m_RotationAngle = 0.0f;
        public float RotationAngle
        {
            get
            {
                return m_RotationAngle;
            }
            set
            {
                m_RotationAngle = value % (MathHelper.Pi * 2);
            }
                
        }
        public bool CanDelete = false;
        public GameObject(PhysicsManager manager)
        {

            if (m_Bounds == null)
            {
                m_Bounds = new Rectangle();
            }
            m_PhysicsManager = manager;
        }
        public virtual void Init(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public virtual void Update(Player player, TimeSpan elapsedTime)
        {
        }
        public virtual void LoadContent() 
        {
            Width = Texture != null ? Texture.Width : 0;
            Height = Texture != null ? Texture.Height : 0;
            
            if (Texture != null)
            {
                m_Bounds.Width = Width;
                m_Bounds.Height = Height;
                m_Bounds.X = (int)Position.X - Width / 2;
                m_Bounds.Y = (int)Position.Y - Height / 2;
                m_Origin.X = Width / 2;
                m_Origin.Y = Height / 2;
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
        }

        public virtual void Move(Vector2 amount, TimeSpan elapsedTime)
        {
            Position += amount * 60 / 1000 * elapsedTime.Milliseconds;
        }
        public virtual void Save()
        {
        }

        public virtual void CheckCollisions(GameObject obj)
        {

        }
        protected virtual void LoadExplodedParts()
        {
        }
    }
    public class ExplodedPart
    {
        static Random GibRandom = new Random();
        Texture2D m_Texture;
        Body Body;
        Vector2 m_Origin;
        float RotationAngle;
        public void LoadContent(Texture2D tex, Vector2 pos, PhysicsManager physics)
        {
            m_Texture = tex;
            Fixture fixture;
            Body = physics.GetBody(ConvertUnits.ToSimUnits(tex.Height / 2), 1f, ConvertUnits.ToSimUnits(pos), out fixture);
            Body.BodyType = BodyType.Dynamic;
            Body.Mass = 2.5f;
            Body.LinearDamping = 2.5f;
            Body.AngularDamping = 0.6f;
            fixture.OnCollision += MyOnCollision;
            m_Origin = new Vector2(m_Texture.Width / 2, m_Texture.Height / 2);
            RotationAngle = (float)((GibRandom.Next(0, 301) / 300.0f) * Math.PI * 2);
            Body.Rotation = RotationAngle;
        }

        public bool MyOnCollision(Fixture f1, Fixture f2,
                                    FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return false;
        }
        public void Update(out bool hasStopped)
        {
            hasStopped = false;
            RotationAngle = Body.Rotation;
            Vector2 vel = Body.LinearVelocity;
            if (vel.X == 0 && vel.Y == 0)
            {
                hasStopped = true;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Color c)
        {
            Vector2 temp = ConvertUnits.ToDisplayUnits(Body.Position);
            spriteBatch.Draw(m_Texture, temp, null, c, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
        }
        public void DrawOffset(SpriteBatch spriteBatch, Color c)
        {
            Vector2 temp = ConvertUnits.ToDisplayUnits(Body.Position);
            temp.X -= UI.OFFSET;
            spriteBatch.Draw(m_Texture, temp, null, c, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
        }
        public void ApplyLinearForce(Vector2 angle, float force)
        {
            Vector2 impulse = Vector2.Normalize(angle) * force;
            Body.ApplyLinearImpulse(impulse);
        }
        public void ApplyTorque(float force)
        {
            Body.ApplyTorque(force);
        }
        public void CleanBody(PhysicsManager manager)
        {
            manager.ReturnBody(Body);
            Body = null;
        }
    }
}
