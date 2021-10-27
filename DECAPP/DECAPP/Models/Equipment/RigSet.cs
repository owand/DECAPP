using DECAPP.Resources;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace DECAPP.Models.Equipment
{
    public class RigSetList : ViewModelBase
    {
        private static readonly object collisionLock = new object(); //Заглушка для блокирования одновременных операций с бд, если к базе данных может обращаться сразу несколько потоков

        public ObservableCollection<RigSetModel> collection;
        public ObservableCollection<RigSetModel> Collection
        {
            get => collection;
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<RigSetModel> GetCollection(string FilterType, string FilterGroup)
        {
            List<RigSetModel> _collection = (App.Database.Table<RigSetModel>().Select(a => a).Where(a =>
                        (string.IsNullOrEmpty(FilterType) || a.TYPEID.ToString().Equals(FilterType)) &&
                        (string.IsNullOrEmpty(FilterGroup) || a.GROUPID.ToString().Equals(FilterGroup)))).ToList();

            foreach (RigSetModel element in _collection)
            {
                App.Database.GetChildren(element);
            }

            return new ObservableCollection<RigSetModel>(_collection);
        }

        private RigSetModel selectItem = null;
        public RigSetModel SelectItem
        {
            get => selectItem;
            set
            {
                selectItem = value;
                OnPropertyChanged(nameof(SelectItem));
                IndexTypeList = TypeList.IndexOf(TypeList.Where(X => X.TYPEID == SelectItem?.TYPEID).FirstOrDefault());
                IndexGroupList = GroupList.IndexOf(GroupList.Where(X => X.GROUPID == SelectItem?.GROUPID).FirstOrDefault());
                IndexUnitList = UnitList.IndexOf(UnitList.Where(X => X.UNITID == SelectItem?.UNITID).FirstOrDefault());
            }
        }

        private RigSetModel newItem = null;
        public RigSetModel NewItem
        {
            get => newItem;
            set
            {
                newItem = value;
                OnPropertyChanged(nameof(NewItem));
            }
        }
        public RigSetModel PreSelectItem { get; set; }

        private List<RigTypeModel> typeList = App.Database.Table<RigTypeModel>().OrderBy(a => a.TYPENAME).ToList();
        public List<RigTypeModel> TypeList
        {
            get => typeList;
            set
            {
                typeList = value;
                OnPropertyChanged(nameof(TypeList));
            }
        }

        public List<UnitGroupModel> groupList = App.Database.Table<UnitGroupModel>().OrderBy(a => a.GROUPNAME).ToList();
        public List<UnitGroupModel> GroupList
        {
            get => groupList;
            set
            {
                groupList = value;
                OnPropertyChanged(nameof(GroupList));
            }
        }

        public List<UnitModel> unitList = App.Database.Table<UnitModel>().OrderBy(a => a.UNITNAME).ToList();
        public List<UnitModel> UnitList
        {
            get => unitList;
            set
            {
                unitList = value;
                OnPropertyChanged(nameof(UnitList));
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

        private int indexGroupList;
        public int IndexGroupList
        {
            get => indexGroupList;
            set
            {
                indexGroupList = value;
                OnPropertyChanged(nameof(IndexGroupList));
            }
        }

        private int indexUnitList;
        public int IndexUnitList
        {
            get => indexUnitList;
            set
            {
                indexUnitList = value;
                OnPropertyChanged(nameof(IndexUnitList));
            }
        }



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

        public RigSetList()
        {
            // If the table is empty, initialize the collection
            if (!App.Database.Table<RigSetModel>().Any())
            {
                Collection?.Add(new RigSetModel { });
            }
        }

        // Создаем новую запись в объединенной коллекции
        public void AddItem()
        {
            try
            {
                NewItem = new RigSetModel();
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
                    App.Database.Delete<RigSetModel>(SelectItem.RIGSETID);
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



    //Таблица комплектации станков
    [Table("tbDrillRigSet")]
    public class RigSetModel : ViewModelBase
    {
        [Column("DrillRigSetID"), PrimaryKey, AutoIncrement, Unique, NotNull, Indexed]
        public int RIGSETID { get; set; }   // Уникальный код

        [Column("DrillRigTypeID"), NotNull, Indexed, ForeignKey(typeof(RigTypeModel))]     // Specify the foreign key
        public int TYPEID
        {
            get => typeid;
            set
            {
                typeid = value;
                OnPropertyChanged(nameof(TYPEID));
            }
        }

        [Column("UnitGroupID"), NotNull, Indexed, ForeignKey(typeof(UnitGroupModel))]     // Specify the foreign key
        public int GROUPID
        {
            get => groupid;
            set
            {
                groupid = value;
                OnPropertyChanged(nameof(GROUPID));
            }
        }

        [Column("UnitID"), NotNull, Indexed, ForeignKey(typeof(UnitModel))]     // Specify the foreign key
        public int UNITID
        {
            get => unitid;
            set
            {
                unitid = value;
                OnPropertyChanged(nameof(UNITID));
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

        [Column("Transport")]
        public decimal TRANSPORT   // Колличество рейсов для перевозки
        {
            get => transport;
            set
            {
                transport = value;
                OnPropertyChanged(nameof(TRANSPORT));
            }
        }

        [Column("Amt")]
        public decimal AMT   // Колличество единиц оборудования
        {
            get => amt;
            set
            {
                amt = value;
                OnPropertyChanged(nameof(AMT));
            }
        }

        [ManyToOne]      // Many to one relationship with RigType
        public RigTypeModel RigTypes { get; set; }

        [ManyToOne]      // Many to one relationship with UnitGroups
        public UnitGroupModel UnitGroups { get; set; }

        [ManyToOne]      // Many to one relationship with Unit
        public UnitModel UnitList { get; set; }



        public int typeid;
        public int groupid;
        public int unitid;
        public string description;
        public decimal transport;
        public decimal amt;

        public RigSetModel()
        {
        }
    }
}
