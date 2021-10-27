using DECAPP.Resources;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Other
{
    public class LexisList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<LexisModel> collection;
        public ObservableCollection<LexisModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<LexisModel> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<LexisModel> _collection = new List<LexisModel>(App.Database.Table<LexisModel>().Select(a => a).Where(a =>
            a.RUSLEXIS.ToLowerInvariant().Contains(searchCriterion) ||
            a.ENGLEXIS.ToLowerInvariant().Contains(searchCriterion)).OrderBy(a => a.RUSLEXIS).ToList());

            return new ObservableCollection<LexisModel>(_collection);
        }

        private LexisModel selectItem = null;
        public LexisModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private LexisModel newItem = null;
        public LexisModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public LexisModel PreSelectItem { get; set; }

        public bool detailMode;
        public bool DetailMode
        {
            get => detailMode;
            set
            {
                detailMode = value;
                OnPropertyChanged(nameof(DetailMode));
            }
        }

        public LexisList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<LexisModel>().Any())
            {
                Collection?.Add(new LexisModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new LexisModel();
                Collection.Add(NewItem);
                SelectItem = NewItem;
                NewItem = null;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                if (NewItem != null)
                {
                    Collection.Remove(NewItem);
                    NewItem = null;
                }
                return;
            }
        }

        // Сохраняем или создаем и сохраняем новую запись.
        public void UpdateItem()
        {
            try
            {
                lock (collisionLock)
                {
                    if (NewItem != null)
                    {
                        App.Database.Insert(SelectItem);
                    }
                    else
                    {
                        App.Database.Update(SelectItem);
                    }
                }

                NewItem = null;

                DetailMode = true;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись.
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Delete<LexisModel>(SelectItem.LEXISID);
                    Collection.Remove(SelectItem);
                }
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }
    }


    [Table("tbLexis")]
    public class LexisModel : ViewModelBase
    {
        [Column("LexisID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int LEXISID { get; set; }

        [Column("RusLexis"), NotNull]
        public string RUSLEXIS
        {
            get => ruslexis;
            set
            {
                ruslexis = value;
                OnPropertyChanged(nameof(RUSLEXIS));
            }
        }

        [Column("EngLexis"), NotNull]
        public string ENGLEXIS
        {
            get => englexis;
            set
            {
                englexis = value;
                OnPropertyChanged(nameof(ENGLEXIS));
            }
        }

        public string ruslexis;
        public string englexis;

        public LexisModel()
        {
        }
    }
}
