using DECAPP.Models.Equipment;
using DECAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Equipment
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FuelDenCalcPage : ContentPage
    {
        private FuelDenCalcViewModel viewModel;

        private static string TableSYMBOL
        {
            get => Xamarin.Essentials.Preferences.Get("TableSYMBOL", "A");
            set => Xamarin.Essentials.Preferences.Set("TableSYMBOL", value);
        }
        private static double DENSITY
        {
            get => Xamarin.Essentials.Preferences.Get("DENSITY", 0.00);
            set => Xamarin.Essentials.Preferences.Set("DENSITY", value);
        }
        private static double TEMP
        {
            get => Xamarin.Essentials.Preferences.Get("TEMP", 0.00);
            set => Xamarin.Essentials.Preferences.Set("TEMP", value);
        }


        public FuelDenCalcPage()
        {
            InitializeComponent();
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true;

                BindingContext = viewModel = viewModel ?? new FuelDenCalcViewModel();

                entrDENSITY.Text = DENSITY.ToString("N2");
                entrTEMP.Text = TEMP.ToString("N2");

                entrDENSITY.Focus();

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

        // Происходит, когда ширина или высота свойств измените значение на этот элемент.
        private void OnSizeChanged(object sender, EventArgs e)
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Width > 1000)
                        {
                            //Formula.SetValue(Grid.ColumnProperty, 2);
                            //Formula.SetValue(Grid.RowProperty, 1);
                            FormulaContent.ColumnDefinitions[2].Width = 500;
                        }
                        else
                        {
                            //Formula.SetValue(Grid.ColumnProperty, 0);
                            //Formula.SetValue(Grid.RowProperty, 0);
                            FormulaContent.ColumnDefinitions[2].Width = 0;
                        }
                        break;

                    case TargetIdiom.Phone:
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void SwitchObserved(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                entrDENSITY.Text = string.Empty;
                entrTEMP.Text = string.Empty;
                resultDENSITY.Text = string.Empty;

                if (metricObserved.IsChecked)
                {
                    metricObserved.IsEnabled = false;

                    metricBase.IsChecked = false;
                    metricBase.IsEnabled = true;

                    switch (App.BaseTEMP)
                    {
                        case "15":
                            TableSYMBOL = "A"; // Application Version (1.0.0)
                            break;

                        case "20":
                            TableSYMBOL = "B"; // Application Version (1.0.0)
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void SwitchBase(object sender, CheckedChangedEventArgs e)
        {
            try
            {
                entrDENSITY.Text = string.Empty;
                DENSITY = 0.00;
                entrTEMP.Text = string.Empty;
                TEMP = 0.00;
                resultDENSITY.Text = string.Empty;

                if (metricBase.IsChecked)
                {
                    metricBase.IsEnabled = false;

                    metricObserved.IsChecked = false;
                    metricObserved.IsEnabled = true;

                    switch (App.BaseTEMP)
                    {
                        case "15":
                            TableSYMBOL = "V"; // Application Version (1.0.0)
                            break;

                        case "20":
                            TableSYMBOL = "G"; // Application Version (1.0.0)
                            break;

                        default:
                            break;
                    }

                    //OnNETtoGROSS();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            try
            {
                entrDENSITY.Text = string.Empty;
                DENSITY = 0.00;
                entrTEMP.Text = string.Empty;
                TEMP = 0.00;

                resultDENSITY.Text = string.Empty;
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnTEMPChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                resultDENSITY.Text = string.Empty;

                if (!string.IsNullOrWhiteSpace(e.NewTextValue) && !(e.NewTextValue.Length == 1 && e.NewTextValue.StartsWith("-")))
                {
                    TEMP = Convert.ToDouble(entrTEMP.Text);
                }
            }
            catch (Exception)
            {
                DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnDENSITYChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                resultDENSITY.Text = string.Empty;

                if (!string.IsNullOrWhiteSpace(e.NewTextValue) && !(e.NewTextValue.Length == 1 && e.NewTextValue.StartsWith("-")))
                {
                    DENSITY = Convert.ToDouble(entrDENSITY.Text);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnBaseTEMPChanged(object sender, EventArgs e)
        {
            try
            {
                App.BaseTEMP = viewModel.BaseTempList[picTEMP.SelectedIndex].BASETEMP.ToString();

                resultDENSITY.Text = string.Empty;

                if (metricObserved.IsChecked)
                {
                    switch (App.BaseTEMP)
                    {
                        case "15":
                            TableSYMBOL = "A";
                            break;

                        case "20":
                            TableSYMBOL = "B";
                            break;

                        default:
                            break;
                    }
                }
                else if (metricBase.IsChecked)
                {
                    switch (App.BaseTEMP)
                    {
                        case "15":
                            TableSYMBOL = "V";
                            break;

                        case "20":
                            TableSYMBOL = "G";
                            break;

                        default:
                            break;
                    }
                }

                picTEMP.BackgroundColor = Color.Transparent;
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnENERGYChanged(object sender, EventArgs e)
        {
            try
            {
                App.ENERGY = viewModel.EnergyList[picENERGY.SelectedIndex].ENERGYID.ToString();

                resultDENSITY.Text = string.Empty;

                picENERGY.BackgroundColor = Color.Transparent;
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private async void OnCalculate(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                viewModel.Collection = null;
                viewModel.Collection = viewModel.GetCollection(App.ENERGY, App.BaseTEMP, TableSYMBOL);

                IsBusy = false;
                resultDENSITY.Text = string.Empty;

                if ((picENERGY.SelectedIndex < 0) || (picTEMP.SelectedIndex < 0) || string.IsNullOrEmpty(entrDENSITY.Text) || string.IsNullOrEmpty(entrTEMP.Text) || viewModel.Collection.Count == 0)
                {
                    await DisplayAlert(AppResource.messageAttention, AppResource.FormulaError, AppResource.messageСancel); // Что-то пошло не так
                    return;
                }

                double minBaseDENSITY = 0.000;
                double maxBaseDENSITY = 0.000;
                double MinTEMP = 0.000;
                double MaxTEMP = 0.000;
                double MinDENSITY = 0.000;
                double MaxDENSITY = 0.000;
                double ObservedDENSITY = 0.000;

                if (!viewModel.Collection.Where(X => X.BASEDENSITY < DENSITY).Any() || !viewModel.Collection.Where(X => X.BASEDENSITY > DENSITY).Any() ||
                !viewModel.Collection.Where(X => X.TEMP > TEMP).Any() || !viewModel.Collection.Where(X => X.TEMP < TEMP).Any())
                {
                    await DisplayAlert(AppResource.messageAttention, AppResource.FormulaError, AppResource.messageСancel); // Что-то пошло не так
                    return;
                }

                minBaseDENSITY = viewModel.Collection.Where(X => X.BASEDENSITY < DENSITY).Last().BASEDENSITY;
                maxBaseDENSITY = viewModel.Collection.Where(X => X.BASEDENSITY >= DENSITY).First().BASEDENSITY;

                MinTEMP = viewModel.Collection.Where(X => X.TEMP < TEMP).Last().TEMP;
                MaxTEMP = viewModel.Collection.Where(X => X.TEMP > TEMP).First().TEMP;


                //if (minBaseDENSITY == 0 || maxBaseDENSITY == 0 || MinTEMP == 0 || maxBaseDENSITY == 0)
                //if (minBaseDENSITY == 0 || maxBaseDENSITY == 0)
                //{
                //    Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageAttention, AppResource.FormulaError, AppResource.messageСancel); }); // Что-то пошло не так
                //    return;
                //}

                double MinDENSITY_MinTEMP = viewModel.Collection.Where(X => X.TEMP == MinTEMP && X.BASEDENSITY == minBaseDENSITY).FirstOrDefault().DENSITY;
                double MaxDENSITY_MinTEMP = viewModel.Collection.Where(X => X.TEMP == MinTEMP && X.BASEDENSITY == maxBaseDENSITY).FirstOrDefault().DENSITY;
                MinDENSITY = MinDENSITY_MinTEMP + (DENSITY - minBaseDENSITY) * (MaxDENSITY_MinTEMP - MinDENSITY_MinTEMP) / 10;

                double MinDENSITY_MaxTEMP = viewModel.Collection.Where(X => X.TEMP == MaxTEMP && X.BASEDENSITY == minBaseDENSITY).FirstOrDefault().DENSITY;
                double MaxDENSITY_MaxTEMP = viewModel.Collection.Where(X => X.TEMP == MaxTEMP && X.BASEDENSITY == maxBaseDENSITY).FirstOrDefault().DENSITY;
                MaxDENSITY = MinDENSITY_MaxTEMP + (DENSITY - minBaseDENSITY) * (MaxDENSITY_MaxTEMP - MinDENSITY_MaxTEMP) / 10;

                ObservedDENSITY = (MinDENSITY + MaxDENSITY) / 2;

                resultDENSITY.Text = ObservedDENSITY.ToString("N2");
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
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