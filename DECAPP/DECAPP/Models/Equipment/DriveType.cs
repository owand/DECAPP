using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class DriveType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<DriveTypeJoin> collection;
        public ObservableCollection<DriveTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DriveTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<DriveTypeJoin> _collection = (from collection in App.Database.Table<DriveTypeModel>().ToList()
                                               join subCollection in App.Database.Table<DriveTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                               from subCollection in joinCollection.DefaultIfEmpty(new DriveTypeSubModel() { })
                                               select new DriveTypeJoin()
                                               {
                                                   ID = collection.TYPEID,
                                                   TYPENAME = collection.TYPENAME,
                                                   DESCRIPTION = subCollection.DESCRIPTION,
                                                   NOTE = subCollection.NOTE
                                               }).OrderBy(a => a.TYPENAME).Where(a =>
                                               a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                   (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                   (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            foreach (DriveTypeJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<DriveTypeJoin>(_collection);
        }

        private DriveTypeJoin selectItem = null;
        public DriveTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private DriveTypeJoin newJoinItem = null;
        public DriveTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public DriveTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private DriveTypeModel selectHostItem = null;
        public DriveTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public DriveTypeModel GetSelectHostItem()
        {
            return App.Database.Table<DriveTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private DriveTypeModel newHostItem = null;
        public DriveTypeModel NewHostItem
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

        private DriveTypeSubModel selectSubItem = null;
        public DriveTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private DriveTypeSubModel newSubItem = null;
        public DriveTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public DriveTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<DriveTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
        }

        #endregion ------------------------------------


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


        public DriveType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<DriveTypeModel>().Any())
            {
                Collection?.Add(new DriveTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new DriveTypeJoin();
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
                    App.Database.Delete<DriveTypeModel>(SelectItem.ID);
                    App.Database.Delete<DriveTypeSubModel>(SelectItem.ID);
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



    public class DriveTypeJoin : ViewModelBase
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

        // Sub Catalog
        public string symbol;
        public string description;
        public string note;

        public DriveTypeJoin()
        {
        }
    }



    //Таблица типов привода буровых станков
    [Table("tbDriveType")]
    public class DriveTypeModel : ViewModelBase
    {
        [Column("DriveTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("DriveType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }



        public string typename;

        public DriveTypeModel()
        {
        }
    }



    //Таблица типов привода буровых станков
    [Table("tbDriveTypeML")]
    public class DriveTypeSubModel : ViewModelBase
    {
        [Column("DriveTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int DRIVETYPESUBID { get; set; }   // Уникальный код

        [Column("DriveTypeID"), NotNull, Indexed, ForeignKey(typeof(DriveTypeModel))]     // Specify the foreign key
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

        public DriveTypeSubModel()
        {
        }
    }
}