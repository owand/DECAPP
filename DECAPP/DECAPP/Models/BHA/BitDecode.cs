using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DECAPP.Models.BHA
{
    public class BitDecode : ViewModelBase
    {


        public List<BitCodeModel> Code1SymbolList { get; set; }
        public List<BitCodeModel> Code2SymbolList { get; set; }
        public List<BitCodeModel> Code3SymbolList { get; set; }
        public List<BitCodeModel> Code4SymbolList { get; set; }

        public BitDecode(int Id)
        {
            Code1SymbolList = Id == 0 ? null : new List<BitCodeModel>(App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 1 && c.LANGUAGE == App.AppLanguage).ToList());
            Code2SymbolList = Id == 0 ? null : new List<BitCodeModel>(App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 2 && c.LANGUAGE == App.AppLanguage).ToList());
            Code3SymbolList = Id == 0 ? null : new List<BitCodeModel>(App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 3 && c.LANGUAGE == App.AppLanguage).ToList());
            Code4SymbolList = Id == 0 ? null : new List<BitCodeModel>(App.Database.Table<BitCodeModel>().Where(c => c.TYPEID == Id && c.SERIAL == 4 && c.LANGUAGE == App.AppLanguage).ToList());
        }

        public List<BitCodeModel> DecodeContent(int TypeId, string Symbol_1, string Symbol_2, string Symbol_3, string Symbol_4)
        {
            List<BitCodeModel> CollectionList =
            //App.Database.Query<BitCodeModel>($"SELECT DISTINCT tbBitCode.Feature, tbBitCode.Specification " +
            //$"FROM tbBitCode " +
            //$"WHERE (tbBitCode.BitTypeID LIKE '{TypeId}' " +
            //$"AND (tbBitCode.BitCodeID LIKE '{Symbol_1}' OR tbBitCode.BitCodeID LIKE '{Symbol_2}' " +
            //$"OR tbBitCode.BitCodeID LIKE '{Symbol_3}' OR tbBitCode.BitCodeID LIKE '{Symbol_4}')) " +
            //$"GROUP BY tbBitCode.Feature, tbBitCode.Specification, tbBitCode.Serial ").OrderBy(a => a.SERIAL).ToList();

            App.Database.Query<BitCodeModel>($"SELECT DISTINCT tbBitCode.Feature, tbBitCode.Specification FROM tbBitCode " +
            $"WHERE (tbBitCode.BitTypeID LIKE '{TypeId}' " +
            $"AND (tbBitCode.BitCodeID LIKE '{Symbol_1}' OR tbBitCode.BitCodeID LIKE '{Symbol_2}' " +
            $"OR tbBitCode.BitCodeID LIKE '{Symbol_3}' OR tbBitCode.BitCodeID LIKE '{Symbol_4}')) ").GroupBy(a => (a.FEATURE, a.SPECIFICATION)).Select(x => x.First()).OrderBy(a => a.SERIAL).ToList();

            return CollectionList;
        }
    }


    public class BitTypeListViewModel : ViewModelBase
    {

        public List<BitCodeModel> TypePickerList { get; set; }

        public BitTypeListViewModel()
        {
            TypePickerList = App.Database.Table<BitCodeModel>().GroupBy(x => x.TYPEID).Select(x => x.First()).ToList();
            foreach (BitCodeModel element in TypePickerList)
            {
                App.Database.GetChildren(element);
            }
        }
    }
}