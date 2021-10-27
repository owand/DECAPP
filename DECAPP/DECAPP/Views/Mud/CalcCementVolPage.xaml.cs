using DECAPP.Models.Calc;
using DECAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Mud
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalcCementVolPage : ContentPage
    {
        private CalcCementViewModel viewModel;

        public double PI; // Число Пи

        public CalcCementVolPage()
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

                picDwell.SelectedIndex = viewModel.GetDwellIndex();

                //type of the casing
                picPipeType.SelectedIndex = viewModel.GetPipeTypeIndex();

                //type of the previous casing
                picPrevPipeType.SelectedIndex = viewModel.GetPrevPipeTypeIndex();

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
            catch { lbVcs.Text = AppResource.FormulaError; }
        }

        // Событие при изменении текста в соответствующих полях.
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OnResult();
            }
            catch { lbVcs.Text = AppResource.FormulaError; }
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
                picTcas.Behaviors.Clear();

                //outside diameter of the casing
                viewModel.PipesODList = viewModel.GetPipesODList(picPipeType.SelectedIndex < 0 ? null : viewModel.PipesCollection?[picPipeType.SelectedIndex].TYPEID.ToString());
                picODcas.SelectedIndex = viewModel.PipesODList.IndexOf(viewModel.PipesODList.FirstOrDefault(X => X.PIPESOD == viewModel.CalcCementItem.ODCAS));
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void ChangePrevPipeType(object sender, EventArgs e)
        {
            try
            {
                // очищаем диаметры
                viewModel.PipesPrevODList = null;
                picODprcas.SelectedIndex = -1;
                picODprcas.Behaviors.Clear();

                // очищаем толщину стенки
                picTprcas.SelectedIndex = -1;
                picTprcas.Behaviors.Clear();

                //outer diameter of the previous casing
                viewModel.PipesPrevODList = viewModel.GetPipesODList(picPrevPipeType.SelectedIndex < 0 ? null : viewModel.PipesCollection?[picPrevPipeType.SelectedIndex].TYPEID.ToString());
                picODprcas.SelectedIndex = viewModel.PipesPrevODList.IndexOf(viewModel.PipesPrevODList.FirstOrDefault(X => X.PIPESOD == viewModel.CalcCementItem.ODPRCAS));
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
                picTcas.SelectedIndex = viewModel.PipesTWList.IndexOf(viewModel.PipesTWList?.FirstOrDefault(X => X.PIPESWALL == viewModel.CalcCementItem.TCAS));
            }
            try
            {
                OnResult();
            }
            catch { lbVcs.Text = AppResource.FormulaError; }
        }

        // Фильт по диаметру труб.
        private void ChangePipePrevOD(object sender, EventArgs e)
        {
            if (picODprcas.SelectedIndex >= 0)
            {
                // очищаем толщину стенки
                viewModel.PipesPrevTWList = null;
                picTprcas.SelectedIndex = -1;
                picTprcas.Behaviors.Clear();

                //thickness of the wall of the cemented casing
                viewModel.PipesPrevTWList = viewModel.GetPipesTWList(picPrevPipeType.SelectedIndex < 0 ? null : viewModel.PipesCollection?[picPrevPipeType.SelectedIndex].TYPEID.ToString(),
                                                                     picODprcas.SelectedIndex < 0 ? null : viewModel.PipesPrevODList?[picODprcas.SelectedIndex].PIPESOD.ToString());
                picTprcas.SelectedIndex = viewModel.PipesPrevTWList.IndexOf(viewModel.PipesPrevTWList?.FirstOrDefault(X => X.PIPESWALL == viewModel.CalcCementItem.TPRCAS));
            }
            try
            {
                OnResult();
            }
            catch { lbVcs.Text = AppResource.FormulaError; }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                viewModel.CalcCementItem.CCS = decimal.Parse(edCcs.Text); //coefficient of cement slurry
                viewModel.CalcCementItem.CCAV = decimal.Parse(edCcav.Text); //coefficient of cavernosity
                viewModel.CalcCementItem.DWELL = picDwell.SelectedIndex >= 0 ? viewModel.BitCollection[picDwell.SelectedIndex].BITOD : -1; //borehole diameter

                viewModel.CalcCementItem.PIPESTYPEID = picPipeType.SelectedIndex >= 0 ? viewModel.PipesCollection[picPipeType.SelectedIndex].PIPESTYPEID : -1; //type of the casing
                viewModel.CalcCementItem.ODCAS = picODcas.SelectedIndex >= 0 ? viewModel.PipesODList[picODcas.SelectedIndex].PIPESOD : -1; //outside diameter of the casing
                viewModel.CalcCementItem.TCAS = picTcas.SelectedIndex >= 0 ? viewModel.PipesTWList[picTcas.SelectedIndex].PIPESWALL : -1; //thickness of the wall of the cemented casing

                viewModel.CalcCementItem.LCAS = decimal.Parse(edLcas.Text); //casing length
                viewModel.CalcCementItem.LHCP = decimal.Parse(edLhcp.Text); //HCP

                viewModel.CalcCementItem.PIPEPREVTYPEID = picPrevPipeType.SelectedIndex >= 0 ? viewModel.PipesCollection[picPrevPipeType.SelectedIndex].PIPESTYPEID : -1; //type of the previous casing
                viewModel.CalcCementItem.ODPRCAS = picODprcas.SelectedIndex >= 0 ? viewModel.PipesPrevODList[picODprcas.SelectedIndex].PIPESOD : -1; //outer diameter of the previous casing
                viewModel.CalcCementItem.TPRCAS = picTprcas.SelectedIndex >= 0 ? viewModel.PipesPrevTWList[picTprcas.SelectedIndex].PIPESWALL : -1; //thickness of the wall of the previous casing

                viewModel.CalcCementItem.LPRCAS = decimal.Parse(edLprcas.Text); //depth of descent of the previous column along the trunk
                viewModel.CalcCementItem.LPRHCP = decimal.Parse(edLprhcp.Text); //PrevHCP

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
                edCcs.Text = 0.ToString("N2");
                edCcav.Text = 0.ToString("N2");
                picDwell.SelectedIndex = -1;

                picPipeType.SelectedIndex = -1;
                picODcas.SelectedIndex = -1;
                picTcas.SelectedIndex = -1;
                edLcas.Text = 0.ToString("N2");
                edLhcp.Text = 0.ToString("N2");

                picPrevPipeType.SelectedIndex = -1;
                picODprcas.SelectedIndex = -1;
                picTprcas.SelectedIndex = -1;
                edLprcas.Text = 0.ToString("N2");
                edLprhcp.Text = 0.ToString("N2");
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

                //coefficient of cement slurry
                double Ccs = !string.IsNullOrWhiteSpace(edCcs.Text) ? Convert.ToDouble(edCcs.Text) : 1.05;

                //coefficient of cavernosity
                double Ccav = !string.IsNullOrWhiteSpace(edCcav.Text) ? Convert.ToDouble(edCcav.Text) : 1.2;

                //borehole diameter
                double Dwell = picDwell.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.BitCollection[picDwell.SelectedIndex].BITODNAME) : 0;

                //casing type
                double ODcas = picODcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesODList[picODcas.SelectedIndex].PIPESOD) : 0;

                //thickness of the wall of the cemented casing
                double Tcas = picTcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesTWList[picTcas.SelectedIndex].PIPESWALL) : 0;

                //casing length
                double Lcas = !string.IsNullOrWhiteSpace(edLcas.Text) ? Convert.ToDouble(edLcas.Text) : 0;

                //HCP
                double Lhcp = !string.IsNullOrWhiteSpace(edLhcp.Text) ? Convert.ToDouble(edLhcp.Text) : 0;

                //outer diameter of the previous casing
                double ODprcas = picODprcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesPrevODList[picODprcas.SelectedIndex].PIPESOD) : 0;

                //thickness of the wall of the previous casing
                double Tprcas = picTprcas.SelectedIndex >= 0 ? Convert.ToDouble(viewModel.PipesPrevTWList[picTprcas.SelectedIndex].PIPESWALL) : 0;

                //depth of descent of the previous column along the trunk
                double Lprcas = !string.IsNullOrWhiteSpace(edLprcas.Text) ? Convert.ToDouble(edLprcas.Text) : 0;

                //PrevHCP
                double Lprhcp = !string.IsNullOrWhiteSpace(edLprhcp.Text) ? Convert.ToDouble(edLprhcp.Text) : 0;


                double result = 0;
                //result = Math.Pow(Dwell / 1000, 2);

                result = PI / 4 * Ccs * (
                        (Ccav * Math.Pow(Dwell / 1000, 2) - Math.Pow(ODcas / 1000, 2)) * (Lcas - Lprcas) +
                        (Math.Pow((ODprcas / 1000 - Math.Pow(Tprcas / 1000, 2)), 2) - Math.Pow(ODcas / 1000, 2)) * (Lprcas - Lprhcp) +
                        (Math.Pow(Dwell / 1000, 2) - Math.Pow(ODcas / 1000, 2)) * Lprhcp +
                        Math.Pow((ODcas - Tcas * 2) / 1000, 2) * Lhcp);

                lbVcs.Text = result.ToString("N2");
            }
            catch (Exception)
            {
                // Что-то пошло не так
                lbVcs.Text = "Error";
                return;
            }
        }
    }
}