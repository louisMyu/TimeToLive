using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimeToLive
{
    
    class DeathMenu : MenuScreen
    {
        SoundEffectInstance song;
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public DeathMenu(UnPauseGamePlayScreen callback)
            : base("You Died!")
        {
            // Create our menu entries.
            MenuEntry NewGameEntry = new MenuEntry("Try Again");
            MenuEntry UpgradeMenuEntry = new MenuEntry("Upgrade");
            MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry ExitGameMenuEntry = new MenuEntry("Return To Main Menu");

            // Hook up menu event handlers.
            NewGameEntry.Selected += NewGameMenuEntrySelected;
            UpgradeMenuEntry.Selected += UpgradeMenuEntrySelected;
            ExitGameMenuEntry.Selected += ReturnToMainEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(NewGameEntry);
            MenuEntries.Add(UpgradeMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(ExitGameMenuEntry);
            IsPopup = true;
            song = SoundBank.GetSoundInstance("menuMusic");
            StartbackgroundMusic();

            UnPauseGameCallback = callback;
        }

        public delegate void UnPauseGamePlayScreen();
        UnPauseGamePlayScreen UnPauseGameCallback;
        #endregion
        private void StartbackgroundMusic()
        {
            song.IsLooped = true;
            song.Play();
        }
        #region Handle Input

        void NewGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            song.Stop();
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen(GameplayScreen.GameState.Countdown)); 
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void ReturnToMainEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            song.Stop();
            UnPauseGameCallback();
            ScreenManager.AddScreen(new CustomMenuScreen(), null);
            OnCancel();
        }

        void UpgradeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            //this should push on the upgrade menu on top of the current menu
        }
        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            song.Stop();
            ExitScreen();
        }
        #endregion

        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected override void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new Vector2(0f, 175f);

            // update each menu entry's location in turn
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                // each entry is to be centered horizontally
                position.X = ScreenManager.NativeResolution.X / 2 - menuEntry.GetWidth(this) / 2;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry plus our padding
                position.Y += menuEntry.GetHeight(this) + (menuEntryPadding * 2);
            }
        }
    }
}
