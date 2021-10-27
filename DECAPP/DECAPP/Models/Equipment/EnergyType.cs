using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class EnergyType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        private ObservableCollection<EnergyTypeJoin> collection;
        public ObservableCollection<EnergyTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(EnergyTypeJoin));
            }
        }
        public ObservableCollection<EnergyTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<EnergyTypeJoin> _collection = (from collection in App.Database.Table<EnergyTypeModel>().ToList()
                                                join subCollection in App.Database.Table<EnergyTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage).ToList() on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                                from subCollection in joinCollection.DefaultIfEmpty(new EnergyTypeSubModel() { })
                                                select new EnergyTypeJoin()
                                                {
                                                    ID = collection.TYPEID,
                                                    TYPENAME = collection.TYPENAME,
                                                    DESCRIPTION = subCollection.DESCRIPTION,
                                                    NOTE = subCollection.NOTE
                                                }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<EnergyTypeJoin>(_collection);
        }

        private EnergyTypeJoin selectItem = null;
        public EnergyTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private EnergyTypeJoin newJoinItem = null;
        public EnergyTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }
        public EnergyTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private EnergyTypeModel selectHostItem = null;
        public EnergyTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public EnergyTypeModel GetSelectHostItem()
        {
            return App.Database.Table<EnergyTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        private EnergyTypeModel newHostItem = null;
        public EnergyTypeModel NewHostItem
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

        private EnergyTypeSubModel selectSubItem = null;
        public EnergyTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private EnergyTypeSubModel newSubItem = null;
        public EnergyTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public EnergyTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<EnergyTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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

        public EnergyType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<EnergyTypeModel>().ToList().Any())
            {
                Collection?.Add(new EnergyTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new EnergyTypeJoin();
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
                    App.Database.Delete<EnergyTypeModel>(SelectItem.ID);
                    App.Database.Delete<EnergyTypeSubModel>(SelectItem.ID);
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
    public class EnergyTypeJoin : ViewModelBase
    {
        public int ID { get; set; }   // Уникальный код группы
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename?.ToUpper();
            set
            {
                typename = value?.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
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

        public string typename;
        public string description;
        public string note;
    }

    [Table("tbEnergyType")]
    public class EnergyTypeModel : ViewModelBase
    {
        [Column("EnergyTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("EnergyType"), NotNull, Unique, Indexed]
        public string TYPENAME   // Название номенклатурной группы
        {
            get => typename.ToUpper();
            set
            {
                typename = value.ToUpper();
                OnPropertyChanged(nameof(TYPENAME));
            }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<EnergyModel> EnergyModel { get; set; }

        //[OneToMany(CascadeOperations = CascadeOperation.All)]
        //public List<EnergyTypeSubModel> SubModel { get; set; }

        public string typename;
        //public List<EnergyModel> energy;

        public EnergyTypeModel()
        {
        }
    }


    [Table("tbEnergyTypeML")]
    public class EnergyTypeSubModel : ViewModelBase
    {
        [Column("EnergyTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ENERGYTYPESUBID { get; set; }   // Уникальный код

        [Column("EnergyTypeID"), NotNull, Indexed, ForeignKey(typeof(EnergyTypeModel))]
        public int TYPEID { get; set; }  // внешний ключ

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

        public string description;
        public string note;
        public string language;

        public EnergyTypeSubModel()
        {
        }
    }

}
