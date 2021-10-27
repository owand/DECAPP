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
    public class RigType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<RigTypeJoin> collection;
        public ObservableCollection<RigTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<RigTypeJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<RigTypeJoin> _collection = (from collection in App.Database.Table<RigTypeModel>().ToList()
                                             join subCollection in App.Database.Table<RigTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                             from subCollection in joinCollection.DefaultIfEmpty(new RigTypeSubModel() { })
                                             select new RigTypeJoin()
                                             {
                                                 ID = collection.TYPEID,
                                                 DRIVETYPEID = collection.DRIVETYPEID,
                                                 TYPENAME = collection.TYPENAME,
                                                 TONNAGE = collection.TONNAGE,
                                                 PICTURE = collection.PICTURE,
                                                 DESCRIPTION = subCollection.DESCRIPTION,
                                                 FEATURE = subCollection.FEATURE,
                                                 NOTE = subCollection.NOTE
                                             }).OrderBy(a => a.TYPENAME).Where(a =>
                                             (string.IsNullOrEmpty(FilterCriterion) || a.DRIVETYPEID.ToString().Equals(FilterCriterion)) &&
                   (a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                   (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                   (!string.IsNullOrEmpty(a.FEATURE) && a.FEATURE.ToLowerInvariant().Contains(searchCriterion)) ||
                   (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (RigTypeJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<RigTypeJoin>(_collection);
        }

        private RigTypeJoin selectItem = null;
        public RigTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private RigTypeJoin newJoinItem = null;
        public RigTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public RigTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------

        #region --------- Основная коллекция --------

        private RigTypeModel selectHostItem = null;
        public RigTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public RigTypeModel GetSelectHostItem()
        {
            return App.Database.Table<RigTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private RigTypeModel newHostItem = null;
        public RigTypeModel NewHostItem
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

        private RigTypeSubModel selectSubItem = null;
        public RigTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private RigTypeSubModel newSubItem = null;
        public RigTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public RigTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<RigTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
        }

        #endregion ------------------------------------


        private List<DriveTypeModel> typeList = App.Database.Table<DriveTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<DriveTypeModel> TypeList
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
            IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.DRIVETYPEID).FirstOrDefault());
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


        public RigType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<RigTypeModel>().Any())
            {
                Collection?.Add(new RigTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new RigTypeJoin();
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
                    App.Database.Delete<RigTypeModel>(SelectItem.ID);
                    App.Database.Delete<RigTypeSubModel>(SelectItem.ID);
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



    public class RigTypeJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код группы

        [Column("DriveTypeID"), NotNull, Indexed, ForeignKey(typeof(DriveTypeModel))]     // Specify the foreign key
        public int DRIVETYPEID
        {
            get => drivetypeid;
            set
            {
                drivetypeid = value;
                OnPropertyChanged(nameof(DRIVETYPEID));
            }
        }

        public string TYPENAME    // Название номенклатурной группы
        {
            get => typename;
            set
            {
                typename = value;
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        public int TONNAGE
        {
            get => tonnage;
            set
            {
                tonnage = value;
                OnPropertyChanged(nameof(TONNAGE));
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

        [ManyToOne]      // Many to one relationship with DriveTypes
        public DriveTypeModel DriveTypes { get; set; }



        // Catalog
        public int drivetypeid;
        public string typename;
        public int tonnage;
        public byte[] picture;

        // Sub Catalog
        public string description;
        public string feature;
        public string note;


        public RigTypeJoin()
        {
        }
    }



    //Таблица видов буровых станков
    [Table("tbDrillRigType")]
    public class RigTypeModel : ViewModelBase
    {
        [Column("DrillRigTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("DriveTypeID"), NotNull, Indexed, ForeignKey(typeof(DriveTypeModel))]     // Specify the foreign key
        public int DRIVETYPEID
        {
            get => drivetypeid;
            set
            {
                drivetypeid = value;
                OnPropertyChanged(nameof(DRIVETYPEID));
            }
        }

        [Column("DrillRigType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        [Column("Tonnage")]
        public int TONNAGE
        {
            get => tonnage;
            set
            {
                tonnage = value;
                OnPropertyChanged(nameof(TONNAGE));
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


        public int drivetypeid;
        public string typename;
        public int tonnage;
        public byte[] picture;

        public RigTypeModel()
        {
        }
    }



    //Таблица видов буровых станков
    [Table("tbDrillRigTypeML")]
    public class RigTypeSubModel : ViewModelBase
    {
        [Column("DrillRigTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int RIGTYPESUBID { get; set; }   // Уникальный код группы

        [Column("DrillRigTypeID"), NotNull, Indexed, ForeignKey(typeof(RigTypeModel))]     // Specify the foreign key
        public int TYPEID
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



        public int typeid;
        public string description;
        public string feature;
        public string note;
        public string language;

        public RigTypeSubModel()
        {
        }
    }
}