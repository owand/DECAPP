using DECAPP.Models.Tools;
using DECAPP.Resources;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Tools
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ToolsTypePage : ContentPage
    {
        private ToolsType viewModel;

        public ToolsTypePage()
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

                BindingContext = viewModel = viewModel ?? new ToolsType();

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
                        if (Shell.Current.Width <= 800)
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
                DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Фильтр записей отображаемых в ListView.
        private async void OnFilter(object sender, TextChangedEventArgs e)
        {
            await RefreshListView();
        }

        private async void DeleteImage(object sender, EventArgs e)
        {
            try
            {
                bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
                if (dialog == true)
                {
                    viewModel.SelectItem.PICTURE = null;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        private async void UploadImage(object sender, EventArgs args)
        {
            try
            {
                FileResult file = await FilePicker.PickAsync();
                if (file != null)
                {
                    if (file.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || file.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        Stream stream = await file.OpenReadAsync();
                        stream.CopyTo(memoryStream);
                        stream.Dispose();
                        viewModel.SelectItem.PICTURE = memoryStream.ToArray();
                    }
                }

                GoToEditState(); // Переходим в режим чтения.
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                return;
            }
        }

        #region --------- Header - Command --------

        // Переходим в режим редактирования.
        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                viewModel.NewJoinItem = null;

                if (viewModel.SelectItem != null)
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

        // Удаляем текущую запись.async
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

        // Сохраняем изменения в основной коллекции.
        private async Task SaveHostItem()
        {
            try
            {
                if (viewModel.GetSelectHostItem() != null)
                {
                    // Текущая основная коллекция
                    viewModel.SelectHostItem = viewModel.GetSelectHostItem();
                    viewModel.SelectHostItem.TYPENAME = viewModel.SelectItem.TYPENAME;
                    viewModel.SelectHostItem.PICTURE = viewModel.SelectItem.PICTURE ?? null; // Читаем данные из соответствующего поля.
                }
                else
                {
                    // Новая основная коллекция
                    viewModel.NewHostItem = new ToolsTypeModel() // Данные для новой основной коллекции
                    {
                        TYPENAME = viewModel.SelectItem.TYPENAME,
                        PICTURE = viewModel.SelectItem.PICTURE
                    };
                }

                viewModel.UpdateItem(); // Сохраняем запись в основной и подчиненной коллекции
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем изменения в подчиненной коллекции.
        private async Task SaveSubItem()
        {
            try
            {
                // Данные для текущей подчиненной коллекции
                if (viewModel.GetSelectSubItem() != null)
                {
                    viewModel.SelectSubItem = viewModel.GetSelectSubItem();
                    viewModel.SelectSubItem.DESCRIPTION = viewModel.SelectItem.DESCRIPTION;
                    viewModel.SelectSubItem.NOTE = viewModel.SelectItem.NOTE;
                }
                else
                {
                    // Данные для новой подчиненной коллекции
                    viewModel.NewSubItem = new ToolsTypeSubModel()
                    {
                        TYPEID = viewModel.NewHostItem != null ? viewModel.NewHostItem.TYPEID : viewModel.SelectHostItem.TYPEID,
                        DESCRIPTION = viewModel.SelectItem.DESCRIPTION,
                        NOTE = viewModel.SelectItem.NOTE,
                        LANGUAGE = App.AppLanguage
                    };
                }

                viewModel.UpdateSubItem(); // Сохраняем запись в основной и подчиненной коллекции
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем изменения.
        private async void OnSave(object sender, EventArgs e)
        {
            try
            {
                await SaveHostItem(); // Сохраняем изменения в основной коллекции.

                await SaveSubItem(); // Сохраняем изменения в подчиненной коллекции.

                await RefreshListView();

                //После обновления записей отображаемых в ListView.
                if (viewModel.Collection.Count != 0)
                {
                    viewModel.SelectItem = viewModel.NewJoinItem != null ? viewModel.Collection.FirstOrDefault(a => a.ID == viewModel.NewHostItem.TYPEID) :
                                                                           viewModel.Collection.FirstOrDefault(a => a.ID == viewModel.SelectHostItem.TYPEID);
                    MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }

                viewModel.SelectHostItem = null;
                viewModel.NewHostItem = null;
                viewModel.SelectSubItem = null;
                viewModel.NewSubItem = null;
                viewModel.NewJoinItem = null;

                viewModel.DetailMode = true;

                SaveCommandBar.IsVisible = false;
                SearchBar.Focus();
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Отмена изменений.
        private void OnCancel(object sender, EventArgs e)
        {
            Cancel(); // Отмена изменений в записи.
        }

        #endregion --------- Header - Command --------

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                if (viewModel.DetailMode == false)
                {
                    Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                    viewModel = null;
                    Shell.Current.GoToAsync("..", true);
                }
                else if ((viewModel.DetailMode == true) && (SaveCommandBar.IsVisible == true))
                {
                    Cancel(); // Отмена изменений в записи.
                }
                else if (viewModel.DetailMode == true)
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
                    if (viewModel.NewJoinItem != null)
                    {
                        viewModel.SelectItem = viewModel.PreSelectItem;
                        viewModel.Collection.Remove(viewModel.NewJoinItem);
                    }
                    else
                    {
                        viewModel.SelectItem = viewModel.SelectItem == null ? viewModel.Collection.FirstOrDefault() : viewModel.PreSelectItem;
                    }
                    MasterContent.ScrollTo(viewModel.SelectItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                }

                viewModel.NewJoinItem = null;
                viewModel.SelectHostItem = null;
                viewModel.SelectSubItem = null;

                SearchBar.Focus();
                viewModel.DetailMode = Device.Idiom != TargetIdiom.Desktop && Device.Idiom != TargetIdiom.Tablet || Shell.Current.Width <= 800;

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
                        if (Shell.Current.Width <= 800)
                        {
                            if (viewModel?.DetailMode == false)
                            {
                                // Компактный вид, режим чтения, детали скрыты.
                                Body.ColumnDefinitions[0].Width = GridLength.Star;
                                Body.ColumnDefinitions[1].Width = new GridLength(0);
                                Master.IsVisible = true;
                            }
                            else
                            {
                                // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                                Body.ColumnDefinitions[0].Width = new GridLength(0);
                                Body.ColumnDefinitions[1].Width = GridLength.Star;
                                Master.IsVisible = false;
                            }
                        }
                        else
                        {
                            // Расширенный (полный) вид, режим чтения.
                            Body.ColumnDefinitions[0].Width = new GridLength(320);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        break;

                    case TargetIdiom.Phone:
                        if (viewModel?.DetailMode == false)
                        {
                            // Компактный вид, режим чтения, детали скрыты.
                            Body.ColumnDefinitions[0].Width = GridLength.Star;
                            Body.ColumnDefinitions[1].Width = new GridLength(0);
                            Master.IsVisible = true;
                        }
                        else
                        {
                            // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                            Body.ColumnDefinitions[0].Width = new GridLength(0);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = false;
                        }
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

        // Обновление записей отображаемых в ListView.
        private async Task RefreshListView()
        {
            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                await Task.Delay(100);

                //Обновление записей в ListView Collection
                MasterContent.BeginRefresh();
                MasterContent.IsRefreshing = true;

                viewModel.Collection = viewModel.GetCollection(SearchBar.Text);

                MasterContent.ItemsSource = viewModel.Collection;

                MasterContent.IsRefreshing = false;
                MasterContent.EndRefresh();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }
    }
}