using DECAPP.Resources;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Rock
{
    public class RockRang : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<RockRangModel> collection;
        public ObservableCollection<RockRangModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<RockRangModel> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<RockRangModel> _collection = App.Database.Table<RockRangModel>().Select(a => a).Where(a =>
            a.LANGUAGE.Equals(App.AppLanguage) &&
            (a.ROCKRANG.ToLowerInvariant().Contains(searchCriterion) ||
            (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)))).OrderBy(a => a.ROCKRANG).ToList();

            return new ObservableCollection<RockRangModel>(_collection);
        }

        private RockRangModel selectItem = null;
        public RockRangModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private RockRangModel newItem = null;
        public RockRangModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public RockRangModel PreSelectItem { get; set; }


        public bool detailMode = false;
        public bool DetailMode
        {
            get => detailMode;
            set
            {
                detailMode = value;
                OnPropertyChanged(nameof(DetailMode));
            }
        }

        public RockRang()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<RockRangModel>().Any())
            {
                Collection?.Add(new RockRangModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new RockRangModel();
                Collection.Add(NewItem);
                SelectItem = NewItem;
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
                    App.Database.Delete<RockRangModel>(SelectItem.ROCKRANGID);
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



    //Таблица типов породы
    [Table("tbRockRang")]
    public class RockRangModel : ViewModelBase
    {
        [Column("RockRangID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ROCKRANGID { get; set; }   // Уникальный код группы

        [Column("RockRang"), NotNull, Indexed]
        public string ROCKRANG   // Название группы
        {
            get => rockrang.ToUpper();
            set
            {
                rockrang = value.ToUpper();
                OnPropertyChanged(nameof(ROCKRANG));
            }
        }

        [Column("Symbol"), NotNull, Indexed]
        public string SYMBOL   // Аббревиатура названия группы
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(SYMBOL));
            }
        }

        [Column("RockSort")]
        public string ROCKSORT   // Виды пород входящие в группу/тип
        {
            get => rocksort;
            set
            {
                rocksort = value;
                OnPropertyChanged(nameof(ROCKSORT));
            }
        }

        [Column("Description")]
        public string DESCRIPTION   // Описание
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }

        [Column("Language"), NotNull, Indexed]
        public string LANGUAGE   // Язык
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(LANGUAGE));
            }
        }



        public string rockrang;
        public string symbol;
        public string rocksort;
        public string description;
        public string language;

        public RockRangModel()
        {
            ROCKRANG = string.Empty;
            SYMBOL = string.Empty;
            ROCKSORT = null;
            DESCRIPTION = null;
            LANGUAGE = string.Empty;
        }
    }
}