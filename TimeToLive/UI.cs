using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace TimeToLive
{
    class UI
    {
        private TimeSpan TimeToDeath;
        private Texture2D m_StatusBackground;
        public static SpriteFont m_SpriteFont;


        public static int OFFSET = 175;
        private Vector2 m_StatusBackgroundPosition;
        private Vector2 m_StatusBackGroundScale;
        private Texture2D m_Background;

        public int PlayfieldBottom;
        private float GameWidth;
        private float GameHeight;

        private Texture2D m_SkullBackground;

        public Effect m_SkullLeftEyePointLight;
        public Effect m_SkullRightEyePointLight;

        private SpriteFont ColunaFont;
        public static float RotationDelta;

        private int BackGroundHueCounter = -250;
        public Color BackGroundHueColor = new Color(255,0,0);
        private const int MAX_PERIOD = 600;
        private const int MIN_PERIOD = 40;
        private int m_Period;
        //start increasing oscillation at 30 seconds
        private const int OSCILLATE_START = 10;
        private const int SCALE = 200;

        private List<ExplodedPart> BakedGibs = new List<ExplodedPart>();
        public static List<ExplodedPart> ActiveGibs = new List<ExplodedPart>();
        public bool TimeAlmostOut;

        public UI()
        {
        }

        public void LoadContent(ContentManager content, float width, float height)
        {
            TimeAlmostOut = false;
            m_StatusBackground = content.Load<Texture2D>("Line");
            m_SpriteFont = content.Load<SpriteFont>("Retrofont");
            ColunaFont = content.Load<SpriteFont>("ColunaFont");

            m_StatusBackgroundPosition = new Vector2(0, 0);
            m_StatusBackGroundScale = Utilities.GetSpriteScaling(new Vector2(OFFSET, height), new Vector2(m_StatusBackground.Width, m_StatusBackground.Height));
            PlayfieldBottom = OFFSET;
            GameWidth = ScreenManager.NativeResolution.X;
            GameHeight = ScreenManager.NativeResolution.Y;
            m_Background = content.Load<Texture2D>("Louis-game-backgroundFULL");

            ActiveGibs.Clear();

            Vector2 viewport = new Vector2(GameWidth, GameHeight);
            Vector2 textSize = ColunaFont.MeasureString("00:00:00");
            DeathTimerScale = Utilities.GetSpriteScaling(new Vector2((int)((viewport.X) * 0.2), (int)((viewport.Y) * 0.2)), textSize);
            Vector2 scaledTextSize = textSize*DeathTimerScale;
            Vector2 textPosition = new Vector2(viewport.X - (scaledTextSize.X/2)-10, (scaledTextSize.Y/2)+ 10);
            DeathTimerPosition = textPosition;
            DeathTimerOrigin = new Vector2(textSize.X / 2, textSize.Y / 2);

            textSize = ColunaFont.MeasureString("00");
            CountdownScale = Utilities.GetSpriteScaling(new Vector2((int)(viewport.Y * 0.45), (int)(viewport.X * 0.35)), textSize);
            CountdownPosition = viewport / 2;
            CountdownOrigin = new Vector2(textSize.X / 2, textSize.Y / 2);

            m_SkullBackground = TextureBank.GetTexture("isolatedSkullBG01");

            m_SkullLeftEyePointLight = ((Game1)ScreenManager.Game).LoadShader("TimeToLive.Shaders.pointlight.mgfxo");
            m_SkullRightEyePointLight = ((Game1)ScreenManager.Game).LoadShader("TimeToLive.Shaders.pointlight.mgfxo");
            m_SkullLeftEyePointLight.Parameters["centerX"].SetValue(546.0f);
            m_SkullLeftEyePointLight.Parameters["centerY"].SetValue(307.0f);

        }

        public void Update(TimeSpan elapsedTime)
        {

            TimeAlmostOut = false;
            m_Period = MAX_PERIOD;
            if (TimeToDeath.TotalSeconds <= OSCILLATE_START)
            {
                TimeAlmostOut = true;
            }
            if (BackGroundHueCounter >= m_Period + 1)
            {
                BackGroundHueCounter = 0;
            }
            if (TimeAlmostOut)
            {
                m_Period = MIN_PERIOD;
            }
            int delta = (int)(Math.Sin(BackGroundHueCounter * 2 * Math.PI / m_Period) * (SCALE / 2) + (SCALE / 2));
            ++BackGroundHueCounter;
            BackGroundHueColor = new Color(255 - delta, delta, 0);
            for (int i = 0; i < ActiveGibs.Count; ++i)
            {
                ExplodedPart part = ActiveGibs[i];
                bool hasStopped;
                part.Update(out hasStopped);
                if (hasStopped)
                {
                    BakedGibs.Add(part);
                    ActiveGibs.Remove(part);
                    i--;
                }
            }
        }

        public void ProcessInput(Player p, TouchCollection input)
        {
            p.Moving = true;
            bool isFireDown = false;
            bool isStopDown = false;
            RotationDelta = 0;
            foreach (TouchLocation touch in input)
            {
                Vector2 vec = touch.Position;

                if (vec.X > GameWidth / 2)
                {
                    isFireDown = true;
                }
                else
                {
                    isStopDown = true;
                }
                
            }

            p.ProcessInput(isFireDown, isStopDown);
        }

        public void Draw(SpriteBatch spriteBatch, Player p)
        {

        }
        public void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_Background, new Vector2(0, 0), null, Color.White, 0.0f, new Vector2(0,0), new Vector2(1,1), SpriteEffects.None, 0.0f);
        }
        public void DrawActiveGibs(SpriteBatch spriteBatch)
        {
            foreach (ExplodedPart part in ActiveGibs)
            {
                part.Draw(spriteBatch, BackGroundHueColor);
            }
        }
        public void DrawBakedGibs(SpriteBatch spriteBatch, PhysicsManager manager)
        {
            foreach (ExplodedPart part in BakedGibs)
            {
                part.DrawOffset(spriteBatch, BackGroundHueColor);
                part.CleanBody(manager);
            }
            BakedGibs.Clear();
        }

        private Vector2 DeathTimerScale;
        private Vector2 DeathTimerOrigin;
        private Vector2 DeathTimerPosition;
        public void DrawDeathTimer(SpriteBatch spriteBatch)
        {
            string timeToDeathString = TimeToDeath.ToString(@"mm\:ss\:ff");
            spriteBatch.DrawString(ColunaFont, timeToDeathString, DeathTimerPosition, Color.Black * 0.75f, 0, 
                                    DeathTimerOrigin, DeathTimerScale, SpriteEffects.None, 0.0f);
        }

        private Vector2 CountdownScale;
        private Vector2 CountdownOrigin;
        private Vector2 CountdownPosition;
        public void DrawCountdown(SpriteBatch spriteBatch, TimeSpan countdown)
        {
            string countdownString = countdown.ToString(@"ss");
            spriteBatch.DrawString(ColunaFont, countdownString, CountdownPosition, Color.Blue * 0.45f, 0,
                         CountdownOrigin, CountdownScale, SpriteEffects.None, 0.0f);
        }

        public void SetTimeToDeath(TimeSpan time)
        {
            TimeToDeath = time;
        }
        public void DrawMainMenu(SpriteBatch spriteBatch)
        {
            
        }
        public void DrawSkullBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_SkullBackground, new Vector2(0, 0), null, Color.White, 0.0f, new Vector2(0, 0), new Vector2(1, 1), SpriteEffects.None, 0.0f);
        }
        public void DrawPlayerStatusText(string text)
        {

        }
    }
}
