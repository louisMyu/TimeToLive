using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToLive
{
    public static class SoundBank
    {
        public static Dictionary<string, SoundEffect> Bank = new Dictionary<string, SoundEffect>();
        private static ContentManager Content;
        public static SoundEffectInstance GetSoundInstance(string key)
        {
            if (!Bank.ContainsKey(key))
            {
                Bank[key] = Content.Load<SoundEffect>(key);
            }
            return Bank[key].CreateInstance();
        }

        public static Dictionary<string, Song> SongBank = new Dictionary<string, Song>();
        public static Song GetSong(string key)
        {
            if (!Bank.ContainsKey(key))
            {
                SongBank[key] = Content.Load<Song>(key);
            }
            return SongBank[key];
        }
        public static void SetContentManager(ContentManager content)
        {
            Content = content;
        }
        public static void ClearSoundBank()
        {
            Bank.Clear();
        }
    }
}
