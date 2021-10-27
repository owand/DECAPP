using DECAPP.Resources;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.BHA
{
    public class BitODViewModel : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<BitODModel> collection;
        public ObservableCollection<BitODModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BitODModel> GetCollection(string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<BitODModel> _collection = new List<BitODModel>(App.Database.Table<BitODModel>().Select(a => a).Where(a =>
                         (!string.IsNullOrEmpty(a.BITODNAME) && a.BITODNAME.ToLowerInvariant().Contains(searchCriterion)) ||
                         (!string.IsNullOrEmpty(a.BITODINCH) && a.BITODINCH.ToLowerInvariant().Contains(searchCriterion)) ||
                         (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion))).OrderBy(a => a.BITOD).ToList());

            return new ObservableCollection<BitODModel>(_collection);
        }

        private BitODModel selectItem = null;
        public BitODModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
            }
        }

        private BitODModel newItem = null;
        public BitODModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public BitODModel PreSelectItem { get; set; }

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

        public BitODViewModel()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<BitODModel>().Any())
            {
                Collection?.Add(new BitODModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new BitODModel();
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
                    App.Database.Delete<BitODModel>(SelectItem.BITODID);
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


    //Таблица диаметров породоразрушающего инструмента
    [Table("tbBitOD")]
    public class BitODModel : ViewModelBase
    {
        [Column("BitODID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int BITODID { get; set; }   // Уникальный код группы

        [Column("BitOD"), NotNull, Unique, Indexed]
        public decimal BITOD   // Название номенклатурной группы
        {
            get => bitod;
            set
            {
                bitod = value;
                OnPropertyChanged(nameof(BITOD));
            }
        }

        [Column("BitODinch"), NotNull]
        public string BITODINCH   // Наружный диаметр труб, дюйм
        {
            get => bitodinch;
            set
            {
                bitodinch = value;
                OnPropertyChanged(nameof(BITODINCH));
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

        //public string BITODNAME => bitod.ToString("N2"); // Поле в американском формате
        public string BITODNAME => string.Format("{0:N2}", bitod); // Поле в американском формате

        public decimal bitod;
        public string bitodinch;
        public string description;

        public BitODModel()
        {
        }
    }

}