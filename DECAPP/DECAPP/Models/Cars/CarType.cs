using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Cars
{
    public class CarType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<CarTypeJoin> collection;
        public ObservableCollection<CarTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CarTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<CarTypeJoin> _collection = (from collection in App.Database.Table<CarTypeModel>().ToList()
                                             join subCollection in App.Database.Table<CarTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                             from subCollection in joinCollection.DefaultIfEmpty(new CarTypeSubModel() { })
                                             select new CarTypeJoin()
                                             {
                                                 ID = collection.TYPEID,
                                                 TYPENAME = collection.TYPENAME,
                                                 PICTURE = collection.PICTURE,
                                                 DESCRIPTION = subCollection.DESCRIPTION,
                                                 NOTE = subCollection.NOTE
                                             }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                             (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                             (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<CarTypeJoin>(_collection);
        }

        private CarTypeJoin selectItem = null;
        public CarTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private CarTypeJoin newJoinItem = null;
        public CarTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public CarTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private CarTypeModel selectHostItem = null;
        public CarTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public CarTypeModel GetSelectHostItem()
        {
            return App.Database.Table<CarTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private CarTypeModel newHostItem = null;
        public CarTypeModel NewHostItem
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

        private CarTypeSubModel selectSubItem = null;
        public CarTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private CarTypeSubModel newSubItem = null;
        public CarTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public CarTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<CarTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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


        public CarType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<CarTypeModel>().Any())
            {
                Collection?.Add(new CarTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new CarTypeJoin();
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
                    App.Database.Delete<CarTypeModel>(SelectItem.ID);
                    App.Database.Delete<CarTypeSubModel>(SelectItem.ID);
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



    public class CarTypeJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный

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
        public string description;
        public string note;

        public CarTypeJoin()
        {
        }
    }


    //Таблица групп автотранспортной, тампонажной и специальной техники
    [Table("tbCarType")]
    public class CarTypeModel : ViewModelBase
    {
        [Column("CarTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("CarType"), NotNull, Unique, Indexed]
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



        public string typename;
        public byte[] picture;

        public CarTypeModel()
        {
            //this.TYPENAME = null;
            //this.PICTURE = null;
        }
    }



    //Таблица групп автотранспортной, тампонажной и специальной техники
    [Table("tbCarTypeML")]
    public class CarTypeSubModel : ViewModelBase
    {
        [Column("CarTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int CARTYPEMLID { get; set; }   // Уникальный код

        [Column("CarTypeID"), NotNull, Indexed, ForeignKey(typeof(CarTypeModel))]     // Specify the foreign key
        public int TYPEID   // Уникальный код группы
        {
            get => typeid;
            set
            {
                typeid = value;
                OnPropertyChanged(nameof(TYPEID));
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
        public string description;
        public string note;
        public string language;

        public CarTypeSubModel()
        {
        }
    }
}