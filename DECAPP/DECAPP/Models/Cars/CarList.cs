using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Cars
{
    public class CarList : ViewModelBase
    {

        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<CarJoin> collection;
        public ObservableCollection<CarJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CarJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<CarJoin> _collection = (from collection in App.Database.Table<CarModel>().ToList()
                                         join subCollection in App.Database.Table<CarSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.CARID equals subCollection.CARID into joinCollection
                                         from subCollection in joinCollection.DefaultIfEmpty(new CarSubModel() { })
                                         select new CarJoin()
                                         {
                                             ID = collection.CARID,
                                             TYPEID = collection.TYPEID,
                                             CARNAME = collection.CARNAME,
                                             PICTURE = collection.PICTURE,
                                             DESCRIPTION = subCollection.DESCRIPTION,
                                             NOTE = subCollection.NOTE
                                         }).OrderBy(a => a.CARNAME).Where(a =>
                                         (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                (a.CARNAME.ToLowerInvariant().Contains(searchCriterion) ||
                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (CarJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<CarJoin>(_collection);
        }

        private CarJoin selectItem = null;
        public CarJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private CarJoin newJoinItem = null;
        public CarJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public CarJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private CarModel selectHostItem = null;
        public CarModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public CarModel GetSelectHostItem()
        {
            return App.Database.Table<CarModel>().FirstOrDefault(a => a.CARID == SelectItem.ID);
        }

        private CarModel newHostItem = null;
        public CarModel NewHostItem
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

        private CarSubModel selectSubItem = null;
        public CarSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private CarSubModel newSubItem = null;
        public CarSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public CarSubModel GetSelectSubItem()
        {
            return App.Database.Table<CarSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.CARID == SelectItem.ID);
        }

        #endregion ------------------------------------


        private List<CarTypeModel> typeList = App.Database.Table<CarTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<CarTypeModel> TypeList
        {
            get => typeList;
            set
            {
                typeList = value;
                OnPropertyChanged(nameof(TypeList));
            }
        }

        private int indexTypeList;
        public int IndexTypeList
        {
            get => indexTypeList;
            set
            {
                indexTypeList = value;
                OnPropertyChanged(nameof(IndexTypeList));
            }
        }
        public int GetIndexTypeList()
        {
            IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.TYPEID).FirstOrDefault());
            return IndexTypeList;
        }

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

        public CarList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<CarModel>().Any())
            {
                Collection?.Add(new CarJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new CarJoin();
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
                    App.Database.Delete<CarModel>(SelectItem.ID);
                    App.Database.Delete<CarSubModel>(SelectItem.ID);
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



    public class CarJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код

        [Column("CarTypeID"), ForeignKey(typeof(CarTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string CARNAME
        {
            get => carname;
            set
            {
                carname = value;
                OnPropertyChanged(nameof(CARNAME));
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

        public string LANGUAGE   // Язык
        {
            get => language;
            set
            {
                language = value;
                OnPropertyChanged(nameof(LANGUAGE));
            }
        }

        [ManyToOne]
        public CarTypeModel CarTypes { get; set; }



        // Catalog
        public int typeId;
        public string carname;
        public byte[] picture;

        // Sub Catalog
        public string description;
        public string note;
        public string language;

        public CarJoin()
        {
        }
    }



    //Таблица автотранспортной, тампонажной и специальной техники
    [Table("tbCars")]
    public class CarModel : ViewModelBase
    {
        [Column("CarID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int CARID { get; set; }   // Уникальный код

        [Column("CarTypeID"), NotNull, Indexed, ForeignKey(typeof(CarTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("CarName"), NotNull, Unique, Indexed]
        public string CARNAME
        {
            get => carname.ToUpper();
            set
            {
                carname = value.ToUpper();
                OnPropertyChanged(nameof(CARNAME));
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



        public int typeId;
        public string carname;
        public byte[] picture;

        public CarModel()
        {
        }
    }



    //Таблица автотранспортной, тампонажной и специальной техники
    [Table("tbCarsML")]
    public class CarSubModel : ViewModelBase
    {
        [Column("CarMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int CARSUBID { get; set; }   // Уникальный код

        [Column("CarID"), NotNull, Indexed, ForeignKey(typeof(CarModel))]     // Specify the foreign key
        public int CARID
        {
            get => carid;
            set
            {
                carid = value;
                OnPropertyChanged(nameof(CARID));
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



        public int carid;
        public string description;
        public string note;
        public string language;

        public CarSubModel()
        {
        }
    }
}