using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Mud
{
    public class MudList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<MudJoin> collection;
        public ObservableCollection<MudJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MudJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<MudJoin> _collection = (from collection in App.Database.Table<MudModel>().ToList()
                                         join subCollection in App.Database.Table<MudSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.MUDID equals subCollection.MUDID into joinCollection
                                         from subCollection in joinCollection.DefaultIfEmpty(new MudSubModel() { })
                                         select new MudJoin()
                                         {
                                             ID = collection.MUDID,
                                             TYPEID = collection.TYPEID,
                                             MUDNAME = collection.MUDNAME,
                                             FORMULA = collection.FORMULA,
                                             DENSITY = collection.DENSITY,
                                             MinTEMP = collection.MinTEMP,
                                             MaxTEMP = collection.MaxTEMP,
                                             PICTURE = collection.PICTURE,
                                             GOST = subCollection.GOST,
                                             ANALOG = subCollection.ANALOG,
                                             FUNCTION = subCollection.FUNCTION,
                                             DESCRIPTION = subCollection.DESCRIPTION,
                                             NOTE = subCollection.NOTE
                                         }).OrderBy(a => a.MUDNAME).Where(a =>
                                         (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                                         (a.MUDNAME.ToLowerInvariant().Contains(searchCriterion) ||
             (!string.IsNullOrEmpty(a.FORMULA) && a.FORMULA.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.GOST) && a.GOST.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.ANALOG) && a.ANALOG.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.FUNCTION) && a.FUNCTION.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (MudJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<MudJoin>(_collection);
        }

        private MudJoin selectItem = null;
        public MudJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private MudJoin newJoinItem = null;
        public MudJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public MudJoin PreSelectItem { get; set; }

        #endregion ------------------------------------

        #region --------- Основная коллекция --------

        private MudModel selectHostItem = null;
        public MudModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public MudModel GetSelectHostItem()
        {
            return App.Database.Table<MudModel>().FirstOrDefault(a => a.MUDID == SelectItem.ID);
        }

        private MudModel newHostItem = null;
        public MudModel NewHostItem
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

        private MudSubModel selectSubItem = null;
        public MudSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }
        public MudSubModel GetSelectSubItem()
        {
            return App.Database.Table<MudSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.MUDID == SelectItem.ID);
        }

        private MudSubModel newSubItem = null;
        public MudSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }

        #endregion ------------------------------------


        private List<MudTypeModel> typeList = App.Database.Table<MudTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<MudTypeModel> TypeList
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


        public MudList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<MudModel>().Any())
            {
                Collection?.Add(new MudJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new MudJoin();
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
                    App.Database.Delete<MudModel>(SelectItem.ID);
                    App.Database.Delete<MudSubModel>(SelectItem.ID);
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
    public class MudJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код

        [ForeignKey(typeof(MudTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string MUDNAME
        {
            get => mudname;
            set
            {
                mudname = value;
                OnPropertyChanged(nameof(MUDNAME));
            }
        }

        public string FORMULA
        {
            get => formula;
            set
            {
                formula = value;
                OnPropertyChanged(nameof(FORMULA));
            }
        }

        public decimal DENSITY
        {
            get => density;
            set
            {
                density = value;
                OnPropertyChanged(nameof(DENSITY));
            }
        }
        public string DENSITYFORMAT => string.Format("{0:N3}", density); // Поле в американском формате

        public int MinTEMP
        {
            get => minTemp;
            set
            {
                minTemp = value;
                OnPropertyChanged(nameof(MinTEMP));
            }
        }
        public string MinTEMPFORMAT => string.Format("{0}", minTemp); // Поле в американском формате

        public int MaxTEMP
        {
            get => maxTemp;
            set
            {
                maxTemp = value;
                OnPropertyChanged(nameof(MaxTEMP));
            }
        }
        public string MaxTEMPFORMAT => string.Format("{0}", maxTemp); // Поле в американском формате

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
        public string GOST
        {
            get => gost;
            set
            {
                gost = value;
                OnPropertyChanged(nameof(GOST));
            }
        }

        public string ANALOG   // Аналоги
        {
            get => analog;
            set
            {
                analog = value;
                OnPropertyChanged(nameof(ANALOG));
            }
        }

        public string FUNCTION   // Назначение / применение
        {
            get => function;
            set
            {
                function = value;
                OnPropertyChanged(nameof(FUNCTION));
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

        [ManyToOne]      // Many to one relationship with MudTypes
        public MudTypeModel MudType { get; set; }

        #region
        // Catalog
        public int typeId;
        public string mudname;
        public string formula;
        public decimal density;
        public int minTemp;
        public int maxTemp;
        public byte[] picture;

        // Sub Catalog
        public string gost;
        public string analog;
        public string function;
        public string description;
        public string note;
        #endregion

        public MudJoin()
        {
        }
    }



    //Таблица каталог реагентов и тампонажных смесей
    [Table("tbMud")]
    public class MudModel : ViewModelBase
    {
        [Column("MudID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int MUDID { get; set; }   // Уникальный код реагентов и тампонажных смесей

        [Column("MudTypeID"), NotNull, Indexed, ForeignKey(typeof(MudTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("MudName"), Unique, NotNull, Indexed]
        public string MUDNAME
        {
            get => mudname.ToUpper();
            set
            {
                mudname = value.ToUpper();
                OnPropertyChanged(nameof(MUDNAME));
            }
        }

        [Column("Formula")]
        public string FORMULA
        {
            get => formula;
            set
            {
                formula = value;
                OnPropertyChanged(nameof(FORMULA));
            }
        }

        [Column("Density")]
        public decimal DENSITY
        {
            get => density;
            set
            {
                density = value;
                OnPropertyChanged(nameof(DENSITY));
            }
        }

        [Column("MinTemp")]
        public int MinTEMP
        {
            get => minTemp;
            set
            {
                minTemp = value;
                OnPropertyChanged(nameof(MinTEMP));
            }
        }

        [Column("MaxTemp")]
        public int MaxTEMP
        {
            get => maxTemp;
            set
            {
                maxTemp = value;
                OnPropertyChanged(nameof(MaxTEMP));
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


        #region
        public string mudname;
        public byte[] picture;
        public int typeId;
        public string formula;
        public decimal density;
        public int minTemp;
        public int maxTemp;
        #endregion

        public MudModel()
        {
        }
    }



    //Таблица каталог реагентов и тампонажных смесей
    [Table("tbMudML")]
    public class MudSubModel : ViewModelBase
    {
        [Column("MudMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int MUDSUBID { get; set; }   // Уникальный код

        [Column("MudID"), NotNull, Indexed, ForeignKey(typeof(MudModel))]     // Specify the foreign key
        public int MUDID
        {
            get => mudid;
            set
            {
                mudid = value;
                OnPropertyChanged(nameof(MUDID));
            }
        }

        [Column("GOST")]
        public string GOST
        {
            get => gost;
            set
            {
                gost = value;
                OnPropertyChanged(nameof(GOST));
            }
        }

        [Column("Analog")]
        public string ANALOG   // Аналоги
        {
            get => analog;
            set
            {
                analog = value;
                OnPropertyChanged(nameof(ANALOG));
            }
        }

        [Column("Function")]
        public string FUNCTION   // Назначение / применение
        {
            get => function;
            set
            {
                function = value;
                OnPropertyChanged(nameof(FUNCTION));
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



        public int mudid;
        public string gost;
        public string analog;
        public string function;
        public string description;
        public string note;
        public string language;

        public MudSubModel()
        {
        }
    }
}