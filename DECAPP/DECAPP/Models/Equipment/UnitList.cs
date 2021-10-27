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
    public class UnitList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<UnitJoin> collection;
        public ObservableCollection<UnitJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<UnitJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<UnitJoin> _collection = (from collection in App.Database.Table<UnitModel>().ToList()
                                          join subCollection in App.Database.Table<UnitSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.UNITID equals subCollection.UNITID into joinCollection
                                          from subCollection in joinCollection.DefaultIfEmpty(new UnitSubModel() { })
                                          select new UnitJoin()
                                          {
                                              ID = collection.UNITID,
                                              TYPEID = collection.TYPEID,
                                              UNITNAME = collection.UNITNAME,
                                              MASS = collection.MASS,
                                              WIDTH = collection.WIDTH,
                                              HEIGHT = collection.HEIGHT,
                                              LENGTH = collection.LENGTH,
                                              PICTURE = collection.PICTURE,
                                              DESCRIPTION = subCollection.DESCRIPTION,
                                              FEATURE = subCollection.FEATURE,
                                              NOTE = subCollection.NOTE
                                          }).OrderBy(a => a.UNITNAME).Where(a =>
                                          (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                 (a.UNITNAME.ToLowerInvariant().Contains(searchCriterion) ||
                 (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                 (!string.IsNullOrEmpty(a.FEATURE) && a.FEATURE.ToLowerInvariant().Contains(searchCriterion)) ||
                 (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (UnitJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<UnitJoin>(_collection);
        }

        private UnitJoin selectItem = null;
        public UnitJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private UnitJoin newJoinItem = null;
        public UnitJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public UnitJoin PreSelectItem { get; set; }

        #endregion ------------------------------------

        #region --------- Основная коллекция --------

        private UnitModel selectHostItem = null;
        public UnitModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public UnitModel GetSelectHostItem()
        {
            return App.Database.Table<UnitModel>().FirstOrDefault(a => a.UNITID == SelectItem.ID);
        }

        private UnitModel newHostItem = null;
        public UnitModel NewHostItem
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

        private UnitSubModel selectSubItem = null;
        public UnitSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private UnitSubModel newSubItem = null;
        public UnitSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public UnitSubModel GetSelectSubItem()
        {
            return App.Database.Table<UnitSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.UNITID == SelectItem.ID);
        }

        #endregion ------------------------------------


        private List<UnitTypeModel> typeList = App.Database.Table<UnitTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<UnitTypeModel> TypeList
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


        public UnitList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<UnitModel>().Any())
            {
                Collection?.Add(new UnitJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new UnitJoin();
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
                    App.Database.Delete<UnitModel>(SelectItem.ID);
                    App.Database.Delete<UnitSubModel>(SelectItem.ID);
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
    public class UnitJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код группы

        [Column("UnitTypeID"), NotNull, Indexed, ForeignKey(typeof(UnitTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string UNITNAME
        {
            get => unitname;
            set
            {
                unitname = value;
                OnPropertyChanged(nameof(UNITNAME));
            }
        }

        public decimal MASS
        {
            get => mass;
            set
            {
                mass = value;
                OnPropertyChanged(nameof(MASS));
            }
        }
        public string MASSFORMAT => string.Format("{0:N2}", mass); // Поле в американском формате

        public decimal WIDTH
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged(nameof(WIDTH));
            }
        }
        public string WIDTHFORMAT => string.Format("{0}", width); // Поле в американском формате

        public decimal HEIGHT
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged(nameof(HEIGHT));
            }
        }
        public string HEIGHTFORMAT => string.Format("{0}", height); // Поле в американском формате

        public decimal LENGTH
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(LENGTH));
            }
        }
        public string LENGTHFORMAT => string.Format("{0}", length); // Поле в американском формате

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

        public string FEATURE   // Характеристики
        {
            get => feature;
            set
            {
                feature = value;
                OnPropertyChanged(nameof(FEATURE));
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

        [ManyToOne]      // Many to one relationship with MudTypes
        public UnitTypeModel UnitType { get; set; }



        // Catalog
        public int typeId;
        public string unitname;
        public decimal mass;
        public decimal width;
        public decimal height;
        public decimal length;
        public byte[] picture;

        // Sub Catalog
        public string description;
        public string feature;
        public string note;


        public UnitJoin()
        {
        }
    }


    //Таблица оборудования
    [Table("tbUnit")]
    public class UnitModel : ViewModelBase
    {
        [Column("UnitID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int UNITID { get; set; }   // Уникальный код

        [Column("UnitTypeID"), NotNull, Indexed, ForeignKey(typeof(UnitTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("UnitName"), Unique, NotNull, Indexed]
        public string UNITNAME
        {
            get => unitname.ToUpper();
            set
            {
                unitname = value.ToUpper();
                OnPropertyChanged(nameof(UNITNAME));
            }
        }

        [Column("Mass")]
        public decimal MASS
        {
            get => mass;
            set
            {
                mass = value;
                OnPropertyChanged(nameof(MASS));
            }
        }

        [Column("Width")]
        public decimal WIDTH
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged(nameof(WIDTH));
            }
        }

        [Column("Height")]
        public decimal HEIGHT
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged(nameof(HEIGHT));
            }
        }

        [Column("Length")]
        public decimal LENGTH
        {
            get => length;
            set
            {
                length = value;
                OnPropertyChanged(nameof(LENGTH));
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



        public int unitid;
        public string unitname;
        public int typeId;
        public decimal mass;
        public decimal width;
        public decimal height;
        public decimal length;
        public byte[] picture;

        public UnitModel()
        {
        }
    }



    //Таблица оборудования
    [Table("tbUnitML")]
    public class UnitSubModel : ViewModelBase
    {
        [Column("UnitMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int UNITSUBID { get; set; }   // Уникальный код

        [Column("UnitID"), NotNull, Indexed, ForeignKey(typeof(UnitTypeModel))]     // Specify the foreign key
        public int UNITID   // Уникальный код группы
        {
            get => unitid;
            set
            {
                unitid = value;
                OnPropertyChanged(nameof(UNITID));
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

        [Column("Feature")]
        public string FEATURE   // Характеристики
        {
            get => feature;
            set
            {
                feature = value;
                OnPropertyChanged(nameof(FEATURE));
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



        public int unitid;
        public string description;
        public string feature;
        public string note;
        public string language;

        public UnitSubModel()
        {
        }
    }
}