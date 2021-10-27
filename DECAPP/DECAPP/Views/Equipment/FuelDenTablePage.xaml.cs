using DECAPP.Models.Equipment;
using DECAPP.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Equipment
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FuelDenTablePage : ContentPage
    {
        private FuelDen viewModel;

        public FuelDenTablePage()
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
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                BindingContext = viewModel = viewModel ?? new FuelDen();

                await RefreshListView();

                viewModel.SelectItem = viewModel.Collection.Count == 0 ? null : viewModel.Collection.FirstOrDefault();
                MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.

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
            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
        }

        // Событие, которое вызывается при выборе отличного от текущего или нового элемента.
        private void OnSelection(object sender, SelectedItemChangedEventArgs e) // Загружаем дочернюю форму и передаем ей параметр ID.
        {
            try
            {
                if (e.SelectedItem != null) // Если в Collection есть записи.
                {
                    EditButton.IsEnabled = true; // Кнопка Редактирования активна.
                    DeleteButton.IsEnabled = true; // Кнопка Удаления записи активна.
                    MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }
                else
                {
                    EditButton.IsEnabled = false; // Кнопка Редактирования неактивна.
                    DeleteButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private void OnTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Height <= 800)
                        {
                            viewModel.DetailMode = true;
                            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                        }
                        break;

                    case TargetIdiom.Phone:
                        viewModel.DetailMode = true;
                        OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
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

        #region --------- Filter - Command --------

        private async void OpenFilter(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                if (SaveCommandBar.IsVisible)
                {
                    await DisplayAlert(AppResource.messageError, AppResource.FilterError, AppResource.messageOk); // Что-то пошло не так
                    return;
                }

                await Task.Delay(100);

                if (!FilterBar.IsVisible)
                {
                    viewModel.BaseTempList = viewModel.GetBaseTempList();
                    viewModel.TableList = viewModel.GetTableList();
                    viewModel.TempList = viewModel.GetTempList();
                    viewModel.BaseDensityList = viewModel.GetBaseDensityList();
                    FilterBar.IsVisible = true;
                }
                else
                {
                    picFILTERENERGY.SelectedIndex = -1;
                    picFILTERBASETEMP.SelectedIndex = -1;
                    picFILTERTABLE.SelectedIndex = -1;
                    picFILTERTEMP.SelectedIndex = -1;
                    picFILTERBASEDENSITY.SelectedIndex = -1;

                    FilterBar.IsVisible = false;
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);  // Что-то пошло не так
                return;
            }
        }

        // Фильтр записей отображаемых в ListView.
        private async void OnFilter(object sender, EventArgs e)
        {
            await RefreshListView(); // Обновление записей в ListView Collection
        }

        // Очищаем фильтр по виду топлива
        private void OnCancelFilterEnergy(object sender, EventArgs e)
        {
            picFILTERENERGY.SelectedIndex = -1;
        }

        // Очищаем фильтр по базовой температуре
        private void OnCancelFilterBaseTemp(object sender, EventArgs e)
        {
            picFILTERBASETEMP.SelectedIndex = -1;
        }

        // Очищаем фильтр по таблице
        private void OnCancelFilterTable(object sender, EventArgs e)
        {
            picFILTERTABLE.SelectedIndex = -1;
        }

        // Очищаем фильтр по температуре
        private void OnCancelFilterTemp(object sender, EventArgs e)
        {
            picFILTERTEMP.SelectedIndex = -1;
        }

        // Очищаем фильтр по температуре
        private void OnCancelFilterBaseDensity(object sender, EventArgs e)
        {
            picFILTERBASEDENSITY.SelectedIndex = -1;
        }

        #endregion

        #region --------- Header - Command --------

        // Переходим в режим редактирования.
        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                viewModel.NewItem = null;

                if (viewModel.SelectItem != null)
                //if (MasterContent.SelectedItem != null)
                {
                    GoToEditState(); // Переходим в режим редактирования.
                }
                else
                {
                    EditButton.IsEnabled = false; // Если нет активной записи в MasterListView, кнопка Редактирования неактивна.
                    DisplayAlert(AppResource.messageAttention, AppResource.messageNoActiveRecord, AppResource.messageСancel); // Что-то пошло не так
                    return;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Создаем новую запись.
        private void OnAdd(object sender, EventArgs e)
        {
            try
            {
                GoToEditState(); // Переходим в режим редактирования.
                viewModel.AddItem(); // Создаем новую запись в объединенной коллекции
                MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись
        private void OnDelete(object sender, EventArgs e)
        {
            try
            {
                int indexItem = viewModel.Collection.IndexOf(viewModel.SelectItem);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog == true)
                    {
                        viewModel.DeleteItem(); // Удаляем текущую запись.

                        if (viewModel.Collection.Count != 0) // Если в Collection есть записи.
                        {
                            if (indexItem == 0) // Если текущая запись первая.
                            {
                                viewModel.SelectItem = viewModel.Collection[indexItem]; // Переходим на следующую запись после удаленной, у которой такой же индекс как и у удаленной.
                            }
                            else
                            {
                                viewModel.SelectItem = viewModel.Collection[indexItem - 1]; // Переходим на предыдующую запись перед удаленной.
                            }
                        }
                        MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                    }
                    else
                    {
                        return;
                    }
                });
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
                viewModel.SelectItem.ENERGYID = viewModel.EnergyList[picENERGYNAME.SelectedIndex].ENERGYID;
                //viewModel.SelectItem.BASETEMP = double.Parse(editBASETEMP.Text); // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.TABLE = editTABLE.Text; // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.TEMP = double.Parse(editTEMP.Text); // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.BASEDENSITY = double.Parse(editBASEDENSITY.Text); // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.DENSITY = double.Parse(editDENSITY.Text); // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.DESCRIPTION = editDESCRIPTION.Text; // Читаем данные из соответствующего поля.
                //viewModel.SelectItem.NOTE = editNOTE.Text; // Читаем данные из соответствующего поля.

                // Сохраняем изменения в текущей записи.
                viewModel?.UpdateItem();

                SaveCommandBar.IsVisible = false;
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Отмена изменений.
        private void OnCancel(object sender, EventArgs e)
        {
            Cancel(); // Отмена изменений в записи.
        }

        #endregion

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                if (viewModel?.DetailMode == false)
                {
                    Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                    viewModel = null;
                    Shell.Current.GoToAsync("..", true);
                }
                else if ((viewModel?.DetailMode == true) && (SaveCommandBar.IsVisible == true))
                {
                    Cancel(); // Отмена изменений в записи.
                }
                else if (viewModel?.DetailMode == true)
                {
                    viewModel.DetailMode = false;
                    OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                }
            }
            catch { return false; }
            // Always return true because this method is not asynchronous. We must handle the action ourselves: see above.
            return true;
        }

        // Отмена изменений в записи.
        private void Cancel()
        {
            try
            {
                SaveCommandBar.IsVisible = false;

                // После обновления записей отображаемых в ListView.
                if (viewModel.Collection.Count != 0)
                {
                    if (viewModel.NewItem != null)
                    {
                        viewModel.SelectItem = viewModel.PreSelectItem;
                        viewModel.Collection.Remove(viewModel.NewItem);
                    }
                    else
                    {
                        viewModel.SelectItem = viewModel.SelectItem == null ? viewModel.Collection.FirstOrDefault() : viewModel.PreSelectItem;
                    }
                    MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }

                viewModel.NewItem = null;

                //viewModel.DetailMode = true;
                viewModel.DetailMode = Device.Idiom != TargetIdiom.Desktop && Device.Idiom != TargetIdiom.Tablet || Shell.Current.Height <= 800;

                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Переключение между режимами редактирования и чтения.
        private void GoToEditState()
        {
            try
            {
                viewModel.PreSelectItem = viewModel.SelectItem;
                viewModel.DetailMode = true;
                SaveCommandBar.IsVisible = true;
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Изменение интерфейса при изменении размера окна.
        private void OnSizeChangeInterface()
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Height <= 800)
                        {
                            if (viewModel?.DetailMode == false)
                            {
                                // Компактный вид, режим чтения, детали скрыты.
                                Body.RowDefinitions[0].Height = new GridLength(0);
                                Body.RowDefinitions[1].Height = GridLength.Star;
                                Master.IsVisible = true;
                            }
                            else
                            {
                                // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                                Body.RowDefinitions[0].Height = GridLength.Star;
                                Body.RowDefinitions[1].Height = new GridLength(0);
                                Master.IsVisible = false;
                            }
                        }
                        else
                        {
                            // Расширенный (полный) вид, режим чтения.
                            Body.RowDefinitions[0].Height = GridLength.Auto;
                            Body.RowDefinitions[1].Height = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        break;

                    case TargetIdiom.Phone:
                        if (viewModel?.DetailMode == false)
                        {
                            // Компактный вид, режим чтения, детали скрыты.
                            Body.RowDefinitions[0].Height = new GridLength(0);
                            Body.RowDefinitions[1].Height = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        else
                        {
                            // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                            Body.RowDefinitions[0].Height = GridLength.Star;
                            Body.RowDefinitions[1].Height = new GridLength(0);
                            Master.IsVisible = false;
                        }
                        break;

                    default:
                        break;
                }

                // Происходит при изменении ориентации.
                if (Shell.Current.Width > Shell.Current.Height)
                {
                    FilterBar.Orientation = StackOrientation.Horizontal;
                }
                else
                {
                    FilterBar.Orientation = StackOrientation.Vertical;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Обновление записей отображаемых в ListView.
        private async Task RefreshListView()
        {
            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                await Task.Delay(100);

                // Обновление записей в ListView Collection
                MasterContent.BeginRefresh();
                MasterContent.IsRefreshing = true;

                viewModel.Collection = viewModel.GetCollection(picFILTERENERGY.SelectedIndex < 0 ? null : viewModel.EnergyList?[picFILTERENERGY.SelectedIndex].ENERGYID.ToString(),
                                                               picFILTERBASETEMP.SelectedItem?.ToString(),
                                                               picFILTERTABLE.SelectedItem?.ToString(),
                                                               picFILTERTEMP.SelectedItem?.ToString(),
                                                               picFILTERBASEDENSITY.SelectedItem?.ToString());

                MasterContent.ItemsSource = viewModel.Collection;

                MasterContent.IsRefreshing = false;
                MasterContent.EndRefresh();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);  // Что-то пошло не так
                return;
            }
        }
    }
}