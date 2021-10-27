using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DECAPP.Views.Mud
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MudTabPage : TabbedPage
    {
        public MudTabPage()
        {
            InitializeComponent();
        }
    }
}