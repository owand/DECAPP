using DECAPP.Models;
using DECAPP.Resources;
using DECAPP.Views.Settings;
using SQLite;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
[assembly: ExportFont("MaterialDesignIcons.ttf", Alias = "MaterialIcons")]
namespace DECAPP
{
    public partial class App : Application
    {
        // Переменные для базы данных
        public static SQLiteConnection database;
        public static SQLiteConnection Database
        {
            get
            {
                try
                {
                    database = new SQLiteConnection(Constants.DatabasePath, Constants.Flags, false);
                    return database;
                }
                catch (Exception ex)
                {
                    Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                    System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                    return database = null;
                }
            }
        }

        // Переменные для подключения приложения к личному account Microsoft, используя Microsoft Graph API
        public static string ClientID = "78eeb745-6cef-4309-8d6e-6505c7071c9e";
        public static string[] Scopes = { "Files.ReadWrite.All", "Files.ReadWrite.AppFolder" };
        public static object ParentWindow { get; set; }

        //переменные для изменения локализации приложения
        public static string AppLanguage
        {
            get => Xamarin.Essentials.Preferences.Get("currentLanguage", "ru");
            set => Xamarin.Essentials.Preferences.Set("currentLanguage", value);
        }

        public static string ENERGY // Переменая фильтра по виду топлива
        {
            get => Xamarin.Essentials.Preferences.Get("ENERGYNEW", string.Empty);
            set => Xamarin.Essentials.Preferences.Set("ENERGYNEW", value);
        }
        public static string BaseTEMP // Переменая фильтра по базовой температуре
        {
            get => Xamarin.Essentials.Preferences.Get("BaseTEMPNEW", string.Empty);
            set => Xamarin.Essentials.Preferences.Set("BaseTEMPNEW", value);
        }


        //переменные для применения темы
        public static string AppTheme
        {
            get => Xamarin.Essentials.Preferences.Get("currentTheme", "myOSTheme");
            set => Xamarin.Essentials.Preferences.Set("currentTheme", value);
        }

        //переменные для Purchases State
        public static readonly string ProductID = "statepro";

        public static bool ProState
        {
            get => Xamarin.Essentials.Preferences.Get("ProState", true);
            set => Xamarin.Essentials.Preferences.Set("ProState", value);
        }



        public App()
        {
            Device.SetFlags(new string[] { "MediaElement_Experimental", "Shell_UWP_Experimental", "Visual_Experimental",
                                           "CollectionView_Experimental", "FastRenderers_Experimental", "CarouselView_Experimental",
                                           "IndicatorView_Experimental", "RadioButton_Experimental", "AppTheme_Experimental",
                                           "Markup_Experimental", "Expander_Experimental" });
            InitializeComponent();

            // Языковая культура приложения должна определяться как можно раньше.
            AppResource.Culture = new CultureInfo(AppLanguage);

            // Theme of the application
            switch (AppTheme)
            {
                case "myDarkTheme":
                    App.Current.UserAppTheme = OSAppTheme.Dark;
                    break;

                case "myLightTheme":
                    App.Current.UserAppTheme = OSAppTheme.Light;
                    break;

                case "myOSTheme":
                    App.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;

                default:
                    App.Current.UserAppTheme = OSAppTheme.Unspecified;
                    break;
            }

            if (ProState == false)
            {
                Device.BeginInvokeOnMainThread(async () => { await Settings.ProVersionCheck(); });
            }

            //MainPage = new AppShell();
        }

        protected async override void OnStart()
        {
            // Handle when your app starts
            try
            {
                if (!File.Exists(Constants.DatabasePath))
                {
                    await CopyDBifNotExists();
                }
                else if (GetCurrentDBVersion() < Constants.dbVersion)
                {
                    Database.Dispose();
                    Database.Close();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    await CopyDBifNotExists();
                    await Application.Current.MainPage.DisplayAlert("Congratulations! ", " The database has been updated!", AppResource.messageOk); // Что-то пошло не так
                }
                this.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }


        public async Task CopyDBifNotExists()
        {
            try
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{GetType().Namespace}.{Constants.dbName}");
                if (stream == null)
                {
                    await Current.MainPage.DisplayAlert(AppResource.messageError, "The resource " + Constants.dbName + " was not loaded properly.", AppResource.messageOk); // Что-то пошло не так
                    System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                    return;
                }

                // если база данных не существует (еще не скопирована)

                //вариант 1
                using (new StreamReader(stream))
                {
                    using (FileStream fs = new FileStream(Constants.DatabasePath, FileMode.Create))
                    {
                        stream.CopyTo(fs);  // копируем файл базы данных в нужное нам место
                        fs.Flush();
                    }
                }

                //вариант 2
                //BinaryReader br = new BinaryReader(stream);
                //using (br)
                //{
                //    //FileStream fs = new FileStream(Constants.DatabasePath, FileMode.Create);
                //    using (BinaryWriter bw = new BinaryWriter(new FileStream(Constants.DatabasePath, FileMode.Create)))
                //    {
                //        byte[] buffer = new byte[2048];
                //        int len;
                //        while ((len = br.Read(buffer, 0, buffer.Length)) > 0)
                //        {
                //            bw.Write(buffer, 0, len);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }


        // Get current Data Base Version
        public int GetCurrentDBVersion()
        {
            int currentDbVersion;
            try
            {
                if (Database != null)
                {
                    currentDbVersion = Database.ExecuteScalar<int>("pragma user_version");
                    Database.Close();
                    Database.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                else
                {
                    currentDbVersion = 0;
                }
                return currentDbVersion;
            }
            catch (Exception ex)
            {
                currentDbVersion = 0;
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return currentDbVersion;
            }
        }
    }
}