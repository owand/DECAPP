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
    public class BitCodeViewModel : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<BitCodeModel> collection;
        public ObservableCollection<BitCodeModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BitCodeModel> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<BitCodeModel> _collection = App.Database.Table<BitCodeModel>().Select(a => a).Where(a =>
            a.LANGUAGE.Equals(App.AppLanguage) &&
            (a.SYMBOL.ToLowerInvariant().Contains(searchCriterion) ||
            a.FEATURE.ToLowerInvariant().Contains(searchCriterion) ||
            a.SPECIFICATION.ToLowerInvariant().Contains(searchCriterion))).OrderBy(a => a.TYPEID).ToList();
            foreach (BitCodeModel element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<BitCodeModel>(_collection);
        }

        private BitCodeModel selectItem = null;
        public BitCodeModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.TYPEID).FirstOrDefault());
                IndexSerialList = SelectItem.SERIAL - 1;
            }
        }

        private BitCodeModel newItem = null;
        public BitCodeModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public BitCodeModel PreSelectItem { get; set; }

        private List<BitTypeModel> typeList = App.Database.Table<BitTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<BitTypeModel> TypeList
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

        public List<KeyValuePair<int, int>> SerialList => new Dictionary<int, int>() { { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 } }.ToList();
        private int indexSerialList;
        public int IndexSerialList
        {
            get => indexSerialList;
            set
            {
                indexSerialList = value;
                OnPropertyChanged(nameof(IndexSerialList));
            }
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


        public BitCodeViewModel()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitCodeModel>().Any())
            {
                Collection?.Add(new BitCodeModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new BitCodeModel();
                Collection.Add(NewItem);
                SelectItem = NewItem;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                if (NewItem != null)
                {
                    Collection.Remove(NewItem);
                    NewItem = null;
                }
                return;
            }
        }

        // Сохраняем или создаем и сохраняем новую запись.
        public void UpdateItem()
        {
            try
            {
                lock (collisionLock)
                {
                    if (NewItem != null)
                    {
                        App.Database.Insert(SelectItem);
                    }
                    else
                    {
                        App.Database.Update(SelectItem);
                    }
                    //App.Database.Close();
                }

                NewItem = null;

                DetailMode = true;
            }
            catch (SQLiteException ex)
            {
                Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
                return;
            }
        }

        // Удаляем текущую запись.
        public void DeleteItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Delete<BitCodeModel>(SelectItem.BITCODEID);
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



    //Таблица словаря кодов породоразрушающего инструмента
    [Table("tbBitCode")]
    public class BitCodeModel : ViewModelBase
    {
        [Column("BitCodeID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITCODEID { get; set; }

        [Column("BitTypeID"), NotNull, Indexed, ForeignKey(typeof(BitTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("Serial"), NotNull]
        public int SERIAL
        {
            get => serial;
            set
            {
                serial = value;
                OnPropertyChanged(nameof(SERIAL));
            }
        }

        [Column("Symbol"), NotNull]
        public string SYMBOL
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(SYMBOL));
            }
        }

        [Column("Feature"), NotNull]
        public string FEATURE
        {
            get => feature;
            set
            {
                feature = value;
                OnPropertyChanged(nameof(FEATURE));
            }
        }

        [Column("Specification"), NotNull]
        public string SPECIFICATION
        {
            get => specification;
            set
            {
                specification = value;
                OnPropertyChanged(nameof(SPECIFICATION));
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

        [ManyToOne]      // Many to one relationship with BitGroups
        public BitTypeModel BitTypes { get; set; }



        public int typeId;
        public int serial;
        public string symbol;
        public string feature;
        public string specification;
        public string language;

        public BitCodeModel()
        {
        }
    }

}