using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.BHA
{
    public class BitType : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        #region --------- Объединенная коллекция --------

        public ObservableCollection<BitTypeJoin> collection;
        public ObservableCollection<BitTypeJoin> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BitTypeJoin> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<BitTypeJoin> _collection = (from collection in App.Database.Table<BitTypeModel>().ToList()
                                             join subCollection in App.Database.Table<BitTypeSubModel>().Where(a => a.LANGUAGE == App.AppLanguage) on collection.TYPEID equals subCollection.TYPEID into joinCollection
                                             from subCollection in joinCollection.DefaultIfEmpty(new BitTypeSubModel() { })
                                             select new BitTypeJoin()
                                             {
                                                 ID = collection.TYPEID,
                                                 TYPENAME = collection.TYPENAME,
                                                 PICTURE = collection.PICTURE,
                                                 DESCRIPTION = subCollection.DESCRIPTION,
                                                 NOTE = subCollection.NOTE
                                             }).OrderBy(a => a.TYPENAME).Where(a => a.TYPENAME.ToLowerInvariant().Contains(searchCriterion) ||
                                             (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                                             (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).ToList();

            return new ObservableCollection<BitTypeJoin>(_collection);
        }

        private BitTypeJoin selectItem = null;
        public BitTypeJoin SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private BitTypeJoin newJoinItem = null;
        public BitTypeJoin NewJoinItem
        {
            get => newJoinItem;
            set
            {
                newJoinItem = value;
                OnPropertyChanged(nameof(NewJoinItem));
            }
        }

        public BitTypeJoin PreSelectItem { get; set; }

        #endregion ------------------------------------


        #region --------- Основная коллекция --------

        private BitTypeModel selectHostItem = null;
        public BitTypeModel SelectHostItem
        {
            get => selectHostItem;
            set
            {
                selectHostItem = value;
                OnPropertyChanged(nameof(SelectHostItem));
            }
        }

        private BitTypeModel newHostItem = null;
        public BitTypeModel NewHostItem
        {
            get => newHostItem;
            set
            {
                newHostItem = value;
                OnPropertyChanged(nameof(NewHostItem));
            }
        }
        public BitTypeModel GetSelectHostItem()
        {
            return App.Database.Table<BitTypeModel>().FirstOrDefault(a => a.TYPEID == SelectItem.ID);
        }

        #endregion ------------------------------------


        #region --------- Подчиненная коллекция --------

        private BitTypeSubModel selectSubItem = null;
        public BitTypeSubModel SelectSubItem
        {
            get => selectSubItem;
            set
            {
                selectSubItem = value;
                OnPropertyChanged(nameof(SelectSubItem));
            }
        }

        private BitTypeSubModel newSubItem = null;
        public BitTypeSubModel NewSubItem
        {
            get => newSubItem;
            set
            {
                newSubItem = value;
                OnPropertyChanged(nameof(NewSubItem));
            }
        }
        public BitTypeSubModel GetSelectSubItem()
        {
            return App.Database.Table<BitTypeSubModel>().FirstOrDefault(a => a.LANGUAGE == App.AppLanguage && a.TYPEID == SelectItem.ID);
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


        public BitType()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitTypeModel>().Any())
            {
                Collection?.Add(new BitTypeJoin { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewJoinItem = new BitTypeJoin();
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
                    App.Database.Delete<BitTypeModel>(SelectItem.ID);
                    App.Database.Delete<BitTypeSubModel>(SelectItem.ID);
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



    public class BitTypeJoin : ViewModelBase
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

        public BitTypeJoin()
        {
        }
    }




    //Таблица групп породоразрушающего инструмента
    [Table("tbBitType")]
    public class BitTypeModel : ViewModelBase
    {
        [Column("BitTypeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int TYPEID { get; set; }   // Уникальный код группы

        [Column("BitType"), NotNull, Unique, Indexed]
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

        public BitTypeModel()
        {
            //this.TYPENAME = null;
            //this.PICTURE = null;
        }
    }



    //Таблица групп породоразрушающего инструмента
    [Table("tbBitTypeML")]
    public class BitTypeSubModel : ViewModelBase
    {
        [Column("BitTypeMLID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITTYPEMLID { get; set; }   // Уникальный код

        [Column("BitTypeID"), NotNull, Indexed, ForeignKey(typeof(BitTypeModel))]     // Specify the foreign key
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

        public BitTypeSubModel()
        {
        }
    }
}