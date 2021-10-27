using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Pipes
{
    public class PipesList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<PipesModel> collection;
        public ObservableCollection<PipesModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PipesModel> GetCollection(string FilterType, string FilterPipesOD, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<PipesModel> _collection = App.Database.Table<PipesModel>().Select(a => a).Where(a =>
                (string.IsNullOrEmpty(FilterType) || a.TYPEID.ToString().Equals(FilterType)) &&
                (string.IsNullOrEmpty(FilterPipesOD) || a.PIPESOD.ToString().Equals(FilterPipesOD)) &&
                (a.PIPESOD.ToString().Contains(searchCriterion) ||
                a.PIPESODINCH.Contains(searchCriterion) ||
                a.PIPESWALL.ToString().Contains(searchCriterion) ||
                a.PIPESIND.ToString().Contains(searchCriterion) ||
                a.PIPESMASS.ToString().Contains(searchCriterion) ||
                (!string.IsNullOrEmpty(a.PIPESGOST) && a.PIPESGOST.ToLowerInvariant().Contains(searchCriterion)) ||
                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).OrderBy(a => a.TYPEID).ToList();
            foreach (PipesModel element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<PipesModel>(_collection);
        }

        private PipesModel selectItem = null;
        public PipesModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.TYPEID).FirstOrDefault());
            }
        }

        private PipesModel newItem = null;
        public PipesModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public PipesModel PreSelectItem { get; set; }

        private List<PipesTypeModel> typeList = App.Database.Table<PipesTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<PipesTypeModel> TypeList
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


        private List<PipesModel> pipesODList = App.Database.Table<PipesModel>().GroupBy(x => x.PIPESOD).Select(x => x.First()).OrderBy(a => a.PIPESOD).ToList();
        public List<PipesModel> PipesODList
        {
            get => pipesODList;
            set
            {
                pipesODList = value;
                OnPropertyChanged(nameof(PipesODList));
            }
        }
        private int indexPipesODList;
        public int IndexPipesODList
        {
            get => indexPipesODList;
            set
            {
                indexPipesODList = value;
                OnPropertyChanged(nameof(IndexPipesODList));
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


        public PipesList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<PipesModel>().Any())
            {
                Collection?.Add(new PipesModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new PipesModel();
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
                    App.Database.Delete<PipesModel>(SelectItem.PIPESID);
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



    //Таблица сортамента труб
    [Table("tbPipes")]
    public class PipesModel : ViewModelBase
    {
        #region

        [Column("PipesID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int PIPESID { get; set; }   // Уникальный код строки в таблице параметров труб

        [Column("PipesTypeID"), NotNull, Indexed, ForeignKey(typeof(PipesTypeModel))]     // Specify the foreign key
        public int TYPEID   // Номенклатурная группа
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("PipesOD"), NotNull]
        public decimal PIPESOD   // Наружный диаметр труб, мм
        {
            get => pipesod;
            set
            {
                pipesod = value;
                OnPropertyChanged(nameof(PIPESOD));
            }
        }
        public string PIPESODFORMAT => string.Format("{0:N2}", pipesod); // Поле в американском формате
        public string PIPESNAME => string.Format("{0:F2}", pipesod) + " x " + string.Format("{0:F2}", pipeswall); // Склеиваем Наружный диаметр труб и Толщину стенки трубы
        public string PIPESODNAME => string.Format("{0:F2}", pipesod) + "  /  " + pipesodinch; //Переменная для использования в 1-м варианте группировки, который выводит только перый символ группы

        [Column("PipesODinch"), NotNull]
        public string PIPESODINCH   // Наружный диаметр труб, дюйм
        {
            get => pipesodinch;
            set
            {
                pipesodinch = value;
                OnPropertyChanged(nameof(PIPESODINCH));
            }
        }

        [Column("PipesWall"), NotNull]
        public decimal PIPESWALL   // Толщина стенки трубы
        {
            get => pipeswall;
            set
            {
                pipeswall = value;
                OnPropertyChanged(nameof(PIPESWALL));
            }
        }
        public string PIPESWALLFORMAT => string.Format("{0:N2}", pipeswall); // Поле в американском формате

        [Column("PipesIND"), NotNull]
        public decimal PIPESIND   // Внутренний диаметр, мм
        {
            get => pipesind;
            set
            {
                pipesind = value;
                OnPropertyChanged(nameof(PIPESIND));
            }
        }
        public string PIPESINDFORMAT => string.Format("{0:N2}", pipesind); // Поле в американском формате

        [Column("PipesMass"), NotNull]
        public decimal PIPESMASS   // Удельный вес одного метра трубы (без муфт)
        {
            get => pipesmass;
            set
            {
                pipesmass = value;
                OnPropertyChanged(nameof(PIPESMASS));
            }
        }
        public string PIPESMASSFORMAT => string.Format("{0:N2}", pipesmass); // Поле в американском формате

        [Column("PipesGOST")]
        public string PIPESGOST   // ТУ, ГОСТ
        {
            get => pipesgost;
            set
            {
                pipesgost = value;
                OnPropertyChanged(nameof(PIPESGOST));
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



        [ManyToOne]      // Many to one relationship with PipesTypes
        public PipesTypeModel PipesTypes { get; set; }

        #endregion



        #region
        public int typeId;
        public decimal pipesod;
        public string pipesodinch;
        public decimal pipeswall;
        public decimal pipesind;
        public decimal pipesmass;
        public string pipesgost;
        public string note;
        #endregion

        public PipesModel()
        {
        }
    }
}