using DECAPP.Resources;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Other
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BodyDropPage : ContentPage
    {

        private static double DENSITYENVIRONMENT
        {
            get => Xamarin.Essentials.Preferences.Get("BD_DENSITYENVIRONMENT", 0.00);
            set => Xamarin.Essentials.Preferences.Set("BD_DENSITYENVIRONMENT", value);
        }
        private static double DENSITYBODY
        {
            get => Xamarin.Essentials.Preferences.Get("BD_DENSITYBODY", 0.00);
            set => Xamarin.Essentials.Preferences.Set("BD_DENSITYBODY", value);
        }
        private static double DISTANCE
        {
            get => Xamarin.Essentials.Preferences.Get("BD_DISTANCE", 0.00);
            set => Xamarin.Essentials.Preferences.Set("BD_DISTANCE", value);
        }
        private static double DIAMETERBALL
        {
            get => Xamarin.Essentials.Preferences.Get("DIAMETERBALL", 0.00);
            set => Xamarin.Essentials.Preferences.Set("DIAMETERBALL", value);
        }
        private static double DROPBODYFACTOR
        {
            get => Xamarin.Essentials.Preferences.Get("DROPBODYFACTOR", 55.20);
            set => Xamarin.Essentials.Preferences.Set("DROPBODYFACTOR", value);
        }

        public BodyDropPage()
        {
            InitializeComponent();
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
            IsBusy = false;
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                if (DISTANCE != 0.00)
                {
                    entrDISTANCE.Text = DISTANCE.ToString("N2");
                }
                if (DENSITYBODY != 0.00)
                {
                    entrDENSITYBODY.Text = DENSITYBODY.ToString("N2");
                }
                if (DIAMETERBALL != 0.00)
                {
                    entrDIAMETERBALL.Text = DIAMETERBALL.ToString("N2");
                }
                if (DROPBODYFACTOR != 0.00)
                {
                    entrDROPBODYFACTOR.Text = DROPBODYFACTOR.ToString("N2");
                }
                entrDENSITYENVIRONMENT.Text = DENSITYENVIRONMENT.ToString("N2");

                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                entrDISTANCE.Focus();

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
                            Formula.SetValue(Grid.ColumnProperty, 2);
                            Formula.SetValue(Grid.RowProperty, 1);
                            FormulaContent.ColumnDefinitions[2].Width = 500;
                        }
                        else
                        {
                            Formula.SetValue(Grid.ColumnProperty, 0);
                            Formula.SetValue(Grid.RowProperty, 3);
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
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            try
            {
                entrDISTANCE.Text = string.Empty;
                entrDENSITYBODY.Text = string.Empty;
                entrDENSITYENVIRONMENT.Text = string.Empty;
                entrDIAMETERBALL.Text = string.Empty;
                entrDROPBODYFACTOR.Text = string.Empty;
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

        // Событие при изменении текста в соответствующих полях.
        private void OnDISTANCEChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                if (!string.IsNullOrWhiteSpace(entrDISTANCE.Text))
                {
                    DISTANCE = Convert.ToDouble(entrDISTANCE.Text);
                }
                else
                {
                    DISTANCE = 0.00;
                }
            }
            catch (Exception)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk);
                });
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnDENSITYBODYChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                if (!string.IsNullOrWhiteSpace(entrDENSITYBODY.Text))
                {
                    DENSITYBODY = Convert.ToDouble(entrDENSITYBODY.Text);
                }
                else
                {
                    DENSITYBODY = 0.00;
                }
            }
            catch (Exception)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk);
                });
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnDENSITYENVIRONMENTChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                if (!string.IsNullOrWhiteSpace(entrDENSITYENVIRONMENT.Text))
                {
                    DENSITYENVIRONMENT = Convert.ToDouble(entrDENSITYENVIRONMENT.Text);
                }
                else
                {
                    DENSITYENVIRONMENT = 0.00;
                }
            }
            catch (Exception)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk);
                });
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnDIAMETERBALLChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                if (!string.IsNullOrWhiteSpace(entrDIAMETERBALL.Text))
                {
                    DIAMETERBALL = Convert.ToDouble(entrDIAMETERBALL.Text);
                }
                else
                {
                    DIAMETERBALL = 0.00;
                }
            }
            catch (Exception)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk);
                });
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnDROPBODYFACTORChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                {
                    CalculateButton.IsEnabled = true; // Кнопка Удаления записи неактивна.
                }
                else
                {
                    CalculateButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }

                if (!string.IsNullOrWhiteSpace(entrDROPBODYFACTOR.Text))
                {
                    DROPBODYFACTOR = Convert.ToDouble(entrDROPBODYFACTOR.Text);
                }
                else
                {
                    DROPBODYFACTOR = 0.00;
                }
            }
            catch (Exception)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, AppResource.messageFormatError, AppResource.messageOk);
                });
                return;
            }
        }

        private void OnCalculate(object sender, EventArgs e)
        {
            try
            {
                double timeBODYDROP = 0.00;
                double speedBODYDROP = 0.00;
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing
                    await System.Threading.Tasks.Task.Delay(100);

                    //CalcViewModel = null;
                    //CalcViewModel = new FuelDenCalcViewModel(ENERGY, BaseTEMP, TableSYMBOL);

                    IsBusy = false;
                    resultBODYDROP.Text = string.Empty;

                    if ((entrDISTANCE.Text != 0.ToString()) && (entrDENSITYBODY.Text != 0.ToString())
                    && (entrDENSITYENVIRONMENT.Text != string.Empty) && (entrDIAMETERBALL.Text != 0.ToString()) && (entrDROPBODYFACTOR.Text != 0.ToString()))
                    {

                        //speedBODYDROP = DROPBODYFACTOR * Math.Sqrt((DIAMETERBALL / 10) * ((DENSITYBODY - DENSITYENVIRONMENT) / DENSITYBODY));
                        //speedBODYDROP = DROPBODYFACTOR * Math.Sqrt((DIAMETERBALL / 10) * (DENSITYBODY - DENSITYENVIRONMENT));

                        speedBODYDROP = DROPBODYFACTOR * Math.Sqrt((DIAMETERBALL / 10) * ((DENSITYBODY - DENSITYENVIRONMENT) / DENSITYENVIRONMENT));

                        timeBODYDROP = (DISTANCE * 100) / speedBODYDROP / 60;

                        resultBODYDROP.Text = timeBODYDROP.ToString("N1");
                        labSpeedDrop.Text = speedBODYDROP.ToString("N1");
                    }
                    else
                    {
                        // Что-то пошло не так
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert(AppResource.messageAttention, AppResource.FormulaError, AppResource.messageСancel);
                        });
                        return;
                    }
                });
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

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                Shell.Current.GoToAsync("..", true);
            }
            catch { return false; }
            // Always return true because this method is not asynchronous.
            // We must handle the action ourselves: see above.
            return true;
        }
    }
}