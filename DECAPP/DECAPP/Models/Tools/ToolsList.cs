using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Tools
{
    public class ToolsList : ViewModelBase
    {

        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<ToolsJoin> collection;
        public ObservableCollection<ToolsJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ToolsJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<ToolsJoin> _collection = (from collection in App.Database.Table<ToolsModel>().ToList()
                                           join subCollection in App.Database.Table<ToolsSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TOOLSID equals subCollection.TOOLSID into joinCollection
                                           from subCollection in joinCollection.DefaultIfEmpty(new ToolsSubModel() { })
                                           select new ToolsJoin()
                                           {
                                               ID = collection.TOOLSID,
                                               TYPEID = collection.TYPEID,
                                               TOOLSNAME = collection.TOOLSNAME,
                                               PICTURE = collection.PICTURE,
                                               ANALOG = subCollection.ANALOG,
                                               DESCRIPTION = subCollection.DESCRIPTION,
                                               FUNCTION = subCollection.FUNCTION,
                                               NOTE = subCollection.NOTE
                                           }).OrderBy(a => a.TOOLSNAME).Where(a =>
                                           (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                  (a.TOOLSNAME.ToLowerInvariant().Contains(searchCriterion) ||
                  (!string.IsNullOrEmpty(a.ANALOG) && a.ANALOG.ToLowerInvariant().Contains(searchCriterion)) ||
                  (!string.IsNullOrEmpty(a.FUNCTION) && a.FUNCTION.ToLowerInvariant().Contains(searchCriterion)) ||
                  (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                  (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (ToolsJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<ToolsJoin>(_collection);
        }

        private ToolsJoin selectItem = null;
        public ToolsJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private ToolsJoin newJoinItem = null;
        public ToolsJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public ToolsJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private ToolsModel selectHostItem = null;
        public ToolsModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public ToolsModel GetSelectHostItem()
        {
            return App.Database.Table<ToolsModel>().FirstOrDefault(a => a.TOOLSID == SelectItem.ID);
        }

        private ToolsModel newHostItem = null;
        public ToolsModel NewHostItem
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

        private ToolsSubModel selectSubItem = null;
        public ToolsSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private ToolsSubModel newSubItem = null;
        public ToolsSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public ToolsSubModel GetSelectSubItem()
        {
            return App.Database.Table<ToolsSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TOOLSID == SelectItem.ID);
        }

        #endregion ------------------------------------


        private List<ToolsTypeModel> typeList = App.Database.Table<ToolsTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<ToolsTypeModel> TypeList
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

        public ToolsList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<ToolsModel>().Any())
            {
                Collection?.Add(new ToolsJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new ToolsJoin();
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
                    App.Database.Delete<ToolsModel>(SelectItem.ID);
                    App.Database.Delete<ToolsSubModel>(SelectItem.ID);
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



    public class ToolsJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код технологической оснастки

        [Column("ToolsTypeID"), ForeignKey(typeof(ToolsTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string TOOLSNAME
        {
            get => toolsname;
            set
            {
                toolsname = value;
                OnPropertyChanged(nameof(TOOLSNAME));
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
        public string ANALOG   // Аналоги
        {
            get => analog;
            set
            {
                analog = value;
                OnPropertyChanged(nameof(ANALOG));
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

        public string FUNCTION   // Назначение / применение
        {
            get => function;
            set
            {
                function = value;
                OnPropertyChanged(nameof(FUNCTION));
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

        [ManyToOne]      // Many to one relationship with ToolsTypes
        public ToolsTypeModel ToolsTypes { get; set; }



        // Catalog
        public int toolsid;
        public int typeId;
        public string toolsname;
        public byte[] picture;

        // Sub Catalog
        public string analog;
        public string description;
        public string function;
        public string note;
        public string language;

        public ToolsJoin()
        {
        }
    }



    //Таблица технологической оснастки
    [Table("tbTools")]
    public class ToolsModel : ViewModelBase
    {
        [Column("ToolsID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TOOLSID { get; set; }   // Уникальный код технологической оснастки

        [Column("ToolsTypeID"), NotNull, Indexed, ForeignKey(typeof(ToolsTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("ToolsName"), NotNull, Unique, Indexed]
        public string TOOLSNAME
        {
            get => toolsname.ToUpper();
            set
            {
                toolsname = value.ToUpper();
                OnPropertyChanged(nameof(TOOLSNAME));
            }
        }

        [Column("Picture")]
        public byte[] PICTURE
        {
            get => picture; // получает
            set // отдает
            {
                picture = value;
                OnPropertyChanged(nameof(PICTURE));
            }
        }



        public int typeId;
        public string toolsname;
        public byte[] picture;

        public ToolsModel()
        {
        }
    }



    //Таблица технологической оснастки
    [Table("tbToolsML")]
    public class ToolsSubModel : ViewModelBase
    {
        [Column("ToolsMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TOOLSSUBID { get; set; }   // Уникальный код

        [Column("ToolsID"), NotNull, Indexed, ForeignKey(typeof(ToolsModel))]     // Specify the foreign key
        public int TOOLSID
        {
            get => toolsid;
            set
            {
                toolsid = value;
                OnPropertyChanged(nameof(TOOLSID));
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



        public int toolsid;
        public string analog;
        public string description;
        public string function;
        public string note;
        public string language;

        public ToolsSubModel()
        {
        }
    }
}