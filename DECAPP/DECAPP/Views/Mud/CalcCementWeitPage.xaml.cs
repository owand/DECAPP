using DECAPP.Models.Calc;
using DECAPP.Models.Mud;
using DECAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Mud
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalcCementWeitPage : ContentPage
    {
        private CalcCementViewModel viewModel;
        private MudList mudViewModel;

        public CalcCementWeitPage()
        {
            InitializeComponent();
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true;   // Затеняем задний фон и запускаем ProgressRing

                BindingContext = viewModel = viewModel ?? new CalcCementViewModel();

                picMudName.BindingContext = mudViewModel = mudViewModel ?? new MudList();

                mudViewModel.Collection = mudViewModel?.GetCollection(null, null);

                picMudName.SelectedIndex = mudViewModel.Collection.IndexOf(mudViewModel?.Collection.Where(X => X.ID == viewModel.CalcCementItem.MUDID).FirstOrDefault());

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                OnResult();
            }
            catch { }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                labDensityDryCement.Text = mudViewModel?.Collection[picMudName.SelectedIndex].DENSITY.ToString("N2");
                OnResult();
            }
            catch { }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnResult()
        {
            double Dfluid = 1;   // Плотность жидкости затворения, г/см3 / Density of mixing fluid, g / cm3
            double Dslurry = 1;   // Плотность тампонажного раствора, г/см3 / The density of cement slurry, g / cm3
            double Dсement = 1;   // Плотность сухого материала, г/см3 / Density of dry material, g / cm3

            try
            {
                if (!string.IsNullOrWhiteSpace(edDensityFluid.Text))
                {
                    Dfluid = Convert.ToDouble(edDensityFluid.Text);
                }

                if (!string.IsNullOrWhiteSpace(edDensitySlurry.Text))
                {
                    Dslurry = Convert.ToDouble(edDensitySlurry.Text);
                }

                if (!string.IsNullOrWhiteSpace(labDensityDryCement.Text))
                {
                    Dсement = Convert.ToDouble(labDensityDryCement.Text);
                }

                double Cws = Dfluid * (Dсement - Dslurry) / (Dсement * (Dslurry - Dfluid));
                double Gcem = Dslurry / (1 + Cws);
                double Gflu = Cws * Gcem;

                labCWaterSolid.Text = Cws.ToString("N2");
                labGсement.Text = Gcem.ToString("N3");
                labGfluid.Text = Gflu.ToString("N3");
                if (!string.IsNullOrEmpty(edVcs.Text))
                {
                    double Vcs = double.Parse(edVcs.Text);   // Объём цементного раствора, в м3
                    double CLCem = double.Parse(edCLossCement.Text);   // коэффициент потерь сухого цемента (обычно 1,05)
                    double TGcem = Vcs * Gcem * CLCem;
                    labTGcem.Text = TGcem.ToString("N2");

                    double CLWater = double.Parse(edCLossWater.Text);   // Коэффициент потери воды / Water loss coefficient
                    double TGflu = Vcs * Gflu * CLWater / Dfluid;
                    labTGflu.Text = TGflu.ToString("N2");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                viewModel.CalcCementItem.DENSITYSLURRY = decimal.Parse(edDensitySlurry.Text); // Плотность тампонажного раствора, г/см3 / The density of cement slurry, g / cm3
                viewModel.CalcCementItem.DENSITYFLUID = decimal.Parse(edDensityFluid.Text); // Плотность жидкости затворения, г/см3 / Density of mixing fluid, g / cm3

                viewModel.CalcCementItem.MUDID = mudViewModel.Collection[picMudName.SelectedIndex].ID; // Уникальный код реагентов и тампонажных смесей

                viewModel.CalcCementItem.CLOSSCEMENT = decimal.Parse(edCLossCement.Text); // Коэффициент потерь сухого цемента / Dry cement loss ratio
                viewModel.CalcCementItem.CLOSSWATER = decimal.Parse(edCLossWater.Text); // Коэффициент потери воды / Water loss coefficient

                //Сохраняем изменения в текущей записи.
                viewModel?.UpdateItem();
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
                edDensitySlurry.Text = 0m.ToString("N2");   // Плотность тампонажного раствора, г/см3 / The density of cement slurry, g / cm3
                edDensityFluid.Text = 0m.ToString("N2");   // Плотность жидкости затворения, г/см3 / Density of mixing fluid, g / cm3

                picMudName.SelectedIndex = -1;   // Уникальный код реагентов и тампонажных смесей
                labDensityDryCement.Text = string.Empty;

                edCLossCement.Text = 0m.ToString("N2");   // Коэффициент потерь сухого цемента / Dry cement loss ratio
                edCLossWater.Text = 0m.ToString("N2");   // Коэффициент потери воды / Water loss coefficient

            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

    }
}