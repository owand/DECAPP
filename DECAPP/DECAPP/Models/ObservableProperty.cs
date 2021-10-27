using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DECAPP.Models
{
    public class ObservableProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
