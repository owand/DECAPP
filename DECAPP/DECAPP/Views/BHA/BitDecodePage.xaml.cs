using DECAPP.Models.BHA;
using DECAPP.Resources;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.BHA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BitDecodePage : ContentPage
    {
        private BitDecode viewModel = null;
        private BitTypeListViewModel typeListViewModel = null;

        private int BitType;
        private string Symbol_1 = null;
        private string Symbol_2 = null;
        private string Symbol_3 = null;
        private string Symbol_4 = null;

        public BitDecodePage()
        {
            InitializeComponent();
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                BindingContext = viewModel = viewModel ?? new BitDecode(BitType);

                picTYPENAME.BindingContext = typeListViewModel = typeListViewModel ?? new BitTypeListViewModel();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void Current_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.CanCancel)
            {
                e.Cancel(); // Позволяет отменить навигацию
                OnBackButtonPressed();
            }
        }

        private void ChangeType(object sender, EventArgs e)
        {
            try
            {
                pic1Symbol.SelectedIndex = -1;
                pic1Symbol.SelectedItem = null;
                Symbol_1 = null;
                pic2Symbol.SelectedIndex = -1;
                pic2Symbol.SelectedItem = null;
                Symbol_2 = null;
                pic3Symbol.SelectedIndex = -1;
                pic3Symbol.SelectedItem = null;
                Symbol_3 = null;
                pic4Symbol.SelectedIndex = -1;
                pic4Symbol.SelectedItem = null;
                Symbol_4 = null;
                DecodeListContent.ItemsSource = null;
                DecodeListContent.Behaviors.Clear();

                BitType = typeListViewModel.TypePickerList[picTYPENAME.SelectedIndex].TYPEID;

                viewModel = null;
                viewModel = new BitDecode(BitType);
                BindingContext = viewModel;
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        private void ChangeSymbol(object sender, EventArgs e)
        {
            try
            {
                if (pic1Symbol.SelectedIndex >= 0)
                {
                    Symbol_1 = viewModel?.Code1SymbolList[pic1Symbol.SelectedIndex].BITCODEID.ToString();
                }

                if (pic2Symbol.SelectedIndex >= 0)
                {
                    Symbol_2 = viewModel?.Code2SymbolList[pic2Symbol.SelectedIndex].BITCODEID.ToString();
                }

                if (pic3Symbol.SelectedIndex >= 0)
                {
                    Symbol_3 = viewModel?.Code3SymbolList[pic3Symbol.SelectedIndex].BITCODEID.ToString();
                }

                if (pic4Symbol.SelectedIndex >= 0)
                {
                    Symbol_4 = viewModel?.Code4SymbolList[pic4Symbol.SelectedIndex].BITCODEID.ToString();
                }

                DecodeListContent.ItemsSource = viewModel?.DecodeContent(BitType, Symbol_1, Symbol_2, Symbol_3, Symbol_4); // Загружаем в ListView записи из таблицы.
            }
            catch (Exception)
            {
                viewModel = null;
                viewModel = new BitDecode(BitType);
                BindingContext = viewModel;
            }
        }

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                viewModel = null;
                Shell.Current.GoToAsync("..", true);
            }
            catch { return false; }
            // Always return true because this method is not asynchronous.
            // We must handle the action ourselves: see above.
            return true;
        }
    }
}