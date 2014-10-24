using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace TimeToLive
{
    public static class TextureBank
    {
        public static Dictionary<string, Texture2D> Bank = new Dictionary<string, Texture2D>();
        private static ContentManager TexContent;
        //public static void LoadTexture(string name, ContentManager content)
        //{
        //    if (Bank.ContainsKey(name))
        //    {
        //        return;
        //    }
        //    Bank[name] = content.Load<Texture2D>(name);
        //}
        public static Texture2D GetTexture(string name)
        {
            if (!Bank.ContainsKey(name))
            {
                Bank[name] = TexContent.Load<Texture2D>(name);
            }
            return Bank[name];
        }

        public static void SetContentManager(ContentManager content)
        {
            TexContent = content;
        }
        public static void ClearTextureBank()
        {
            Bank.Clear();
        }
    }
}
