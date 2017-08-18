using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using TWP_Shared;

namespace TWP_Android
{
    [Activity(Label = "TWP Android"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class ActivityMain : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new TWPGameAndroid();
            SetContentView((View) g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

