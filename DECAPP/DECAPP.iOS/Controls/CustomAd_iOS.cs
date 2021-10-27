using DECAPP.Controls;
using DECAPP.iOS.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomAd), typeof(CustomAd_iOS))]
namespace DECAPP.iOS.Controls
{
    public class CustomAd_iOS : ViewRenderer
    {
        //[SecurityCritical]
        //[PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            //switch (Connectivity.NetworkAccess)
            //{
            //    case NetworkAccess.Internet:
            //    case NetworkAccess.ConstrainedInternet:
            //        break;

            //    default:
            //        return;
            //}

            //if (DECAPP.App.ProStateCurrent || DECAPP.App.AdStateCurrent)
            //{
            //    return;
            //}

            //if (Control == null)
            //{
            //    BannerView bannerView = new BannerView(AdSizeCons.SmartBannerPortrait)
            //    {

            //        //#if DEBUG   // test Unit Id
            //        //                    AdUnitID = "ca-app-pub-3940256099942544/6300978111",
            //        //#else       // real Unit Id
            //        //                    AdUnitID = "ca-app-pub-3441985749782200/5396403662",
            //        //#endif
            //        RootViewController = GetVisibleViewController()
            //    };

            //    bannerView.LoadRequest(GetRequest());

            //    Request GetRequest()
            //    {
            //        Request request = Request.GetDefaultRequest();
            //        return request;
            //    }
            //}
        }

        private UIViewController GetVisibleViewController()
        {
            UIWindow[] windows = UIApplication.SharedApplication.Windows;
            foreach (UIWindow window in windows)
            {
                if (window.RootViewController != null)
                {
                    return window.RootViewController;
                }
            }
            return null;
        }

    }
}