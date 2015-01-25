using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MonoGame.Framework;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace TimeToLive
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : SwapChainBackgroundPanel
    {
        readonly Game1 _game;
        public GamePage()
        {
            this.InitializeComponent();
            hideShit();
        }

        public GamePage(string launchArguments)
        {
            _game = XamlGame<Game1>.Create(launchArguments, Window.Current.CoreWindow, this);
            hideShit();
        }

        //why was it so hard to find how to do this?!
        async void hideShit()
        {
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;
        }
    }
}
