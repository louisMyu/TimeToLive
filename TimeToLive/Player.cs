using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using System.Runtime.Serialization;
using FarseerPhysics.Factories;
using FarseerPhysics;
using Microsoft.Xna.Framework.Audio;

namespace TimeToLive
{
    [KnownType(typeof(GameObject))]
    [KnownType(typeof(Shotgun))]
    [DataContract]
    public class Player : GameObject
    {
        private enum PlayerState
        {
            Normal,
            Damaged,
            Dead
        }
        //where the weapon shot originates from, rotates with the player
        Vector2 m_WeaponShotPoint;
        [IgnoreDataMember]
        public static readonly string playerSaveDir = "playerDir";
        [IgnoreDataMember]
        public static float VELOCITY = 40f;

        //how many frames of invincible to occur when player is damaged
        private const int INVINCIBLE_FRAMES = 100;
        //how many frames to wait until a flash occurs when being hit, done via modulo
        //this should be based on time, not that bad to change
        private const int INVINCIBLE_FLASH_FRAME = 15;
        [IgnoreDataMember]
        public Vector2 m_MoveToward = new Vector2();
        [DataMember]
        public Vector2 MoveToward { get { return m_MoveToward; } set { m_MoveToward = value; } }
        [IgnoreDataMember]
        private List<Weapon> m_Weapons;
        [IgnoreDataMember]
        private bool m_Moving = false;
        [DataMember]
        public bool Moving { get { return m_Moving; } set { m_Moving = value; } }
        [DataMember]
        public TimeSpan TimeToDeath { get; set; }
        public TimeSpan StartingTime { get; set; }
        [DataMember]
        public CheatPowerUp WeaponSlot1Magic { get; set; }
        private List<Cheat> m_ActiveEffects = new List<Cheat>();
        private PlayerState m_PlayerState;
        private SoundEffectInstance m_PowerupPickupSound;
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public bool isFireButtonDown
        {
            get;
            set;
        }
        public bool IsStopDown { get; set; }
        public Body _circleBody;

        public Texture2D RedFlashTexture;

