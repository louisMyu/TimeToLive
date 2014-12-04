using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    public class PhysicsManager
    {
        private Stack<Body> m_PhysicsBodies;
        private const int MAX_NUM_BODIES = 500;
        private World m_World;
        public PhysicsManager()
        {
            m_PhysicsBodies = new Stack<Body>();
            m_World = new World(new Vector2(0,0));
        }

        public void Init()
        {
            for (int i = 0; i < MAX_NUM_BODIES; ++i)
            {
                Body temp = BodyFactory.CreateBody(m_World);
                temp.Enabled = false;
                m_PhysicsBodies.Push(temp);
            }
        }
        public void Update(float dt)
        {
            m_World.Step(dt);
        }
        public Body GetBody(float radius, float density, Vector2 position, out Fixture attached)
        {
            Body bodyFromPool;
            if (m_PhysicsBodies.Count != 0)
            {
                bodyFromPool = m_PhysicsBodies.Pop();
            }
            else
            {
                bodyFromPool = BodyFactory.CreateBody(m_World);
            }
            bodyFromPool.Position = position;
            attached = FixtureFactory.AttachCircle(radius, density, bodyFromPool);
            bodyFromPool.LinearVelocity = new Vector2(0,0);
            bodyFromPool.AngularVelocity = 0;
            bodyFromPool.Enabled = true;
            return bodyFromPool;
        }

        public void ReturnBody(Body body)
        {
            if (m_PhysicsBodies.Count >= MAX_NUM_BODIES)
            {
                body.Dispose();
                return;
            }
            body.Enabled = false;
            
            body.DestroyFixture(body.FixtureList[0]);

            m_PhysicsBodies.Push(body);
        }
    }
}
