using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Services
{
    [ContentProperty(nameof(SourceImage))]
    public class PlatformImageExtension : IMarkupExtension<string>
    {
        public string SourceImage { get; set; }

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            if (SourceImage == null)
            {
                return null;
            }

            string imagePath;
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    imagePath = SourceImage;
                    break;

                case Device.iOS:
                    imagePath = "Images/" + SourceImage + ".png";
                    break;

                case Device.UWP:
                    imagePath = "Assets/Images/" + SourceImage + ".png";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return imagePath;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}