        public bool DrawRedFlash;
        private int m_HowLongInvincible = 0;
        private Vector2 m_InitialPosition;
        public Player(PhysicsManager manager) : base(manager)
        {
			
        }
        public void Init(Microsoft.Xna.Framework.Content.ContentManager content, Player player)
        {
            m_Weapons = player.m_Weapons;
            Position = player.Position;
            isFireButtonDown = player.isFireButtonDown;
            TimeToDeath = player.TimeToDeath;
            StartingTime = player.StartingTime;
            IsStopDown = false;
        }
        public void Init(Microsoft.Xna.Framework.Content.ContentManager content, Vector2 pos)
        {
            m_Weapons = new List<Weapon>();
            m_Weapons.Add(new Shotgun());
            Position = pos;
            isFireButtonDown = false;
            StartingTime = TimeSpan.FromMinutes(1);
            TimeToDeath = StartingTime;
            IsStopDown = false;
            DrawRedFlash = false;
        }
        //check collisions with things
        public void CheckCollisions()
        {
            //float nearestLength = float.MaxValue;
            List<List<GameObject>> objectsToCheck = ObjectManager.GetCellsOfRectangle(Bounds);
            foreach (List<GameObject> gameObjectList in objectsToCheck)
            {
                foreach (GameObject ob in gameObjectList)
                {
                    if (ob is Player) { continue; }
                    if (ob == null)
                    {
                        //need to handle null exeception here
                        gameObjectList.Remove(ob);
                        continue;
                    }
                    if (ob.m_Bounds.Intersects(this.m_Bounds))
                    {
                        if (ob is IEnemy && !ob.CanDelete)
                        {
                            IEnemy enemy = ob as IEnemy;
                            //get the amount of time that should be removed from the enemy and remove it from the player
                            //TimeToDeath -= enemy.GetDamageAmount();
                            //do collision should take care of removing the enemy
                            enemy.DoCollision(this);
                            if (TimeToDeath.TotalSeconds <= 0)
                            {
                                m_PlayerState = PlayerState.Dead;
                                return;
                            }
                            continue;
                        }
                        if (ob is PowerUp)
                        {
                            if (m_PowerupPickupSound != null)
                            {
                                m_PowerupPickupSound.Stop();
                                m_PowerupPickupSound.Dispose();
                            }
                            m_PowerupPickupSound = ((PowerUp)ob).GetPickupSound();
                            m_PowerupPickupSound.Play();
                            //cheat powerups will always be instant now
                            if (ob is CheatPowerUp)
                            {
                                CheatPowerUp temp = ob as CheatPowerUp;
                                if (temp.CheatEffect is IInstant)
                                {
                                    IInstant instantEffect = temp.CheatEffect as IInstant;
                                    instantEffect.GetInstantEffect(this);
                                    WeaponSlot1Magic = null;
                                }
                                else
                                {
                                    CheatPowerUp cheat = ob as CheatPowerUp;
                                    WeaponSlot1Magic = cheat;
                                }
                            }
                            if (ob is WeaponPowerUp)
                            {
                                if (m_Weapons.Count < 4)
                                {
                                    Weapon weapon = WeaponPowerUp.GetWeaponType((WeaponPowerUp)ob);
                                    weapon.LoadContent();
                                    m_Weapons.Add(weapon);
                                }
                            }
                            ObjectManager.RemoveObject(ob);
                        }
                    }
                }
            }
        }
        //check if our weapon hit anything
        public void CheckWeaponHits()
        {
            foreach (Weapon w in m_Weapons) {
                if (w.Firing || w.BulletsExist)
                {
                    foreach (GameObject ob in ObjectManager.AllGameObjects)
                    {
                        if (ob is PowerUp)
                        {
                            continue;
                        }
                        //this probably should check for collision only when firing
                        //that way the bullet lines wont update to the next person while a shot is going off
                        if (w.CheckCollision(ob, m_PhysicsManager))
                        {
                            ++Score;
                        }
                    }
                }
            }
        }
        public void LoadContent(PhysicsManager physManager)
        {
            m_InitialPosition = Position;
            Texture = TextureBank.GetTexture("Player");
            base.LoadContent();
            foreach (Weapon w in m_Weapons)
            {
                w.LoadContent();
            }
            Fixture fixture;
            _circleBody = physManager.GetBody(ConvertUnits.ToSimUnits(35 / 2f), 1f, ConvertUnits.ToSimUnits(Position), out fixture);
            _circleBody.BodyType = BodyType.Dynamic;
            _circleBody.Mass = 4f;
            _circleBody.LinearDamping = 2f;
            if (!float.IsNaN(this.Position.X) && !float.IsNaN(this.Position.Y))
            {
                _circleBody.Position = ConvertUnits.ToSimUnits(this.Position);
            }
            RedFlashTexture = TextureBank.GetTexture("RED");
        }

        //moves a set amount per frame toward a certain location
        public override void Move(Microsoft.Xna.Framework.Vector2 loc, TimeSpan elapsedTime)
        {
            
            if (Input.UseAccelerometer)
            {
                base.Move(loc, elapsedTime);
                Vector2 temp = new Vector2();
                temp.X = MathHelper.Clamp(Position.X, 0, Game1.GameWidth);
                temp.Y = MathHelper.Clamp(Position.Y, 0, Game1.GameHeight);
                Position = temp;
                m_Bounds.X = (int)Position.X - Width / 2;
                m_Bounds.Y = (int)Position.Y - Height / 2;
            }
            else
            {
                //get a normalized direction toward the point
                Vector2 amount = new Vector2(loc.X - Position.X, loc.Y - Position.Y);
                if (amount.LengthSquared() > VELOCITY * VELOCITY)
                {
                    amount = Vector2.Normalize(amount);
                    amount *= VELOCITY;
                }
                base.Move(amount, elapsedTime);
                Vector2 temp = new Vector2();
                temp.X = MathHelper.Clamp(Position.X, 0 + UI.OFFSET, Game1.GameWidth + Width);
                temp.Y = MathHelper.Clamp(Position.Y, 0, Game1.GameHeight + Height);
                Position = temp;

                m_Bounds.X = (int)Position.X - Width / 2;
                m_Bounds.Y = (int)Position.Y - Height / 2;
            }
        }

