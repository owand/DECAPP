using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class FuelDen : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        private ObservableCollection<FuelDenModel> collection;
        public ObservableCollection<FuelDenModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(Collection));
            }
        }
        public ObservableCollection<FuelDenModel> GetCollection(string Energy, string BaseTemp, string TableSymbol, string Temp, string BaseDensity)
        {
            return new ObservableCollection<FuelDenModel>(App.Database.Table<FuelDenModel>().Select(a => a).Where(a =>
                        (string.IsNullOrEmpty(Energy) || a.ENERGYID.ToString().Equals(Energy)) &&
                        (string.IsNullOrEmpty(BaseTemp) || a.BASETEMP.ToString().Equals(BaseTemp)) &&
                        (string.IsNullOrEmpty(TableSymbol) || a.TABLE.ToString().Equals(TableSymbol)) &&
                        (string.IsNullOrEmpty(Temp) || a.TEMP.ToString().Equals(Temp)) &&
                        (string.IsNullOrEmpty(BaseDensity) || a.BASEDENSITY.ToString().Equals(BaseDensity))).ToList());
        }

        private FuelDenModel selectItem = null;
        public FuelDenModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                GetIndexEnergyList();
            }
        }

        private FuelDenModel newItem = null;
        public FuelDenModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }

        public FuelDenModel PreSelectItem { get; set; }


        #region --------- Filters --------

        private List<EnergyModel> energyList = App.Database.Table<EnergyModel>().OrderBy(a => a.ENERGYNAME).ToList();
        public List<EnergyModel> EnergyList
        {
            get => energyList;
            set
            {
                energyList = value;
                OnPropertyChanged(nameof(EnergyList));
            }
        }

        private int indexEnergyList;
        public int IndexEnergyList
        {
            get => indexEnergyList;
            set
            {
                indexEnergyList = value;
                OnPropertyChanged(nameof(IndexEnergyList));
            }
        }
        public int GetIndexEnergyList()
        {
            IndexEnergyList = EnergyList.IndexOf(EnergyList.Where(X => X.ENERGYID == SelectItem?.ENERGYID).FirstOrDefault());
            return IndexEnergyList;
        }

        private IEnumerable<double> baseTempList = null;
        public IEnumerable<double> BaseTempList
        {
            get => baseTempList;
            set
            {
                baseTempList = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<double> GetBaseTempList()
        {
            return App.Database.Table<FuelDenModel>().Select(x => x.BASETEMP).Distinct().ToList();
        }

        private IEnumerable<string> tableList = null;
        public IEnumerable<string> TableList
        {
            get => tableList;
            set
            {
                tableList = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<string> GetTableList()
        {
            return App.Database.Table<FuelDenModel>().Select(x => x.TABLE).Distinct().ToList();
        }

        private IEnumerable<double> tempList = null;
        public IEnumerable<double> TempList
        {
            get => tempList;
            set
            {
                tempList = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<double> GetTempList()
        {
            return App.Database.Table<FuelDenModel>().Select(x => x.TEMP).Distinct().ToList();
        }

        private IEnumerable<double> baseDensityList = null;
        public IEnumerable<double> BaseDensityList
        {
            get => baseDensityList;
            set
            {
                baseDensityList = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<double> GetBaseDensityList()
        {
            return App.Database.Table<FuelDenModel>().Select(x => x.BASEDENSITY).Distinct().ToList();
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

        public FuelDen()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<FuelDenModel>().ToList().Any())
            {
                Collection?.Add(new FuelDenModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new FuelDenModel();
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
                    App.Database.InsertOrReplace(SelectItem);
                    //if (SelectItem.FUELDENID == 0)
                    //{
                    //    App.Database.Insert(SelectItem);
                    //}
                    //else
                    //{
                    //    App.Database.Update(SelectItem);
                    //}
                }

                NewItem = null;
                GetIndexEnergyList();
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
                    App.Database.Delete<FuelDenModel>(SelectItem.FUELDENID);
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


    public class FuelDenCalcViewModel : ViewModelBase
    {

        private List<FuelDenModel> collection;
        public List<FuelDenModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged(nameof(Collection));
            }
        }
        public List<FuelDenModel> GetCollection(string Energy, string BaseTemp, string TableSymbol)
        {
            return App.Database.Table<FuelDenModel>().Where(x => x.ENERGYID.Equals(Energy) && x.BASETEMP.Equals(BaseTemp) && x.TABLE.Equals(TableSymbol)).ToList();
        }


        private List<EnergyModel> energyList = App.Database.Table<EnergyModel>().OrderBy(a => a.ENERGYNAME).ToList();
        public List<EnergyModel> EnergyList
        {
            get => energyList;
            set
            {
                energyList = value;
                OnPropertyChanged(nameof(EnergyList));
            }
        }

        private int energy;
        public int Energy
        {
            get => energy;
            set
            {
                energy = value;
                OnPropertyChanged(nameof(Energy));
            }
        }
        public int GetEnergy()
        {
            return EnergyList.IndexOf(EnergyList.Where(X => X.ENERGYID.ToString().Equals(App.ENERGY)).FirstOrDefault());
        }

        private List<FuelDenModel> baseTempList = App.Database.Table<FuelDenModel>().ToLookup(x => x.BASETEMP).Select(x => x.First()).OrderBy(a => a.BASETEMP).ToList();
        public List<FuelDenModel> BaseTempList
        {
            get => baseTempList;
            set
            {
                baseTempList = value;
                OnPropertyChanged(nameof(BaseTempList));
            }
        }

        private int baseTemp;
        public int BaseTemp
        {
            get => baseTemp;
            set
            {
                baseTemp = value;
                OnPropertyChanged(nameof(BaseTemp));
            }
        }
        public int GetBaseTemp()
        {
            return BaseTempList.IndexOf(BaseTempList.Where(X => X.BASETEMP.ToString().Equals(App.BaseTEMP)).FirstOrDefault());
        }


        public FuelDenCalcViewModel()
        {
            energy = GetEnergy();
            baseTemp = GetBaseTemp();
        }
    }



    [Table("tbFuelDensity")]
    public class FuelDenModel : ViewModelBase
    {
        [Column("FuelDenID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int FUELDENID { get; set; }   // Уникальный код

        [Column("EnergyID"), NotNull, Indexed, ForeignKey(typeof(EnergyModel))]     // Specify the foreign key
        public int ENERGYID
        {
            get => energyid;
            set
            {
                energyid = value;
                OnPropertyChanged(nameof(ENERGYID));
            }
        }

        [Column("BaseTemp"), NotNull, Indexed]
        public double BASETEMP
        {
            get => basetemp;
            set
            {
                basetemp = value;
                OnPropertyChanged(nameof(BASETEMP));
            }
        }

        [Column("TableSymbol"), NotNull, Indexed]
        public string TABLE
        {
            get => table;
            set
            {
                table = value;
                OnPropertyChanged(nameof(TABLE));
            }
        }

        [Column("Temp"), NotNull, Indexed]
        public double TEMP
        {
            get => temp;
            set
            {
                temp = value;
                OnPropertyChanged(nameof(TEMP));
            }
        }
        public string TEMPFORMAT => string.Format("{0:N2}", temp); // Поле в американском формате

        [Column("BaseDensity"), NotNull, Indexed]
        public double BASEDENSITY
        {
            get => basedensity;
            set
            {
                basedensity = value;
                OnPropertyChanged(nameof(BASEDENSITY));
            }
        }

        [Column("Density"), NotNull, Indexed]
        public double DENSITY
        {
            get => density;
            set
            {
                density = value;
                OnPropertyChanged(nameof(DENSITY));
            }
        }
        public string DENSITYFORMAT => string.Format("{0:N2}", density); // Поле в американском формате

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
        public string NOTE   // Описание
        {
            get => note;
            set
            {
                note = value;
                OnPropertyChanged(nameof(NOTE));
            }
        }



        public int energyid;
        public double basetemp;
        public string table;
        public double temp;
        public double basedensity;
        public double density;
        public string description;
        public string note;

        public FuelDenModel()
        {
            //BASETEMP = double.Parse(App.BaseTEMP);
        }
    }

}
