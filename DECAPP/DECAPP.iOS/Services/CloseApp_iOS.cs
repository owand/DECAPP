using DECAPP.Services;
using System.Security;
using System.Threading;

[assembly: Xamarin.Forms.Dependency(typeof(DECAPP.iOS.Services.CloseApp_iOS))]
namespace DECAPP.iOS.Services
{
    public class CloseApp_iOS : ICloseApplication
    {
        [SecurityCritical]
        public void CloseApp()
        {
            Thread.CurrentThread.Abort();
        }
    }
}