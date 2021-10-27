using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace DECAPP.Services
{
    // Класс для конвертации массива байт в изображение.
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource imageSource = null;
            try
            {
                byte[] imageAsBytes = (byte[])value;
                if (value == null || value is DBNull)
                {
                    string imagePath;
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            imagePath = "default_image_app.png";
                            break;

                        case Device.iOS:
                            imagePath = "Images/default_image_app.png";
                            break;

                        case Device.UWP:
                            imagePath = "Assets/Images/default_image_app.png";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    imageSource = ImageSource.FromFile(imagePath); // Значение по умолчанию
                }
                else
                {
                    //imageSource = ImageSource.FromStream(() =>
                    //{
                    //    MemoryStream ms = new MemoryStream(imageAsBytes)
                    //    {
                    //        Position = 0
                    //    };
                    //    return ms;
                    //});
                    imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                    //imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes.ToArray()));
                }
                return imageSource;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}