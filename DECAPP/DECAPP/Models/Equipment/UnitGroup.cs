using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class UnitGroup : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        private ObservableCollection<UnitGroupJoin> collection;
        public ObservableCollection<UnitGroupJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(UnitGroupJoin));
            }
        }
        public ObservableCollection<UnitGroupJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<UnitGroupJoin> _collection = (from collection in App.Database.Table<UnitGroupModel>().ToList()
                                               join subCollection in App.Database.Table<UnitGroupSubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.GROUPID equals subCollection.GROUPID into joinCollection
                                               from subCollection in joinCollection.DefaultIfEmpty(new UnitGroupSubModel() { })
                                               select new UnitGroupJoin()
                                               {
                                                   ID = collection.GROUPID,
                                                   GROUPNAME = collection.GROUPNAME,
                                                   SYMBOL = subCollection.SYMBOL,
                                                   DESCRIPTION = subCollection.DESCRIPTION,
                                                   NOTE = subCollection.NOTE
                                               }).OrderBy(a => a.GROUPNAME).Where(a => a.GROUPNAME.ToLowerInvariant().Contains(searchCriterion) ||
               (!string.IsNullOrEmpty(a.SYMBOL) && a.SYMBOL.ToLowerInvariant().Contains(searchCriterion)) ||
               (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
               (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<UnitGroupJoin>(_collection);
        }

        private UnitGroupJoin selectItem = null;
        public UnitGroupJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private UnitGroupJoin newJoinItem = null;
        public UnitGroupJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }
        public UnitGroupJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private UnitGroupModel selectHostItem = null;
        public UnitGroupModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public UnitGroupModel GetSelectHostItem()
        {
            return App.Database.Table<UnitGroupModel>().FirstOrDefault(a => a.GROUPID == SelectItem.ID);
        }

        private UnitGroupModel newHostItem = null;
        public UnitGroupModel NewHostItem
        {
            get => newHostItem;
            set
            {
                newHostItem = value;
                OnPropertyChanged(nameof(NewHostItem));
            }
        }

        #endregion ------------------------------------


        #region --------- Подчиненная коллекция --------

        private UnitGroupSubModel selectSubItem = null;
        public UnitGroupSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private UnitGroupSubModel newSubItem = null;
        public UnitGroupSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public UnitGroupSubModel GetSelectSubItem()
        {
            return App.Database.Table<UnitGroupSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.GROUPID == SelectItem.ID);
        }

        #endregion ------------------------------------


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

        public UnitGroup()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<UnitGroupModel>().Any())
            {
                Collection?.Add(new UnitGroupJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new UnitGroupJoin();
                Collection.Add(NewJoinItem);
                SelectItem = NewJoinItem;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                if (NewJoinItem != null)
                {
                    Collection.Remove(NewJoinItem);
                    NewJoinItem = null;
                }
                return;
            }
        }

        // Сохраняем новую или изменяем запись в основной коллекции
        public void UpdateItem()
        {
            try
            {
                lock (collisionLock)
                {
                    if (SelectHostItem != null)
                    {
                        App.Database.Update(SelectHostItem);
                    }
                    else
                    {
                        App.Database.Insert(NewHostItem);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Сохраняем новую или изменяем запись в подчиненной коллекции
        public void UpdateSubItem()
        {
            try
            {
                lock (collisionLock)
                {
                    if (SelectSubItem != null)
                    {
                        App.Database.Update(SelectSubItem);
                    }
                    else
                    {
                        App.Database.Insert(NewSubItem);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Delete<UnitGroupModel>(SelectItem.ID);
                    App.Database.Delete<UnitGroupSubModel>(SelectItem.ID);
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



    public class UnitGroupJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код группы

        public string GROUPNAME   // Название номенклатурной группы
        {
            get => groupname;
            set
            {
                groupname = value;
                OnPropertyChanged(nameof(GROUPNAME));
            }
        }

        // Sub Catalog
        public string SYMBOL
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(SYMBOL));
            }
        }

        public string DESCRIPTION   // Описание
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(DESCRIPTION));
            }
        }

        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
            }
        }



        // Catalog
        public string groupname;

        // Sub Catalog
        public string symbol;
        public string description;
        public string note;


        public UnitGroupJoin()
        {
        }
    }


    //Таблица групп оборудования
    [Table("tbUnitGroup")]
    public class UnitGroupModel : ViewModelBase
    {
        [Column("UnitGroupID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int GROUPID { get; set; }   // Уникальный код группы

        [Column("UnitGroup"), NotNull, Unique, Indexed]
        public string GROUPNAME   // Название номенклатурной группы
        {
            get => groupname.ToUpper();
            set
            {
                groupname = value.ToUpper();
                OnPropertyChanged(nameof(GROUPNAME));
            }
        }



        public string groupname;

        public UnitGroupModel()
        {
        }
    }



    //Таблица групп оборудования
    [Table("tbUnitGroupML")]
    public class UnitGroupSubModel : ViewModelBase
    {
        [Column("UnitGroupMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int UNITGROUPSUBID { get; set; }   // Уникальный код

        [Column("UnitGroupID"), NotNull, Indexed, ForeignKey(typeof(UnitGroupModel))]     // Specify the foreign key
        public int GROUPID   // Уникальный код группы
        {
            get => groupid;
            set
            {
                groupid = value;
                OnPropertyChanged(nameof(GROUPID));
            }
        }

        [Column("Symbol")]
        public string SYMBOL
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(SYMBOL));
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

        [Column("Note")]
        public string NOTE   // Примечания
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
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



        public int groupid;
        public string symbol;
        public string description;
        public string note;
        public string language;

        public UnitGroupSubModel()
        {
        }
    }
}
