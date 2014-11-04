using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    public class ObjectManager
    {
        private const int GRID_DIVISIONS_X = 50;
        private const int GRID_DIVISIONS_Y = 50;
        public static List<GameObject> AllGameObjects;
        public static List<GameObject>[][] GameObjectGrid;
        public static List<SlimeTrail> SlimeTrails;
        public static List<PowerUp> PowerUpItems;

        public static Random ZombieRandom = new Random();
        public double FrameCounter = 0;
        public static bool itemMade = false;
        public static bool face = false;
        public static Player m_Player;
        private ContentManager m_Content;
        private World m_World;
        private PowerUp m_PowerUp;

        private List<SpawnTimer> m_SpawnTimers;
        private double GameTimer = 0;
        public void Init(Player p, ContentManager content, World world)
        {
            m_Player = p;
            m_Content = content;
            m_World = world;
        }

        public void LoadContent()
        {
            //AllGameObjects = Storage.Load<List<GameObject>>("GameObjects", "ObjectList.dat");
            if (AllGameObjects == null)
            {
                AllGameObjects = new List<GameObject>();
            }
            else
            {
                foreach (GameObject g in AllGameObjects)
                {
                    g.Load(m_World);
                }
            }
            GameObjectGrid = new List<GameObject>[(Game1.GameWidth / GRID_DIVISIONS_X)+2][];
            for (int x = 0; x < GameObjectGrid.Length; ++x )
            {
                GameObjectGrid[x] = new List<GameObject>[(Game1.GameHeight / GRID_DIVISIONS_Y)+2];
                for (int y = 0; y < GameObjectGrid[x].Length; ++y)
                {
                    GameObjectGrid[x][y] = new List<GameObject>();
                }
            }
            PowerUpItems = new List<PowerUp>();
            SlimeTrails = new List<SlimeTrail>();
            AllGameObjects = new List<GameObject>();

            m_SpawnTimers = new List<SpawnTimer>();
            m_SpawnTimers.Add(new SpawnTimer(200, SpawnZombie, "Zombie"));
            //m_SpawnTimers.Add(new SpawnTimer(4000, SpawnFace, "Anubis"));
            //m_SpawnTimers.Add(new SpawnTimer(4000, SpawnShroom, "Shroom"));
            m_SpawnTimers.Add(new SpawnTimer(5500, MakeItem, "Item"));
            m_SpawnTimers.Add(new SpawnTimer(10000, MakeSlime, "Slime"));
        }

        public static List<GameObject> GetCell(Vector2 position)
        {
            int x = (int)position.X / GRID_DIVISIONS_X;
            int y = (int)position.Y / GRID_DIVISIONS_Y;
            if (x >= GameObjectGrid.Length || x < 0 || y >= GameObjectGrid[0].Length || y < 0)
            {
                return new List<GameObject>();
            }
            return GameObjectGrid[x][y];
        }

        public static List<List<GameObject>> GetCellsOfRectangle(Rectangle rec)
        {
            //the logic here should work as long as the rectangle passed in is =< half the size of the
            //grid tiles
            List<List<GameObject>> temp = new List<List<GameObject>>();
            List<GameObject> curCell = GetCell(new Vector2(rec.X, rec.Y));
            if (!temp.Contains(curCell))
            {
                temp.Add(curCell);
            }
            curCell = GetCell(new Vector2(rec.X, rec.Y+rec.Height));
            if (!temp.Contains(curCell))
            {
                temp.Add(curCell);
            }
            curCell = GetCell(new Vector2(rec.X+rec.Width, rec.Y));
            if (!temp.Contains(curCell))
            {
                temp.Add(curCell);
            }
            curCell = GetCell(new Vector2(rec.X+rec.Width, rec.Y+rec.Height));
            if (!temp.Contains(curCell))
            {
                temp.Add(curCell);
            }
            return temp;
        }
        public static void ClearGrid()
        {
            for (int x = 0; x < GameObjectGrid.Length; ++x)
            {
                for (int y = 0; y < GameObjectGrid[0].Length; ++y)
                {
                    GameObjectGrid[x][y].Clear();
                }
            }
        }
        public void CleanUp()
        {
            List<List<GameObject>> cellsToClean = new List<List<GameObject>>();
            foreach (GameObject ob in AllGameObjects)
            {
                if (ob is IEnemy)
                {
                    if (((GameObject)ob).CanDelete)
                    {
                        cellsToClean.Add(ObjectManager.GetCell(ob.Position));
                        continue;
                    }
                    IEnemy temp = ob as IEnemy;
                    if (temp.GetHealth() <= 0)
                    {
                        cellsToClean.Add(ObjectManager.GetCell(ob.Position));
                        RemoveObject(ob);
                    }
                }
            }
            foreach (PowerUp p in PowerUpItems)
            {
                if (p.CanDelete)
                {
                    cellsToClean.Add(ObjectManager.GetCell(p.Position));
                }
            }
            foreach (List<GameObject> cell in cellsToClean)
            {
                cell.RemoveAll(x => x.CanDelete);
            }
            AllGameObjects.RemoveAll(x => x.CanDelete);
            PowerUpItems.RemoveAll(x => x.CanDelete);

            for (int i = 0; i < SlimeTrails.Count; ++i)
            {
                SlimeTrails[i].Update();
                if (!SlimeTrails[i].Alive)
                {
                    SlimeTrails.Remove(SlimeTrails[i]);
                    --i;
                }
            }
        }

        public void Update(TimeSpan elapsedTime)
        {
            GameTimer += elapsedTime.TotalMilliseconds;
            CleanUp();
            m_SpawnTimers.RemoveAll(x => x.CanDelete);
            foreach (SpawnTimer timer in m_SpawnTimers)
            {
                timer.Update(elapsedTime);
            }
            if (GameTimer > 60000 && GameTimer < 65000)
            {
                m_SpawnTimers.RemoveAll(x => x.Name == "Anubis");
            }
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            foreach (GameObject g in AllGameObjects)
            {
                if (g is PowerUp)
                {
                    _spriteBatch.Draw(g.Texture, g.Position, null, Color.White, Utilities.DegreesToRadians(90.0f), new Vector2(g.Texture.Width / 2, g.Texture.Height / 2), new Vector2(1,1), SpriteEffects.None, 0);
                }
                else
                {
                    g.Draw(_spriteBatch);
                }
            }
        }
        public void DrawSlimeTrails(SpriteBatch spriteBatch)
        {
            foreach (SlimeTrail trail in SlimeTrails)
            {
                trail.Draw(spriteBatch);
            }
        }
        public void DrawPowerUps(SpriteBatch spriteBatch)
        {
            foreach (PowerUp p in PowerUpItems)
            {
                p.Draw(spriteBatch);
            }
        }
        public static void RemoveObject(GameObject obj)
        {
            if (obj is IEnemy)
            {
                ((IEnemy)obj).CleanBody();
            }
            obj.CanDelete = true;
        }

        public void ResetGame()
        {
            ClearGrid();
            AllGameObjects.Clear();
            FrameCounter = 0;
            itemMade = false;
            face = false;
            m_Player.Score = 0;
        }
        private void SpawnZombie()
        {
            bool nearPlayer = true;
            int x = 0;
            int y = 0;
            while (nearPlayer)
            {
                x = ZombieRandom.Next(Game1.GameWidth);
                y = ZombieRandom.Next(Game1.GameHeight);

                //don't spawn near player
                Vector2 distanceFromPlayer = new Vector2(x - m_Player.Position.X, y - m_Player.Position.Y);
                if (distanceFromPlayer.LengthSquared() >= (300.0f * 300f))
                {
                    nearPlayer = false;
                }
            }
            Zombie z = new Zombie();
            Vector2 temp = new Vector2(x,y);
            z.Position = temp;
            z.LoadContent(m_World);
            AllGameObjects.Add(z);
        }

        private void MakeItem()
        {
            bool nearPlayer = true;
            int x = 0;
            int y = 0;
            while (nearPlayer)
            {
                x = ZombieRandom.Next(720);
                y = ZombieRandom.Next(1280);

                //don't spawn near player
                Vector2 distanceFromPlayer = new Vector2(x - m_Player.Position.X, y - m_Player.Position.Y);
                if (distanceFromPlayer.LengthSquared() >= (150.0f * 150.0f))
                {
                    nearPlayer = false;
                }
            }
            //int powerUpType = ZombieRandom.Next(2);
            int powerUpType = 0;
            if (powerUpType == 0) 
            {
                m_PowerUp = new CheatPowerUp(CheatPowerUp.CheatTypes.Time);
            }
            else if (powerUpType == 1)
            {
                m_PowerUp = new WeaponPowerUp((WeaponPowerUp.WeaponType)ZombieRandom.Next(3));
            }
            Vector2 temp = new Vector2();
            temp.X = MathHelper.Clamp(x, 0 + UI.OFFSET, Game1.GameWidth-15);
            temp.Y = MathHelper.Clamp(y, 0, Game1.GameHeight-15);
            m_PowerUp.Position = temp;
            m_PowerUp.LoadContent();
            ObjectManager.PowerUpItems.Add(m_PowerUp);
            GetCell(m_PowerUp.Position).Add(m_PowerUp);
        }
        //probably should add spawn face in here
        private void MakeSlime()
        {
            bool nearPlayer = true;
            int x = 0;
            int y = 0;
            while (nearPlayer)
            {
                x = ZombieRandom.Next(Game1.GameWidth);
                y = ZombieRandom.Next(Game1.GameHeight);

                //don't spawn near player
                Vector2 distanceFromPlayer = new Vector2(x - m_Player.Position.X, y - m_Player.Position.Y);
                if (distanceFromPlayer.LengthSquared() >= (200.0f * 200f))
                {
                    nearPlayer = false;
                }
            }
            Slime z = new Slime();
            Vector2 temp = new Vector2();
            temp.X = x;
            temp.Y = y;
            z.Position = temp;
            z.LoadContent(m_World);
            AllGameObjects.Add(z);
        }
        private void SpawnFace()
        {
            bool nearPlayer = true;
            int x = 0;
            int y = 0;
            while (nearPlayer)
            {
                x = ZombieRandom.Next(Game1.GameWidth);
                y = ZombieRandom.Next(Game1.GameHeight);

                //don't spawn near player
                Vector2 distanceFromPlayer = new Vector2(x - m_Player.Position.X, y - m_Player.Position.Y);
                if (distanceFromPlayer.LengthSquared() >= (150.0f * 150f))
                {
                    nearPlayer = false;
                }
            }
            Anubis z = new Anubis();
            Vector2 temp = new Vector2();
            temp.X = x;
            temp.Y = y;
            z.Position = temp;
            z.LoadContent(m_World);
            ObjectManager.AllGameObjects.Add(z);
        }
        private void SpawnWolf()
        {
            Wolf wolf = new Wolf();
            wolf.Position = new Vector2(m_Player.Position.X + 50, m_Player.Position.Y + 50);
            wolf.LoadContent(m_World);
            ObjectManager.AllGameObjects.Add(wolf);
        }
        private bool shroomSpawned = false;
        private void SpawnShroom()
        {

            if (shroomSpawned) return;
            Shroom mushroom = new Shroom();
            mushroom.Position = new Vector2(m_Player.Position.X + 150, m_Player.Position.Y - 150);
            mushroom.LoadContent(m_World);
            ObjectManager.AllGameObjects.Add(mushroom);
            shroomSpawned = true;
        }
        public PowerUp MakeRandomItem()
        {
            Random r = new Random();
            int x = r.Next(0, 10);
            if (x == 5)
            {
                return new WeaponPowerUp(WeaponPowerUp.WeaponType.Shotgun);
            }
            return new CheatPowerUp(CheatPowerUp.CheatTypes.Time);
        }
        public delegate void SpawnDelegate();
        public class SpawnTimer
        {
            public string Name;
            public bool CanDelete;
            double Timer = 0;
            double SpawnTime;
            SpawnDelegate callback;
            public SpawnTimer(double spawntime, SpawnDelegate cb, string name)
            {
                SpawnTime = spawntime;
                callback = cb;
                CanDelete = false;
                Name = name;
            }
            public void Update(TimeSpan elapsedTime)
            {
                if (CanDelete) return;
                Timer += elapsedTime.TotalMilliseconds;
                if (Timer >= SpawnTime)
                {
                    Timer = 0;
                    callback();
                }
            }
        }
    }

}
