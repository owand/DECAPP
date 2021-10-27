using DECAPP.Resources;
using DECAPP.Services;
using Microsoft.Graph;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Settings
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public Models.Settings settingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();

            BindingContext = settingsViewModel = new Models.Settings();

            PickerLanguages.SelectedIndexChanged += OnLanguagesChanged;
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true;

                labAppName.Text = AppInfo.Name; // Application Name
                switch (Xamarin.Forms.Device.RuntimePlatform)
                {
                    case Xamarin.Forms.Device.iOS:
                    case Xamarin.Forms.Device.Android:
                        labAppVersion.Text = $"{AppInfo.VersionString}." + $"{AppInfo.BuildString}"; // Application Version (1.0.0)
                        break;

                    case Xamarin.Forms.Device.UWP:
                        labAppVersion.Text = $"{AppInfo.VersionString}"; // Application Version (1.0.0)
                        break;

                    default:
                        break;
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Происходит, когда ширина или высота свойств измените значение на этот элемент.
        private void OnSizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current.Width > 1000)
                {
                    About.SetValue(Grid.ColumnProperty, 2);
                    About.SetValue(Grid.RowProperty, 0);
                    SettingsContent.ColumnDefinitions[2].Width = 500;
                }
                else
                {
                    About.SetValue(Grid.ColumnProperty, 0);
                    About.SetValue(Grid.RowProperty, 1);
                    SettingsContent.ColumnDefinitions[2].Width = 0;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnLanguagesChanged(object sender, EventArgs e)
        {
            try
            {
                Xamarin.Essentials.Preferences.Set("currentLanguage", settingsViewModel?.LangCollection[PickerLanguages.SelectedIndex].LANGNAME);
                AppResource.Culture = new System.Globalization.CultureInfo(App.AppLanguage);

                App.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); }); // Что-то пошло не так
                return;
            }
        }

        private void OnThemesChanged(object sender, EventArgs e)
        {
            try
            {
                Xamarin.Essentials.Preferences.Set("currentTheme", settingsViewModel?.ThemesCollection[PickerThemes.SelectedIndex].THEMENAME);

                switch (settingsViewModel.ThemesCollection[PickerThemes.SelectedIndex].THEMENAME)
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
            }
            catch (Exception ex)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); }); // Что-то пошло не так
                return;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert(AppResource.messageTitleExit, AppResource.messageExit, AppResource.messageOk, AppResource.messageСancel))
                    {
                        System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                    }
                });
            }
            catch { return false; }
            // Always return true because this method is not asynchronous.
            // We must handle the action ourselves: see above.
            return true;
        }

        //------------------Tapped----------------------
        #region Tapped events

        private void OnTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync(settingsViewModel._SelectedItem.TargetName); });
                //MasterDetailPage mainPage = App.Current.MainPage as MasterDetailPage;
                //mainPage.Detail = new NavigationPage((Page)Activator.CreateInstance(settingsViewModel._SelectedItem.TargetType));
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); });
                return;
            }
        }

        private void Tapped_privacyPolicy(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://sites.google.com/view/owand/privacy"));
        }

        private void Tapped_siteProject(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("https://sites.google.com/view/owand/drilling-ecatalog"));
        }

        private void Tapped_mailAuthor(object sender, EventArgs e)
        {
            Launcher.OpenAsync(new Uri("mailto:plowand@outlook.com"));
        }

        private void OpenReviewStore(object sender, EventArgs e)
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    OpenGooglePlay();
                    break;
                case Xamarin.Forms.Device.UWP:
                    OpenMicrosoftStore();
                    break;
                default:
                    break;
            }
        }

        private void OpenStore(object sender, EventArgs e)
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    OpenMicrosoftStore();
                    break;
                case Xamarin.Forms.Device.UWP:
                    OpenGooglePlay();
                    break;
                default:
                    break;
            }
        }

        private void OpenMicrosoftStore()
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case Xamarin.Forms.Device.Android:
                    Launcher.OpenAsync(new Uri("https://www.microsoft.com/store/apps/9P1C0XGRCMNX"));
                    break;
                case Xamarin.Forms.Device.UWP:
                    Launcher.OpenAsync(new Uri("ms-windows-store://review/?productid=9P1C0XGRCMNX"));
                    break;
                default:
                    break;
            }
        }

        private void OpenGooglePlay()
        {
            Launcher.OpenAsync(new Uri("https://play.google.com/store/apps/details?id=com.plowand.decapp"));
        }

        #endregion

        //------------------Backup----------------------
        #region Backup events
        private async void OnUploadBackupAsync(object sender, EventArgs e)
        {
            switch (Connectivity.NetworkAccess)
            {
                case NetworkAccess.Internet:
                case NetworkAccess.ConstrainedInternet:
                    // Connection to internet is available
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.DBUpload, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog)
                    {
                        IsBusy = true;  // Затеняем задний фон и запускаем ProgressRing
                        labActivityUpload.IsVisible = true;
                        SettingsContent.IsEnabled = false;

                        try
                        {
                            App.Database.Dispose();
                            App.Database.Close();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            // Создание сеанса загрузки
                            UploadSession uploadSession = await AuthenticationHelper.GetAuthenticatedClient().Me.Drive.Special.AppRoot.ItemWithPath(Constants.dbName).CreateUploadSession().Request().PostAsync();
                            Stream fileStream = System.IO.File.OpenRead(Constants.DatabasePath);

                            using (fileStream)
                            {
                                // Создание задачи
                                LargeFileUploadTask<DriveItem> fileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, 320 * 1024);

                                // Загрузить файл
                                UploadResult<DriveItem> uploadResult = null;
                                uploadResult = await fileUploadTask.UploadAsync();
                                DriveItem itemResult = null;
                                itemResult = uploadResult.ItemResponse;

                                if (uploadResult.UploadSucceeded)
                                {
                                    await DisplayAlert(AppResource.messageSuccess, AppResource.messageSuccessUpload, AppResource.messageOk);
                                }
                            }

                            fileStream.Close();
                            fileStream.Dispose();

                            App.Database.Commit();
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert(AppResource.messageError, AppResource.BackupError + "\n\n" + ex.Message, AppResource.messageOk);
                        }
                        IsBusy = false;
                        labActivityUpload.IsVisible = false;
                        SettingsContent.IsEnabled = true;
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:

                    await DisplayAlert(AppResource.messageError, "Oops, looks like you don't have internet connection :(", AppResource.messageOk); // Что-то пошло не так
                    return;
            }
        }

        private async void OnDownloadBackup(object sender, EventArgs e)
        {
            switch (Connectivity.NetworkAccess)
            {
                case NetworkAccess.Internet:
                case NetworkAccess.ConstrainedInternet:
                    // Connection to internet is available
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.DBDownload, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog)
                    {
                        IsBusy = true; // Затеняем задний фон и запускаем ProgressRing
                        labActivityDownload.IsVisible = true;
                        SettingsContent.IsEnabled = false;

                        Stream contentStream;
                        DriveItem foundFile;
                        Stream fileStream;

                        try
                        {
                            App.Database.Dispose();
                            App.Database.Close();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            IDriveItemRequestBuilder request = AuthenticationHelper.GetAuthenticatedClient().Me.Drive.Special.AppRoot.ItemWithPath(Constants.dbName);
                            foundFile = await request.Request().GetAsync();
                            fileStream = new FileStream(Constants.DatabasePath, FileMode.Open);

                            if (foundFile == null)
                            {
                                await DisplayAlert(AppResource.messageError, "Нет резервной копии базы данных!", AppResource.messageOk);
                                IsBusy = false;
                                return;
                            }

                            contentStream = await request.Content.Request().GetAsync();

                            if (contentStream == null)
                            {
                                await DisplayAlert(AppResource.messageError, "Резервная копии базы данных пуста!", AppResource.messageOk);
                                IsBusy = false;
                                return;
                            }

                            // Save the retrieved stream to the local drive
                            using (fileStream)
                            {
                                using (BinaryWriter writer = new BinaryWriter(fileStream))
                                {
                                    contentStream.Position = 0;
                                    using (BinaryReader reader = new BinaryReader(contentStream))
                                    {
                                        byte[] bytes;
                                        do
                                        {
                                            bytes = reader.ReadBytes(1024);
                                            writer.Write(bytes);
                                        }
                                        while (bytes.Length == 1024);
                                    }
                                }
                            }

                            fileStream.Close();
                            fileStream.Dispose();
                            contentStream.Close();
                            contentStream.Dispose();
                            await DisplayAlert(AppResource.messageSuccess, AppResource.messageSuccessDownload, AppResource.messageOk);
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                        }
                        IsBusy = false;
                        labActivityDownload.IsVisible = false;
                        SettingsContent.IsEnabled = true;
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:

                    await DisplayAlert(AppResource.messageError, "Oops, looks like you don't have internet connection :(", AppResource.messageOk); // Что-то пошло не так
                    return;
            }
        }

        #endregion

        //------------------Purchases----------------------
        #region Purchases events

        private async void ProVersionPurchase(object sender, EventArgs e)
        {
            try
            {
                await settingsViewModel.ProVersionPurchase();
            }
            catch (Exception ex) // Что-то пошло не так
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                return;
            }
        }

        #endregion
    }
}