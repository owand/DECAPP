using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Pipes
{
    public class CouplingType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<CouplingTypeJoin> collection;
        public ObservableCollection<CouplingTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CouplingTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<CouplingTypeJoin> _collection = (from collection in App.Database.Table<CouplingTypeModel>().ToList()
                                                  join subCollection in App.Database.Table<CouplingTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                                  from subCollection in joinCollection.DefaultIfEmpty(new CouplingTypeSubModel() { })
                                                  select new CouplingTypeJoin()
                                                  {
                                                      ID = collection.TYPEID,
                                                      TYPENAME = collection.TYPENAME,
                                                      PICTURE = collection.PICTURE,
                                                      DESCRIPTION = subCollection.DESCRIPTION,
                                                      NOTE = subCollection.NOTE
                                                  }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                                  (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                                  (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<CouplingTypeJoin>(_collection);
        }

        private CouplingTypeJoin selectItem = null;
        public CouplingTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private CouplingTypeJoin newJoinItem = null;
        public CouplingTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public CouplingTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private CouplingTypeModel selectHostItem = null;
        public CouplingTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public CouplingTypeModel GetSelectHostItem()
        {
            return App.Database.Table<CouplingTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private CouplingTypeModel newHostItem = null;
        public CouplingTypeModel NewHostItem
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

        private CouplingTypeSubModel selectSubItem = null;
        public CouplingTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private CouplingTypeSubModel newSubItem = null;
        public CouplingTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public CouplingTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<CouplingTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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


        public CouplingType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<CouplingTypeModel>().Any())
            {
                Collection?.Add(new CouplingTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new CouplingTypeJoin();
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
                    App.Database.Delete<CouplingTypeModel>(SelectItem.ID);
                    App.Database.Delete<CouplingTypeSubModel>(SelectItem.ID);
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



    public class CouplingTypeJoin : ViewModelBase
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

        public CouplingTypeJoin()
        {
        }
    }



    //Таблица типов муфт
    [Table("tbCouplingType")]
    public class CouplingTypeModel : ViewModelBase
    {
        [Column("CouplingTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код номенклатурной группы

        [Column("CouplingType"), NotNull, Unique, Indexed]
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
        public byte[] PICTURE   // Илюстрация
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

        public CouplingTypeModel()
        {
            //this.TYPENAME = null;
            //this.PICTURE = null;
        }
    }



    //Таблица групп породоразрушающего инструмента
    [Table("tbCouplingTypeML")]
    public class CouplingTypeSubModel : ViewModelBase
    {
        [Column("CouplingTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int COUPLINGTYPESUBID { get; set; }   // Уникальный код

        [Column("CouplingTypeID"), NotNull, Indexed, ForeignKey(typeof(CouplingTypeModel))]     // Specify the foreign key
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
        public string DESCRIPTION
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

        public CouplingTypeSubModel()
        {
        }
    }
}