using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DECAPP.Models
{
    public abstract class ViewModelBase : ObservableProperty
    {
        public Dictionary<string, ICommand> Commands { get; protected set; }

        //public Command BackCommand { get; set; } = new Command(async () => await Shell.Current.GoToAsync("..", true));
        //public Command BackCommand { get; set; } = new Command(() => Shell.Current.GoToAsync("..", true));

        public Task GoBack => Shell.Current.GoToAsync("..", true);
    }



}
