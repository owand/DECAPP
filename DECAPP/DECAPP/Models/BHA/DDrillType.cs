using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.BHA
{
    public class DDrillType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<DDrillTypeJoin> collection;
        public ObservableCollection<DDrillTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DDrillTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<DDrillTypeJoin> _collection = (from collection in App.Database.Table<DDrillTypeModel>().ToList()
                                                join subCollection in App.Database.Table<DDrillTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                                from subCollection in joinCollection.DefaultIfEmpty(new DDrillTypeSubModel() { })
                                                select new DDrillTypeJoin()
                                                {
                                                    ID = collection.TYPEID,
                                                    TYPENAME = collection.TYPENAME,
                                                    PICTURE = collection.PICTURE,
                                                    DESCRIPTION = subCollection.DESCRIPTION,
                                                    NOTE = subCollection.NOTE
                                                }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<DDrillTypeJoin>(_collection);
        }

        private DDrillTypeJoin selectItem = null;
        public DDrillTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private DDrillTypeJoin newJoinItem = null;
        public DDrillTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public DDrillTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private DDrillTypeModel selectHostItem = null;
        public DDrillTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public DDrillTypeModel GetSelectHostItem()
        {
            return App.Database.Table<DDrillTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private DDrillTypeModel newHostItem = null;
        public DDrillTypeModel NewHostItem
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

        private DDrillTypeSubModel selectSubItem = null;
        public DDrillTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private DDrillTypeSubModel newSubItem = null;
        public DDrillTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public DDrillTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<DDrillTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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


        public DDrillType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<DDrillTypeModel>().Any())
            {
                Collection?.Add(new DDrillTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new DDrillTypeJoin();
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
                    App.Database.Delete<DDrillTypeModel>(SelectItem.ID);
                    App.Database.Delete<DDrillTypeSubModel>(SelectItem.ID);
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



    public class DDrillTypeJoin : ViewModelBase
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

        public DDrillTypeJoin()
        {
        }
    }


    //Таблица групп услуг направленного бурения
    [Table("tbDDrillType")]
    public class DDrillTypeModel : ViewModelBase
    {
        [Column("DDrillTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("DDrillType"), NotNull, Unique, Indexed]
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

        public DDrillTypeModel()
        {
        }
    }



    //Таблица групп услуг направленного бурения
    [Table("tbDDrillTypeML")]
    public class DDrillTypeSubModel : ViewModelBase
    {
        [Column("DDrillTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int DDRILLTYPEMLID { get; set; }   // Уникальный код

        [Column("DDrillTypeID"), NotNull, Indexed, ForeignKey(typeof(DDrillTypeModel))]     // Specify the foreign key
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

        public DDrillTypeSubModel()
        {
        }
    }
}