        internal void ProcessInput(bool firing, bool stopMoving)
        {
            isFireButtonDown = firing;
            IsStopDown = stopMoving;
            //test.Update(m_MoveToward, new Vector2(Position.X, Position.Y));
        }

        public void Update(TimeSpan elapsedTime)
        {
            switch (m_PlayerState)
            {
                case PlayerState.Damaged:
                case PlayerState.Normal:
                    UpdatePlayerState();
                    //must be a better way to do this...
                    foreach (Cheat cheat in m_ActiveEffects)
                    {
                        if (cheat.IsDone())
                        {
                            cheat.EndEffect(this);
                        }
                    }
                    m_ActiveEffects.RemoveAll(x => x.IsDone() || x == null);
                    foreach (Cheat cheat in m_ActiveEffects)
                    {
                        if (cheat == null) continue;
                        cheat.Update(this);
                    }
                    //foreach (Weapon weapon in m_Weapons)
                    //{
                    //    if (!KickedBack && isFireButtonDown && weapon.CanFire())
                    //    {
                    //        KickedBack = true;
                    //        weapon.ApplyKickback(this);
                    //    }
                    //    if (!weapon.Firing && KickedBack)
                    //    {
                    //        KickedBack = false;
                    //    }
                    //}
                    m_Moving = true;
                    ObjectManager.GetCell(Position).Remove(this);
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

                    if (!Input.UseAccelerometer)
                    {
                        if ((m_MoveToward.X == Position.X && m_MoveToward.Y == Position.Y))
                        {
                            m_Moving = false;
                        }
                    }
                    else
                    {
                        Vector2 acceleration = new Vector2(-Input.CurrentAccelerometerValues.Y, Input.CurrentAccelerometerValues.X);
                        if (acceleration.LengthSquared() > Input.Tilt_Threshold)
                        {
                            m_MoveToward = new Vector2(MathHelper.Clamp(acceleration.X * 30, -(Math.Abs(acceleration.X) * VELOCITY), Math.Abs(acceleration.X) * VELOCITY),
                                                        -1 * MathHelper.Clamp(acceleration.Y * 30, -(Math.Abs(acceleration.Y) * VELOCITY), Math.Abs(acceleration.Y) * VELOCITY));
                            //if (!m_Weapon.Firing)
                            //{
                            //    //dont apply rotation unless tilt amount is greater than a threshold
                            //    //if (!IsStopDown)
                            //    //{
                            //    //    RotationAngle = (float)Math.Atan2(-acceleration.Y, acceleration.X);
                            //    //}
                            //    //RotationAngle = UI.ThumbStickAngle;
                            //}
                        }
                        else
                        {
                            m_MoveToward = new Vector2(0, 0);
                        }
                        foreach (Weapon w in m_Weapons)
                        {
                            if (!w.Firing)
                            {
                                //RotationAngle += UI.RotationDelta;
                                RotationAngle = (float)Math.Atan2(-acceleration.Y, acceleration.X);
                            }
                        }
                    }
                    if (IsStopDown)
                    {
                        m_Moving = false;
                    }
                    if (m_Moving)
                    {
                        Move(m_MoveToward, elapsedTime);
                    }

                    if (!float.IsNaN(this.Position.X) && !float.IsNaN(this.Position.Y))
                    {
                        _circleBody.Position = ConvertUnits.ToSimUnits(this.Position);
                    }
                    ObjectManager.GetCell(Position).Add(this);
                    Vector2 playerVel = m_Moving ? m_MoveToward : new Vector2(0, 0);

                    Matrix rotMatrix = Matrix.CreateRotationZ(RotationAngle);
                    Vector2 offsetFromPlayer = new Vector2(Texture.Width/2, 0);
                    m_WeaponShotPoint = Position + Vector2.Transform(offsetFromPlayer, rotMatrix);
                    foreach (Weapon w in m_Weapons)
                    {
                        w.Update(m_WeaponShotPoint, playerVel, RotationAngle, 10, isFireButtonDown, m_PhysicsManager, elapsedTime);
                    }
                    break;
                case PlayerState.Dead:
                    //update animation logic for playing death animation
                    break;
            }
        }

