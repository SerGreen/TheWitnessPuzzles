using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using TWP_Shared;

namespace TWP_Android
{
    [Activity(Label = "The Witness Puzzles"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , HardwareAccelerated = true
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.Locked
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class ActivityMain : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new TWPGame();
            SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }

        // Trying to get rid of floating three-dot button on some devices
        public override bool OnPrepareOptionsMenu(IMenu menu) => false;
    }
}

