using DECAPP.Models.BHA;
using DECAPP.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.BHA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BitODPage : ContentPage
    {
        private BitODViewModel viewModel;

        public BitODPage()
        {
            InitializeComponent();
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                BindingContext = viewModel = viewModel ?? new BitODViewModel();

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

        // События непосредственно перед тем как страница становится видимой.
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

        // Фильтр записей отображаемых в ListView.
        private async void OnFilter(object sender, TextChangedEventArgs e)
        {
            await RefreshListView(); // Обновление записей в ListView Collection
        }

        #region --------- Header - Command --------

        // Переходим в режим редактирования.
        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                viewModel.NewItem = null;

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

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                viewModel.UpdateItem(); // Сохраняем или создаем новую запись в подчиненной коллекции

                SaveCommandBar.IsVisible = false;
                SearchBar.Focus();
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

                SearchBar.Focus();
                viewModel.DetailMode = Device.Idiom != TargetIdiom.Desktop && Device.Idiom != TargetIdiom.Tablet || Shell.Current.Width <= 800;
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