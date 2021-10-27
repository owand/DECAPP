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
    public class CouplingList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<CouplingModel> collection;
        public ObservableCollection<CouplingModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CouplingModel> GetCollection(string FilterType, string FilterPipesOD, string SearchCriterion)
        {
            string searchCriterion = SearchCriterion?.ToLower() ?? "";

            List<CouplingModel> _collection = App.Database.Table<CouplingModel>().Select(a => a).Where(a =>
                (string.IsNullOrEmpty(FilterType) || a.TYPEID.ToString().Equals(FilterType)) &&
                (string.IsNullOrEmpty(FilterPipesOD) || a.PIPESOD.ToString().Equals(FilterPipesOD)) &&
                (a.PIPESOD.ToString().Contains(searchCriterion) ||
                a.COUPLINGOD.ToString().Contains(searchCriterion) ||
                a.COUPLINGLENGTH.ToString().Contains(searchCriterion) ||
                a.COUPLINGMASS.ToString().Contains(searchCriterion) ||
                (!string.IsNullOrEmpty(a.DESCRIPTION) && a.DESCRIPTION.ToLowerInvariant().Contains(searchCriterion)) ||
                (!string.IsNullOrEmpty(a.NOTE) && a.NOTE.ToLowerInvariant().Contains(searchCriterion)))).OrderBy(a => a.TYPEID).ToList();
            foreach (CouplingModel element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<CouplingModel>(_collection);
        }

        private CouplingModel selectItem = null;
        public CouplingModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.TYPEID).FirstOrDefault());
                IndexPipesList = TypePipesList.IndexOf(TypePipesList.Where(X => X.TYPEID == SelectItem?.PIPESTYPEID).FirstOrDefault());
                IndexPipesODList = PipesODList.IndexOf(PipesODList.Where(X => X.PIPESOD == SelectItem?.PIPESOD).FirstOrDefault());
            }
        }

        private CouplingModel newItem = null;
        public CouplingModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public CouplingModel PreSelectItem { get; set; }


        private List<CouplingTypeModel> typeList = App.Database.Table<CouplingTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<CouplingTypeModel> TypeList
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


        private List<PipesTypeModel> typePipesList = App.Database.Table<PipesTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<PipesTypeModel> TypePipesList
        {
            get => typePipesList;
            set
            {
                typePipesList = value;
                OnPropertyChanged(nameof(TypePipesList));
            }
        }
        private int indexPipesList;
        public int IndexPipesList
        {
            get => indexPipesList;
            set
            {
                indexPipesList = value;
                OnPropertyChanged(nameof(IndexPipesList));
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


        public CouplingList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<CouplingModel>().Any())
            {
                Collection?.Add(new CouplingModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new CouplingModel();
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
                    App.Database.Delete<CouplingModel>(SelectItem.COUPLINGID);
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



    //Таблица муфт
    [Table("tbCoupling")]
    public class CouplingModel : ViewModelBase
    {
        #region

        [Column("CouplingID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int COUPLINGID { get; set; }   // Уникальный код

        [Column("PipesTypeID"), NotNull, Indexed, ForeignKey(typeof(PipesTypeModel))]     // Specify the foreign key
        public int PIPESTYPEID   // Номенклатурная группа труб
        {
            get => pipestypeId;
            set
            {
                pipestypeId = value;
                OnPropertyChanged(nameof(PIPESTYPEID));
            }
        }

        [Column("CouplingTypeID"), NotNull, Indexed, ForeignKey(typeof(CouplingTypeModel))]     // Specify the foreign key
        public int TYPEID   // Условное обозначение муфты
        {
            get => typeId;
            set
            {
                typeId = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("PipesOD"), NotNull, Indexed, ForeignKey(typeof(PipesTypeModel))]     // Specify the foreign key
        public decimal PIPESOD   // Наружный диаметр труб, мм
        {
            get => pipesod;
            set
            {
                pipesod = value;
                OnPropertyChanged(nameof(PIPESOD));
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

        [Column("CouplingOD"), NotNull]
        public decimal COUPLINGOD   // Наружный диаметр муфты
        {
            get => couplingod;
            set
            {
                couplingod = value;
                OnPropertyChanged(nameof(COUPLINGOD));
            }
        }
        public string COUPLINGODFORMAT => string.Format("{0:N2}", couplingod); // Поле в американском формате

        [Column("CouplingLength"), NotNull]
        public decimal COUPLINGLENGTH   // Минимальная длина NL , мм
        {
            get => couplinglength;
            set
            {
                couplinglength = value;
                OnPropertyChanged(nameof(COUPLINGLENGTH));
            }
        }
        public string COUPLINGLENGTHFORMAT => string.Format("{0:N2}", couplinglength); // Поле в американском формате

        [Column("CouplingMass"), NotNull]
        public decimal COUPLINGMASS   // Масса, кг
        {
            get => couplingmass;
            set
            {
                couplingmass = value;
                OnPropertyChanged(nameof(COUPLINGMASS));
            }
        }
        public string COUPLINGMASSFORMAT => string.Format("{0:N2}", couplingmass); // Поле в американском формате

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
        public CouplingTypeModel CouplingTypes { get; set; }

        [ManyToOne]      // Many to one relationship with PipesTypes
        public PipesTypeModel PipesTypes { get; set; }

        #endregion



        #region
        public int couplingid;
        public int pipestypeId;
        public int typeId;
        public decimal pipesod;
        public string description;
        public decimal couplingod;
        public decimal couplinglength;
        public decimal couplingmass;
        public string note;
        #endregion

        public CouplingModel()
        {
        }
    }
}