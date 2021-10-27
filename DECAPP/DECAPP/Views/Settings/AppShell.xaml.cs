using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        // трубы
        public Command GoPipeCatalogCom { get; private set; }
        public Command GoPipeTypeCom { get; private set; }
        public Command GoCouplingCom { get; private set; }
        public Command GoCouplingTypeCom { get; private set; }
        public Command GoSteelCatalogCom { get; private set; }

        // реагенты и цементы
        public Command GoMudCatalogCom { get; private set; }
        public Command GoMudTypeCom { get; private set; }
        //public Command GoMudTabCom { get; private set; }
        public Command GoCalcVolCom { get; private set; }
        public Command GoCalcWeitCom { get; private set; }
        public Command GoCalcSpacerCom { get; private set; }

        // породоразрушающий инструмент
        public Command GoBitTypeCom { get; private set; }
        public Command GoBitODCom { get; private set; }
        public Command GoBitDecodeCom { get; private set; }
        public Command GoBitCodeCom { get; private set; }

        // топливо
        public Command GoFuelDenCalcCom { get; private set; }
        public Command GoFuelDenTableCom { get; private set; }
        public Command GoEnergyCom { get; private set; }
        public Command GoEnergyTypeCom { get; private set; }

        // топливо
        public Command GoRigTypeCom { get; private set; }
        public Command GoDriveTypeCom { get; private set; }
        public Command GoUnitCom { get; private set; }
        public Command GoUnitTypeCom { get; private set; }
        public Command GoRigSetCom { get; private set; }
        public Command GoUnitGroupCom { get; private set; }

        // оснастка
        public Command GoToolsCatalogCom { get; private set; }
        public Command GoToolsTypeCom { get; private set; }

        // направленное бурение
        public Command GoDDrillCatalogCom { get; private set; }
        public Command GoDDrillTypeCom { get; private set; }

        // транспорт
        public Command GoCarCatalogCom { get; private set; }
        public Command GoCarTypeCom { get; private set; }

        // каталог пород
        public Command GoRockRangCom { get; private set; }
        public Command GoRockCom { get; private set; }
        public Command GoRockTypeCom { get; private set; }


        // прочее
        public Command GoLexisCom { get; private set; }
        public Command GoToSettingsCom { get; private set; }

        public AppShell()
        {
            InitializeComponent();

            #region  /*трубы*/
            Routing.RegisterRoute(nameof(Pipes.PipeCatalogPage), typeof(Pipes.PipeCatalogPage));
            Routing.RegisterRoute(nameof(Pipes.PipeTypePage), typeof(Pipes.PipeTypePage));
            Routing.RegisterRoute(nameof(Pipes.CouplingCatalogPage), typeof(Pipes.CouplingCatalogPage));
            Routing.RegisterRoute(nameof(Pipes.CouplingTypePage), typeof(Pipes.CouplingTypePage));
            Routing.RegisterRoute(nameof(Pipes.SteelCatalogPage), typeof(Pipes.SteelCatalogPage));

            GoPipeCatalogCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Pipes.PipeCatalogPage)); Shell.Current.FlyoutIsPresented = false; });
            GoPipeTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Pipes.PipeTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoCouplingCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Pipes.CouplingCatalogPage)); Shell.Current.FlyoutIsPresented = false; });
            GoCouplingTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Pipes.CouplingTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoSteelCatalogCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Pipes.SteelCatalogPage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*реагенты и цементы*/
            Routing.RegisterRoute(nameof(Mud.MudCatalogPage), typeof(Mud.MudCatalogPage));
            Routing.RegisterRoute(nameof(Mud.MudTypePage), typeof(Mud.MudTypePage));
            Routing.RegisterRoute(nameof(Mud.MudTabPage), typeof(Mud.MudTabPage));
            Routing.RegisterRoute(nameof(Mud.CalcCementVolPage), typeof(Mud.CalcCementVolPage));
            Routing.RegisterRoute(nameof(Mud.CalcCementWeitPage), typeof(Mud.CalcCementWeitPage));
            Routing.RegisterRoute(nameof(Mud.CalcSpacerVolPage), typeof(Mud.CalcSpacerVolPage));

            GoMudCatalogCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.MudCatalogPage)); Shell.Current.FlyoutIsPresented = false; });
            GoMudTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.MudTypePage)); Shell.Current.FlyoutIsPresented = false; });
            //GoMudTabCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.MudTabPage)); Shell.Current.FlyoutIsPresented = false; });
            GoCalcVolCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.CalcCementVolPage)); Shell.Current.FlyoutIsPresented = false; });
            GoCalcWeitCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.CalcCementWeitPage)); Shell.Current.FlyoutIsPresented = false; });
            GoCalcSpacerCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Mud.CalcSpacerVolPage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*элементы КНБК*/
            Routing.RegisterRoute(nameof(BHA.BitTypePage), typeof(BHA.BitTypePage));
            Routing.RegisterRoute(nameof(BHA.BitODPage), typeof(BHA.BitODPage));
            Routing.RegisterRoute(nameof(BHA.BitDecodePage), typeof(BHA.BitDecodePage));
            Routing.RegisterRoute(nameof(BHA.BitCodePage), typeof(BHA.BitCodePage));
            Routing.RegisterRoute(nameof(BHA.DDrillCatalogPage), typeof(BHA.DDrillCatalogPage));
            Routing.RegisterRoute(nameof(BHA.DDrillTypePage), typeof(BHA.DDrillTypePage));

            GoBitTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(BHA.BitTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoBitODCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(BHA.BitODPage)); Shell.Current.FlyoutIsPresented = false; });
            GoBitDecodeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(BHA.BitDecodePage)); Shell.Current.FlyoutIsPresented = false; });
            GoBitCodeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(BHA.BitCodePage)); Shell.Current.FlyoutIsPresented = false; });
            GoDDrillCatalogCom = new Command(async () => { await Current.GoToAsync(nameof(BHA.DDrillCatalogPage)); Current.FlyoutIsPresented = false; });
            GoDDrillTypeCom = new Command(async () => { await Current.GoToAsync(nameof(BHA.DDrillTypePage)); Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*топливо*/
            Routing.RegisterRoute(nameof(Equipment.FuelDenCalcPage), typeof(Equipment.FuelDenCalcPage));
            Routing.RegisterRoute(nameof(Equipment.FuelDenTablePage), typeof(Equipment.FuelDenTablePage));
            Routing.RegisterRoute(nameof(Equipment.EnergyPage), typeof(Equipment.EnergyPage));
            Routing.RegisterRoute(nameof(Equipment.EnergyTypePage), typeof(Equipment.EnergyTypePage));

            GoFuelDenCalcCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.FuelDenCalcPage)); Shell.Current.FlyoutIsPresented = false; });
            GoFuelDenTableCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.FuelDenTablePage)); Shell.Current.FlyoutIsPresented = false; });
            GoEnergyCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.EnergyPage)); Shell.Current.FlyoutIsPresented = false; });
            GoEnergyTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.EnergyTypePage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*оборудование*/
            Routing.RegisterRoute(nameof(Equipment.RigTypePage), typeof(Equipment.RigTypePage));
            Routing.RegisterRoute(nameof(Equipment.DriveTypePage), typeof(Equipment.DriveTypePage));
            Routing.RegisterRoute(nameof(Equipment.UnitPage), typeof(Equipment.UnitPage));
            Routing.RegisterRoute(nameof(Equipment.UnitTypePage), typeof(Equipment.UnitTypePage));
            Routing.RegisterRoute(nameof(Equipment.RigSetPage), typeof(Equipment.RigSetPage));
            Routing.RegisterRoute(nameof(Equipment.UnitGroupPage), typeof(Equipment.UnitGroupPage));

            GoRigTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.RigTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoDriveTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.DriveTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoUnitCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.UnitPage)); Shell.Current.FlyoutIsPresented = false; });
            GoUnitTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.UnitTypePage)); Shell.Current.FlyoutIsPresented = false; });
            GoRigSetCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.RigSetPage)); Shell.Current.FlyoutIsPresented = false; });
            GoUnitGroupCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Equipment.UnitGroupPage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*оснастка*/
            Routing.RegisterRoute(nameof(Tools.ToolsCatalogPage), typeof(Tools.ToolsCatalogPage));
            Routing.RegisterRoute(nameof(Tools.ToolsTypePage), typeof(Tools.ToolsTypePage));

            GoToolsCatalogCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Tools.ToolsCatalogPage)); Shell.Current.FlyoutIsPresented = false; });
            GoToolsTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Tools.ToolsTypePage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*транспорт*/
            Routing.RegisterRoute(nameof(Cars.CarCatalogPage), typeof(Cars.CarCatalogPage));
            Routing.RegisterRoute(nameof(Cars.CarTypePage), typeof(Cars.CarTypePage));

            GoCarCatalogCom = new Command(async () => { await Current.Navigation.PopToRootAsync(animated: false); await Current.GoToAsync(nameof(Cars.CarCatalogPage)); Current.FlyoutIsPresented = false; });
            GoCarTypeCom = new Command(async () => { await Current.Navigation.PopToRootAsync(animated: false); await Current.GoToAsync(nameof(Cars.CarTypePage)); Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*каталог пород*/
            Routing.RegisterRoute(nameof(Rock.RockRangPage), typeof(Rock.RockRangPage));
            Routing.RegisterRoute(nameof(Rock.RockPage), typeof(Rock.RockPage));
            Routing.RegisterRoute(nameof(Rock.RockTypePage), typeof(Rock.RockTypePage));

            GoRockRangCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Rock.RockRangPage)); Shell.Current.FlyoutIsPresented = false; });
            GoRockCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Rock.RockPage)); Shell.Current.FlyoutIsPresented = false; });
            GoRockTypeCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Rock.RockTypePage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion

            #region  /*прочее*/
            Routing.RegisterRoute(nameof(Other.LexisPage), typeof(Other.LexisPage));

            GoLexisCom = new Command(async () => { await Shell.Current.GoToAsync(nameof(Other.LexisPage)); Shell.Current.FlyoutIsPresented = false; });
            #endregion




            //Routing.RegisterRoute(nameof(Settings.SettingsPage), typeof(Settings.SettingsPage));
            //GoToSettingsCommand = new Command(async () => { await /*Current.Navigation.PopToRootAsync(animated: false);*/ Current.GoToAsync(nameof(Settings.SettingsPage)); Current.FlyoutIsPresented = false; });


            BindingContext = this;
        }


    }
}