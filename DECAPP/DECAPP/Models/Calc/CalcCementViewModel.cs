using DECAPP.Models.BHA;
using DECAPP.Models.Mud;
using DECAPP.Models.Pipes;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DECAPP.Models.Calc
{
    public class CalcCementViewModel : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков
        public ObservableCollection<CalcCementModel> CalcCement { get; set; }
        public CalcCementModel CalcCementItem { get; set; }


        public List<BitODModel> bitCollection = App.Database.Table<BitODModel>().OrderBy(a => a.BITOD).ToList();
        public List<BitODModel> BitCollection
        {
            get => bitCollection;
            set
            {
                bitCollection = value;
                OnPropertyChanged();
            }
        }
        public int GetDwellIndex()
        {
            return BitCollection.IndexOf(BitCollection.FirstOrDefault(X => X.BITOD == CalcCementItem.DWELL));
        }


        public List<PipesTypeModel> pipesCollection = App.Database.Table<PipesTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<PipesTypeModel> PipesCollection
        {
            get => pipesCollection;
            set
            {
                pipesCollection = value;
                OnPropertyChanged();
            }
        }
        public int GetPipeTypeIndex()
        {
            return PipesCollection.IndexOf(PipesCollection.FirstOrDefault(X => X.TYPEID == CalcCementItem.PIPESTYPEID));
        }
        public int GetPrevPipeTypeIndex()
        {
            return PipesCollection.IndexOf(PipesCollection.FirstOrDefault(X => X.TYPEID == CalcCementItem.PIPEPREVTYPEID));
        }


        public ObservableCollection<PipesModel> pipesODList;
        public ObservableCollection<PipesModel> PipesODList
        {
            get => pipesODList;
            set
            {
                pipesODList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PipesModel> pipesPrevODList;
        public ObservableCollection<PipesModel> PipesPrevODList
        {
            get => pipesPrevODList;
            set
            {
                pipesPrevODList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PipesModel> GetPipesODList(string FilterType)
        {
            List<PipesModel> _collection = App.Database.Table<PipesModel>().Select(a => a).
                                           Where(a => string.IsNullOrEmpty(FilterType) || a.TYPEID.ToString().Equals(FilterType)).
                                           GroupBy(a => a.PIPESOD).Select(a => a.First()).OrderBy(a => a.PIPESOD).ToList();

            return new ObservableCollection<PipesModel>(_collection);
        }


        public ObservableCollection<PipesModel> pipesTWList;
        public ObservableCollection<PipesModel> PipesTWList
        {
            get => pipesTWList;
            set
            {
                pipesTWList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PipesModel> pipesPrevTWList;
        public ObservableCollection<PipesModel> PipesPrevTWList
        {
            get => pipesPrevTWList;
            set
            {
                pipesPrevTWList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PipesModel> GetPipesTWList(string FilterType, string FilterPipesOD)
        {
            List<PipesModel> _collection = App.Database.Table<PipesModel>().Select(a => a).
                        Where(a => a.TYPEID.ToString().Equals(FilterType) && a.PIPESOD.ToString().Equals(FilterPipesOD)).
                        OrderBy(a => a.PIPESWALL).ToList();

            return new ObservableCollection<PipesModel>(_collection);
        }



        public CalcCementViewModel()
        {
            CalcCement = new ObservableCollection<CalcCementModel>(App.Database.Table<CalcCementModel>());
            // If the table is empty, initialize the collection
            if (!App.Database.Table<CalcCementModel>().Any())
            {
                AddFirstItem();
            }

            CalcCementItem = App.Database.Table<CalcCementModel>().First();
        }

        // Создаем новую пустую запись в основной коллекции
        public CalcCementModel AddFirstItem()
        {
            CalcCementModel NewItem = new CalcCementModel
            {
                CCS = 0,
                CCAV = 0,
                DWELL = 0,
                PIPESTYPEID = 1,  // Нужно разобраться со значением по умолчанию в ComboBox-е  !!!!!!!!!!!!!!!!!!!!
                ODCAS = 0,
                TCAS = 0,
                LCAS = 0,
                LHCP = 0,
                PIPEPREVTYPEID = 1,  // Нужно разобраться со значением по умолчанию в ComboBox-е  !!!!!!!!!!!!!!!!!!!!
                ODPRCAS = 0,
                TPRCAS = 0,
                LPRCAS = 0,
                LPRHCP = 0,
                CCOMPRES = 0,
                VPIPELINE = 0,
                DENSITYSLURRY = 0,
                DENSITYFLUID = 0,
                MUDID = 0,
                CLOSSCEMENT = 0,
                CLOSSWATER = 0
            };

            CalcCement.Add(NewItem);
            return NewItem;
        }

        // Сохраняем или создаем и сохраняем новую запись.
        public void UpdateItem()
        {
            try
            {
                lock (collisionLock)
                {
                    App.Database.Update(CalcCementItem);
                }
                //App.Database.Close();
            }
            catch (SQLiteException)
            {
            }
        }

    }



    //Таблица параметров формул для расчета объемов и массы тампонажных материалов
    [Table("tbCalcCement")]
    public class CalcCementModel : ViewModelBase
    {
        #region

        [Column("CalcCementID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int ID { get; set; }   // Уникальный код

        [Column("Ccs")]
        public decimal CCS
        {
            get => ccs;
            set
            {
                ccs = value;
                OnPropertyChanged(nameof(CCS));
            }
        }

        [Column("Ccav")]
        public decimal CCAV
        {
            get => ccav;
            set
            {
                ccav = value;
                OnPropertyChanged(nameof(CCAV));
            }
        }

        [Column("Dwell")]
        public decimal DWELL { get; set; }

        [Column("PipesTypeID"), Indexed, ForeignKey(typeof(PipesTypeModel))]     // Specify the foreign key
        public int PIPESTYPEID { get; set; }   // Номенклатурная группа

        [Column("ODcas")]
        public decimal ODCAS { get; set; }

        [Column("Tcas")]
        public decimal TCAS { get; set; }

        [Column("Lcas")]
        public decimal LCAS { get; set; }

        [Column("Lhcp")]
        public decimal LHCP { get; set; }

        [Column("PipePrevTypeID"), Indexed, ForeignKey(typeof(PipesTypeModel))]     // Specify the foreign key
        public int PIPEPREVTYPEID { get; set; }   // Номенклатурная группа

        [Column("ODprcas")]
        public decimal ODPRCAS { get; set; }

        [Column("Tprcas")]
        public decimal TPRCAS { get; set; }

        [Column("Lprcas")]
        public decimal LPRCAS { get; set; }

        [Column("Lprhcp")]
        public decimal LPRHCP { get; set; }

        [Column("Ccompres")]
        public decimal CCOMPRES { get; set; }

        [Column("Vpipeline")]
        public decimal VPIPELINE { get; set; }

        [Column("DensitySlurry")]   // Плотность тампонажного раствора, г/см3 / The density of cement slurry, g / cm3
        public decimal DENSITYSLURRY { get; set; }

        [Column("DensityFluid")]   // Плотность жидкости затворения, г/см3 / Density of mixing fluid, g / cm3
        public decimal DENSITYFLUID { get; set; }

        [Column("MudID"), Indexed, ForeignKey(typeof(MudModel))]     // Specify the foreign key
        public int MUDID { get; set; }   // Номенклатурная группа

        [Column("CLossCement")]   // Коэффициент потерь сухого цемента / Dry cement loss ratio
        public decimal CLOSSCEMENT { get; set; }

        [Column("CLossWater")]   // Коэффициент потери воды / Water loss coefficient
        public decimal CLOSSWATER { get; set; }



        public decimal ccs;
        public decimal ccav;


        #endregion

        public CalcCementModel()
        {
        }
    }

}