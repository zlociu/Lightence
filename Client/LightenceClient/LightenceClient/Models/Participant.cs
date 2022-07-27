using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace LightenceClient.Models
{
    class Participant: INotifyPropertyChanged
    {
        public string Name { get; set; }
        private Brush _isNewMessageColor;
        public Brush IsNewMessageColor
        {
            get
            {
                return _isNewMessageColor;
            }
            set
            {
                _isNewMessageColor = value;
                OnPropertyChanged(nameof(IsNewMessageColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
