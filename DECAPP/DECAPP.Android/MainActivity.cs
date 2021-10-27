using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System;

namespace DECAPP.Droid
{
    [Activity(Label = "@string/ApplicationName", Icon = "@drawable/icon", MainLauncher = false, Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 0, 0));
            Xamarin.Essentials.Platform.Init(this, bundle);
            Xamarin.Forms.Forms.Init(this, bundle);

            //use GlideX in my Xamarin.Forms
            //Force the custom renderers to get loaded
            //Android.Glide.Forms.Init(this, debug: true);
            // Or use this one instead to try IGlideHandler
            // Android.Glide.Forms.Init (this, handler: new RandomAlphaHandler (), debug: true);

            LoadApplication(new App());

            // Подключение приложения к личному account Microsoft, используя Microsoft Graph API
            App.ParentWindow = this;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            // Подключение приложения к личному account Microsoft, используя Microsoft Graph API
            Microsoft.Identity.Client.AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, intent);
        }

        // Xamarin.Essentials must receive any OnRequestPermissionsResult. Write the following code for runtime permission.
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //Clear cache and memory considerations
        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            //ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnTrimMemory(level);
        }
    }
}