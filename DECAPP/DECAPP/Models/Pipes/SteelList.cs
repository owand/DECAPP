using DECAPP.Resources;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Pipes
{
    public class SteelList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<SteelModel> collection;
        public ObservableCollection<SteelModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<SteelModel> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<SteelModel> _collection = App.Database.Table<SteelModel>().Select(a => a).Where(a =>
                a.STEELNAME.ToLowerInvariant().Contains(searchCriterion) ||
                (!string.IsNullOrEmpty(a.ANALOG) && a.ANALOG.ToLowerInvariant().Contains(searchCriterion)) ||
                (!string.IsNullOrEmpty(a.STEELCLASS) && a.STEELCLASS.ToLowerInvariant().Contains(searchCriterion)) ||
                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion))).OrderBy(a => a.STEELNAME).ToList();

            return new ObservableCollection<SteelModel>(_collection);
        }

        private SteelModel selectItem = null;
        public SteelModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private SteelModel newItem = null;
        public SteelModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public SteelModel PreSelectItem { get; set; }


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

        public SteelList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<SteelModel>().Any())
            {
                Collection?.Add(new SteelModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new SteelModel();
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
                }
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
                    App.Database.Delete<SteelModel>(SelectItem.STEELID);
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



    //Таблица видов стали
    [Table("tbSteel")]
    public class SteelModel : ViewModelBase
    {
        [Column("SteelID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int STEELID { get; set; }   // Уникальный код характеристики муфты

        [Column("SteelName"), NotNull, Unique, Indexed]
        public string STEELNAME   // Уникальный код характеристики стали
        {
            get => steelname;
            set
            {
                steelname = value;
                OnPropertyChanged(nameof(STEELNAME));
            }
        }

        [Column("Analog")]
        public string ANALOG   // Аналог группы прочности
        {
            get => analog;
            set
            {
                analog = value;
                OnPropertyChanged(nameof(ANALOG));
            }
        }

        [Column("Class")]
        public string STEELCLASS   // Класс стали
        {
            get => steelclass;
            set
            {
                steelclass = value;
                OnPropertyChanged(nameof(STEELCLASS));
            }
        }

        [Column("Tensile")]
        public decimal TENSILE   // Предел прочности, Мпа
        {
            get => tensile;
            set
            {
                tensile = value;
                OnPropertyChanged(nameof(TENSILE));
            }
        }
        public string TENSILEFORMAT => string.Format("{0:N2}", tensile); // Поле в американском формате

        [Column("MinYield")]
        public decimal MINYIELD   // Минимальный предел текучести, Мпа
        {
            get => minyield;
            set
            {
                minyield = value;
                OnPropertyChanged(nameof(MINYIELD));
            }
        }
        public string MINYIELDFORMAT => string.Format("{0:N2}", minyield); // Поле в американском формате

        [Column("MaxYield")]
        public decimal MAXYIELD   // Максимальный предел текучести, Мпа
        {
            get => maxyield;
            set
            {
                maxyield = value;
                OnPropertyChanged(nameof(MAXYIELD));
            }
        }
        public string MAXYIELDFORMAT => string.Format("{0:N2}", maxyield); // Поле в американском формате

        [Column("Elongation")]
        public decimal ELONGATION   // Относительное удлинение, %
        {
            get => elongation;
            set
            {
                elongation = value;
                OnPropertyChanged(nameof(ELONGATION));
            }
        }
        public string ELONGATIONFORMAT => string.Format("{0:N3}", elongation); // Поле в американском формате

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



        #region объявление переменных
        public string steelname;
        public string analog;
        public string steelclass;
        public decimal tensile;
        public decimal minyield;
        public decimal maxyield;
        public decimal elongation;
        public string note;
        #endregion

        public SteelModel()
        {
            //this.STEELNAME = String.Empty;
            //this.ANALOG = String.Empty;
            //this.STEELCLASS = String.Empty;
            //this.TENSILE = 0;
            //this.MINYIELD = 0;
            //this.MAXYIELD = 0;
            //this.ELONGATION = 0;
            //this.NOTE = String.Empty;
        }
    }
}