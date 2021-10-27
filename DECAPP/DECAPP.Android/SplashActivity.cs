using Android.App;
using Android.OS;
using Android.Views.Animations;
using Android.Widget;

namespace DECAPP.Droid
{
    [Activity(Icon = "@drawable/icon", Theme = "@style/Theme.Transparent", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.SplashScreen);

            ImageView imageView = (ImageView)FindViewById(Resource.Id.SplashImageView);
            Animation splash_animation = AnimationUtils.LoadAnimation(this, Resource.Animation.splash_animation);
            imageView.StartAnimation(splash_animation);

            splash_animation.AnimationEnd += (object sender, Animation.AnimationEndEventArgs e) =>
            {
                StartActivity(typeof(MainActivity));
                Finish();
            };
            //StartActivity(typeof(MainActivity));
            //Finish();
        }




    }
}