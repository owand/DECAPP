using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Rock
{
    public class RockList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<RockJoin> collection;
        public ObservableCollection<RockJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<RockJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<RockJoin> _collection = (from collection in App.Database.Table<RockModel>().ToList()
                                          join subCollection in App.Database.Table<RockSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.ROCKID equals subCollection.ROCKID into joinCollection
                                          from subCollection in joinCollection.DefaultIfEmpty(new RockSubModel() { })
                                          select new RockJoin()
                                          {
                                              ID = collection.ROCKID,
                                              ROCKNAME = collection.ROCKNAME,
                                              ROCKTYPENAME = subCollection.ROCKTYPENAME,
                                              DESCRIPTION = subCollection.DESCRIPTION,
                                              NOTE = subCollection.NOTE
                                          }).Where(a => a.ROCKNAME.ToLowerInvariant().Contains(searchCriterion) ||
             (!string.IsNullOrEmpty(a.ROCKTYPENAME) && a.ROCKTYPENAME.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
             (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).OrderBy(a => a.ROCKNAME).ToList();

            return new ObservableCollection<RockJoin>(_collection);
        }

        private RockJoin selectItem = null;
        public RockJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private RockJoin newJoinItem = null;
        public RockJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public RockJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private RockModel selectHostItem = null;
        public RockModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }
        public RockModel GetSelectHostItem()
        {
            return App.Database.Table<RockModel>().FirstOrDefault(a => a.ROCKID == SelectItem.ID);
        }

        private RockModel newHostItem = null;
        public RockModel NewHostItem
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

        private RockSubModel selectSubItem = null;
        public RockSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private RockSubModel newSubItem = null;
        public RockSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public RockSubModel GetSelectSubItem()
        {
            return App.Database.Table<RockSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.ROCKID == SelectItem.ID);
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

        public RockList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<RockModel>().Any())
            {
                Collection?.Add(new RockJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new RockJoin();
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
                    App.Database.Delete<RockModel>(SelectItem.ID);
                    App.Database.Delete<RockSubModel>(SelectItem.ID);
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



    public class RockJoin : ViewModelBase
    {
        // Catalog
        public int ID { get; set; }   // Уникальный код группы

        public string ROCKNAME   // Название группы
        {
            get => rockname;
            set
            {
                rockname = value;
                OnPropertyChanged(nameof(ROCKNAME));
            }
        }

        // Sub Catalog
        public string ROCKTYPENAME   // Название группы
        {
            get => rocktypename;
            set
            {
                rocktypename = value;
                OnPropertyChanged(nameof(ROCKTYPENAME));
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
        public string rockname;

        // Sub Catalog
        public string rocktypename;
        public string description;
        public string note;

        public RockJoin()
        {
        }
    }



    [Table("tbRock")]
    public class RockModel : ViewModelBase
    {
        [Column("RockID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ROCKID { get; set; }   // Уникальный код группы

        [Column("RockName"), NotNull, Indexed]
        public string ROCKNAME   // Название группы
        {
            get => rockname.ToUpper();
            set
            {
                rockname = value.ToUpper();
                OnPropertyChanged(nameof(ROCKNAME));
            }
        }


        public int rockid;
        public string rockname;

        public RockModel()
        {
        }
    }



    [Table("tbRockML")]
    public class RockSubModel : ViewModelBase
    {
        [Column("RockMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ROCKSUBID { get; set; }   // Уникальный код

        [Column("RockID"), NotNull, Indexed, ForeignKey(typeof(RockTypeModel))]     // Specify the foreign key
        public int ROCKID   // Уникальный код группы
        {
            get => rockid;
            set
            {
                rockid = value;
                OnPropertyChanged(nameof(ROCKID));
            }
        }

        [Column("RockType"), NotNull, Indexed]
        public string ROCKTYPENAME   // Название группы
        {
            get => rocktypename;
            set
            {
                rocktypename = value;
                OnPropertyChanged(nameof(ROCKTYPENAME));
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


        public int rockid;
        public string rocktypename;
        public string description;
        public string note;
        public string language;

        public RockSubModel()
        {
        }
    }
}
