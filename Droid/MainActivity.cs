using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace OpenGloveApp.Droid
{
    [Activity(Label = "MainActivity.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme.Base", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static UIHandler mUIHandler;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            // START Handle message for Android UI thread
            mUIHandler = new UIHandler(Android.OS.Looper.MainLooper);
            // END Handle message for Android UI thread

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());

        }

        // START Handle message for Android UI thread
        public class UIHandler : Android.OS.Handler
        {
            public UIHandler(Android.OS.Looper looper) : base(Android.OS.Looper.MainLooper)
            {

            }
            public override void HandleMessage(Android.OS.Message msg)
            {
                base.HandleMessage(msg);
                Android.Util.Log.Error("UI_THREAD: ", (string) msg.Obj);
            }
        }
        // END Handle message for Android UI thread
    }
}
