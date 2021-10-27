using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class UnitType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        private ObservableCollection<UnitTypeJoin> collection;
        public ObservableCollection<UnitTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(UnitTypeJoin));
            }
        }
        public ObservableCollection<UnitTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<UnitTypeJoin> _collection = (from collection in App.Database.Table<UnitTypeModel>().ToList()
                                              join subCollection in App.Database.Table<UnitTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                              from subCollection in joinCollection.DefaultIfEmpty(new UnitTypeSubModel() { })
                                              select new UnitTypeJoin()
                                              {
                                                  ID = collection.TYPEID,
                                                  TYPENAME = collection.TYPENAME,
                                                  PICTURE = collection.PICTURE,
                                                  SYMBOL = subCollection.SYMBOL,
                                                  DESCRIPTION = subCollection.DESCRIPTION,
                                                  NOTE = subCollection.NOTE
                                              }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
              (!string.IsNullOrEmpty(a.SYMBOL) && a.SYMBOL.ToLowerInvariant().Contains(searchCriterion)) ||
              (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
              (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<UnitTypeJoin>(_collection);
        }

        private UnitTypeJoin selectItem = null;
        public UnitTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private UnitTypeJoin newJoinItem = null;
        public UnitTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }
        public UnitTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private UnitTypeModel selectHostItem = null;
        public UnitTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public UnitTypeModel GetSelectHostItem()
        {
            return App.Database.Table<UnitTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private UnitTypeModel newHostItem = null;
        public UnitTypeModel NewHostItem
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

        private UnitTypeSubModel selectSubItem = null;
        public UnitTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private UnitTypeSubModel newSubItem = null;
        public UnitTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public UnitTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<UnitTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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

        public UnitType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<UnitTypeModel>().Any())
            {
                Collection?.Add(new UnitTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new UnitTypeJoin();
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
                    App.Database.Delete<UnitTypeModel>(SelectItem.ID);
                    App.Database.Delete<UnitTypeSubModel>(SelectItem.ID);
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

    // Объединенная коллекция
    public class UnitTypeJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код группы

        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename;
            set
            {
                typename = value;
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        public byte[] PICTURE
        {
            get => picture;
            set
            {
                picture = value;
                OnPropertyChanged(nameof(PICTURE));
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
        public string typename;
        public byte[] picture;

        // Sub Catalog
        public string symbol;
        public string description;
        public string note;

        public UnitTypeJoin()
        {
        }
    }


    //Таблица типов оборудования
    [Table("tbUnitType")]
    public class UnitTypeModel : ViewModelBase
    {
        [Column("UnitTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("UnitType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        [Column("Picture")]
        public byte[] PICTURE
        {
            get => picture;
            set
            {
                picture = value;
                OnPropertyChanged(nameof(PICTURE));
            }
        }



        public int typeid;
        public string typename;
        public byte[] picture;

        public UnitTypeModel()
        {
        }
    }


    //Таблица типов оборудования
    [Table("tbUnitTypeML")]
    public class UnitTypeSubModel : ViewModelBase
    {
        [Column("UnitTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int UNITTYPESUBID { get; set; }   // Уникальный код

        [Column("UnitTypeID"), NotNull, Indexed, ForeignKey(typeof(UnitTypeModel))]     // Specify the foreign key
        public int TYPEID   // Уникальный код группы
        {
            get => typeid;
            set
            {
                typeid = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("Symbol")]
        public string SYMBOL
        {
            get => symbol.ToUpper();
            set
            {
                symbol = value.ToUpper();
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



        public int typeid;
        public string symbol;
        public string description;
        public string note;
        public string language;

        public UnitTypeSubModel()
        {
        }
    }
}