        public override void Draw(SpriteBatch _spriteBatch)
        {
            switch (m_PlayerState)
            {
                case PlayerState.Damaged:
                case PlayerState.Normal:
                    if (IsStopDown)
                    {
                        //Vector2 aimScale = Utilities.GetSpriteScaling(new Vector2(UI.StopButtonRec.Width, UI.StopButtonRec.Height), new Vector2(AimCircleTexture.Width, AimCircleTexture.Height));
                        //Texture2D temp;
                        //if (UI.ThumbStickPointOffset.LengthSquared() > (UI.StopButtonRec.Width / 2) * (UI.StopButtonRec.Width / 2))
                        //{
                        //    temp = AimCircleRedTexture;
                        //}
                        //else
                        //{
                        //    temp = AimCircleTexture;
                        //}
                        //_spriteBatch.Draw(temp, Position, null, Color.White, 0.0f, new Vector2(AimCircleTexture.Width / 2, AimCircleTexture.Height / 2), aimScale, SpriteEffects.None, 0);
                    }
                    if ((m_PlayerState != PlayerState.Damaged) || (m_PlayerState == PlayerState.Damaged && CanDrawWhenFlashing()))
                    {
                        base.Draw(_spriteBatch);
                        foreach (Weapon w in m_Weapons)
                        {
                            if (w.Firing)
                            {
                                w.DrawBlast(_spriteBatch);
                            }
                        }
                    }
                    //if (IsStopDown)
                    //{
                    //    //_spriteBatch.Draw(ReticuleTexture, Position + UI.ThumbStickPointOffset, null, Color.White, 0.0f, new Vector2(7, 7), 1.0f, SpriteEffects.None, 0);
                    //}
                    break;
                case PlayerState.Dead:
                    PlayDeathAnimation(_spriteBatch);
                    break;
            }
        }
        public void ApplyLinearForce(Vector2 force)
        {
            this._circleBody.ApplyLinearImpulse(force);
        }

        public void LoadWeapons()
        {
            foreach (Weapon w in m_Weapons)
            {
                w.LoadWeapon();
            }
        }

        public void StartCheatEffect()
        {
            if (WeaponSlot1Magic != null)
            {
                m_ActiveEffects.Add(WeaponSlot1Magic.CheatEffect);
                WeaponSlot1Magic.CheatEffect.StartEffect(this);
                WeaponSlot1Magic = null;
            }
        }
        private void UpdatePlayerState()
        {
            switch (m_PlayerState)
            {
                case PlayerState.Damaged:
                if (m_HowLongInvincible >= INVINCIBLE_FRAMES)
                {
                    m_HowLongInvincible = 0;
                    m_PlayerState = PlayerState.Normal;
                    return;
                }
                ++m_HowLongInvincible;
                break;
            }
        }
        private bool CanDrawWhenFlashing()
        {
            bool temp = m_HowLongInvincible % INVINCIBLE_FLASH_FRAME != 0;
            return temp;
        }

        private int m_CurrentDeathFrame = 0;
        private Color m_CurrentDeathColor = new Color(0,249,0);
        public bool isDead = false;
        private void PlayDeathAnimation(SpriteBatch spriteBatch)
        {
            m_CurrentDeathColor = new Color(m_CurrentDeathFrame, 249 - m_CurrentDeathFrame, m_CurrentDeathFrame/2);
            spriteBatch.Draw(Texture, Position, null, m_CurrentDeathColor, RotationAngle, m_Origin, 1.0f, SpriteEffects.None, 0f);
            ++m_CurrentDeathFrame;
            if (m_CurrentDeathFrame >= 250)
            {
                isDead = true;
            }
        }

        //i should use this to reset the player during a game reset
        public void ResetPlayer()
        {
            TimeToDeath = StartingTime;
            isDead = false;
            m_CurrentDeathFrame = 0;
            Position = m_InitialPosition;
        }

        public void SetPlayerToDyingState()
        {
            m_PlayerState = PlayerState.Dead;
        }
    }
}
