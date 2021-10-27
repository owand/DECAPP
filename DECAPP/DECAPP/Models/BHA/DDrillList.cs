using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.BHA
{
    public class DDrillList : ViewModelBase
    {

        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<DDrillJoin> collection;
        public ObservableCollection<DDrillJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DDrillJoin> GetCollection(string FilterCriterion, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<DDrillJoin> _collection = (from collection in App.Database.Table<DDrillModel>().ToList()
                                            join subCollection in App.Database.Table<DDrillSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.DDID equals subCollection.DDID into joinCollection
                                            from subCollection in joinCollection.DefaultIfEmpty(new DDrillSubModel() { })
                                            select new DDrillJoin()
                                            {
                                                ID = collection.DDID,
                                                TYPEID = collection.TYPEID,
                                                DDNAME = collection.DDNAME,
                                                PICTURE = collection.PICTURE,
                                                DESCRIPTION = subCollection.DESCRIPTION,
                                                NOTE = subCollection.NOTE
                                            }).OrderBy(a => a.DDNAME).Where(a =>
                                            (string.IsNullOrEmpty(FilterCriterion) || a.TYPEID.ToString().Equals(FilterCriterion)) &&
                   (a.DDNAME.ToLowerInvariant().Contains(searchCriterion) ||
                   (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                   (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).ToList();

            foreach (DDrillJoin element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<DDrillJoin>(_collection);
        }

        private DDrillJoin selectItem = null;
        public DDrillJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexTypeList();
            }
        }

        private DDrillJoin newJoinItem = null;
        public DDrillJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public DDrillJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private DDrillModel selectHostItem = null;
        public DDrillModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public DDrillModel GetSelectHostItem()
        {
            return App.Database.Table<DDrillModel>().FirstOrDefault(a => a.DDID == SelectItem.ID);
        }

        private DDrillModel newHostItem = null;
        public DDrillModel NewHostItem
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

        private DDrillSubModel selectSubItem = null;
        public DDrillSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private DDrillSubModel newSubItem = null;
        public DDrillSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public DDrillSubModel GetSelectSubItem()
        {
            return App.Database.Table<DDrillSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.DDID == SelectItem.ID);
        }

        #endregion ------------------------------------


        private List<DDrillTypeModel> typeList = App.Database.Table<DDrillTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<DDrillTypeModel> TypeList
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

        public DDrillList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<DDrillModel>().Any())
            {
                Collection?.Add(new DDrillJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new DDrillJoin();
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
                    App.Database.Delete<DDrillModel>(SelectItem.ID);
                    App.Database.Delete<DDrillSubModel>(SelectItem.ID);
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



    public class DDrillJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код

        [Column("DDrillTypeID"), NotNull, Indexed, ForeignKey(typeof(DDrillTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        public string DDNAME
        {
            get => ddname;
            set
            {
                ddname = value;
                OnPropertyChanged(nameof(DDNAME));
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
        public DDrillTypeModel DDrillTypes { get; set; }



        // Catalog
        public int typeId;
        public string ddname;
        public byte[] picture;

        // Sub Catalog
        public string description;
        public string note;
        public string language;

        public DDrillJoin()
        {
        }
    }



    //Таблица услуг направленного бурения
    [Table("tbDDrill")]
    public class DDrillModel : ViewModelBase
    {
        [Column("DDrillID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int DDID { get; set; }   // Уникальный код

        [Column("DDrillTypeID"), NotNull, Indexed, ForeignKey(typeof(DDrillTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("DDrillName"), NotNull, Unique, Indexed]
        public string DDNAME
        {
            get => ddname.ToUpper();
            set
            {
                ddname = value.ToUpper();
                OnPropertyChanged(nameof(DDNAME));
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
        public string ddname;
        public byte[] picture;

        public DDrillModel()
        {
        }
    }



    //Таблица услуг направленного бурения
    [Table("tbDDrillML")]
    public class DDrillSubModel : ViewModelBase
    {
        [Column("DDrillMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int DDSUBID { get; set; }   // Уникальный код

        [Column("DDrillID"), NotNull, Indexed, ForeignKey(typeof(DDrillModel))]     // Specify the foreign key
        public int DDID
        {
            get => ddid;
            set
            {
                ddid = value;
                OnPropertyChanged(nameof(DDID));
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



        public int ddid;
        public string description;
        public string note;
        public string language;

        public DDrillSubModel()
        {
        }
    }
}