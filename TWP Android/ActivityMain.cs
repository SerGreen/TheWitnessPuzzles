using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using System;
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

            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += AndroidExceptionHandler;

            var g = new TWPGame();
            SetContentView((View) g.Services.GetService(typeof(View)));

            g.Run();
        }


#region Error handling
        void AndroidExceptionHandler(object sender, Android.Runtime.RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            ShowMessageBox(
                "Fatal Error", 
                e.Exception.Message + "\n\n" + e.Exception.StackTrace,
                new EventHandler<Android.Content.DialogClickEventArgs>((sender, args) => { Android.OS.Process.KillProcess(Android.OS.Process.MyPid()); })
            );
        }

        private void ShowMessageBox(string caption, string message, EventHandler<Android.Content.DialogClickEventArgs> buttonClickHandler = null)
        {
            new AlertDialog.Builder(this)
                .SetNeutralButton("Close", buttonClickHandler)
                .SetMessage(message)
                .SetTitle(caption)
                .Show();
        }
#endregion

    }
}

