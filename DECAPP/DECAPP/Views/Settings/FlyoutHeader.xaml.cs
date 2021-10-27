using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlyoutHeader : ContentView
    {
        public FlyoutHeader()
        {
            InitializeComponent();

            //labAppName.Text = AppInfo.Name; // Application Name
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    labAppVersion.Text = $"{AppInfo.VersionString}." + $"{AppInfo.BuildString}"; // Application Version (1.0.0)
                    break;

                case Device.UWP:
                    labAppVersion.Text = $"{AppInfo.VersionString}"; // Application Version (1.0.0)
                    break;

                default:
                    break;
            }
        }
    }
}