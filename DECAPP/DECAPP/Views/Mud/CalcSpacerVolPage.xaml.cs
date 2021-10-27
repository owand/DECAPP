using DECAPP.Models.Calc;
using DECAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Mud
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalcSpacerVolPage : ContentPage
    {
        private CalcCementViewModel viewModel;

        public double PI; // Число Пи

        public CalcSpacerVolPage()
        {
            InitializeComponent();

            PI = 3.14159265358979323846;

            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                BindingContext = viewModel = viewModel ?? new CalcCementViewModel();

                //type of the casing
                picPipeType.SelectedIndex = viewModel.GetPipeTypeIndex();

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
                        Formula.SetValue(Grid.RowProperty, 0);
                        FormulaContent.ColumnDefinitions[2].Width = 0;
                    }
                    break;

                case TargetIdiom.Phone:
                    break;

                default:
                    break;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                OnResult();
            }
            catch { lbVsp.Text = AppResource.FormulaError; }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnResult();
            }
            catch { lbVsp.Text = AppResource.FormulaError; }
        }

        private void ChangePipeType(object sender, EventArgs e)
        {
            try
            {
                // очищаем диаметры
                viewModel.PipesODList = null;
                picODcas.SelectedIndex = -1;
                picODcas.Behaviors.Clear();

                // очищаем толщину стенки
                picTcas.SelectedIndex = -1;

                //outside diameter of the casing
                viewModel.PipesODList = viewModel.GetPipesODList(picPipeType.SelectedIndex < 0 ? null : viewModel.PipesCollection?[picPipeType.SelectedIndex].TYPEID.ToString());
                picODcas.SelectedIndex = viewModel.PipesODList.IndexOf(viewModel.PipesODList.Where(X => X.PIPESOD == viewModel.CalcCementItem.ODCAS).FirstOrDefault());
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Фильт по диаметру труб.
        private void ChangePipeOD(object sender, EventArgs e)
        {
            if (picODcas.SelectedIndex >= 0)
            {
                // очищаем толщину стенки
                viewModel.PipesTWList = null;
                picTcas.SelectedIndex = -1;
                picTcas.Behaviors.Clear();

                //thickness of the wall of the cemented casing
                viewModel.PipesTWList = viewModel.GetPipesTWList(picPipeType.SelectedIndex < 0 ? null : viewModel.PipesCollection?[picPipeType.SelectedIndex].TYPEID.ToString(),
                                                                 picODcas.SelectedIndex < 0 ? null : viewModel.PipesODList?[picODcas.SelectedIndex].PIPESOD.ToString());
                picTcas.SelectedIndex = viewModel.PipesTWList.IndexOf(viewModel.PipesTWList?.Where(X => X.PIPESWALL == viewModel.CalcCementItem.TCAS).FirstOrDefault());
            }
            try
            {
                OnResult();
            }
            catch { lbVsp.Text = AppResource.FormulaError; }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                viewModel.CalcCementItem.CCOMPRES = decimal.Parse(edCcompres.Text); //coefficient of compression
                viewModel.CalcCementItem.VPIPELINE = decimal.Parse(edVpipeline.Text); //Pipeline Volume

                viewModel.CalcCementItem.PIPESTYPEID = picPipeType.SelectedIndex >= 0 ? viewModel.PipesCollection[picPipeType.SelectedIndex].PIPESTYPEID : -1; //type of the casing
                viewModel.CalcCementItem.ODCAS = picODcas.SelectedIndex >= 0 ? viewModel.PipesODList[picODcas.SelectedIndex].PIPESOD : -1; //outside diameter of the casing
                viewModel.CalcCementItem.TCAS = picTcas.SelectedIndex >= 0 ? viewModel.PipesTWList[picTcas.SelectedIndex].PIPESWALL : -1; //thickness of the wall of the cemented casing

                viewModel.CalcCementItem.LCAS = decimal.Parse(edLcas.Text); //casing length
                viewModel.CalcCementItem.LHCP = decimal.Parse(edLhcp.Text); //HCP

                // Сохраняем изменения в текущей записи.
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
                picPipeType.SelectedIndex = -1;
                picODcas.SelectedIndex = -1;
                picTcas.SelectedIndex = -1;
                edLcas.Text = 0.ToString("N2");
                edLhcp.Text = 0.ToString("N2");

                edCcompres.Text = 0.ToString("N2");
                edVpipeline.Text = 0.ToString("N2");
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnResult()
        {
            try
            {
                //calcCementItem = this.viewModel?.CalcCement.First(); // Производим отбор текущей записи (переменная для загрузки картинки)

                //coefficient of compression
                double Ccompres = !string.IsNullOrWhiteSpace(edCcompres.Text) ? Convert.ToDouble(edCcompres.Text) : 1;

                //Pipeline Volume
                double Vpipeline = !string.IsNullOrWhiteSpace(edVpipeline.Text) ? Convert.ToDouble(edVpipeline.Text) : 0;

                //casing type
                double ODcas = picODcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesODList[picODcas.SelectedIndex].PIPESOD) : 0;

                //thickness of the wall of the cemented casing
                double Tcas = picTcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesTWList[picTcas.SelectedIndex].PIPESWALL) : 0;

                //casing length
                double Lcas = !string.IsNullOrWhiteSpace(edLcas.Text) ? Convert.ToDouble(edLcas.Text) : 0;

                //HCP
                double Lhcp = !string.IsNullOrWhiteSpace(edLhcp.Text) ? Convert.ToDouble(edLhcp.Text) : 0;

                double result = 0;

                result = PI / 4 * Ccompres * (Math.Pow((ODcas - Tcas * 2) / 1000, 2) * (Lcas - Lhcp)) + Vpipeline;

                lbVsp.Text = result.ToString("N2");
            }
            catch (Exception)
            {
                // Что-то пошло не так
                lbVsp.Text = "Error";
                return;
            }
        }
    }
}