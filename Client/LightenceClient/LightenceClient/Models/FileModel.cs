using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Media;
using System.Text;
using System.ComponentModel;

namespace LightenceClient.Models
{
    class FileModel: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int? Size { get; set; }
        public string? SizeUnit { get; set; }
        public string? Author { get; set; }
        public DateTime? Date { get; set; }
        public PackIconKind IconKind { get; set; }


        private Brush _isDownloadIconColor;
        public Brush IsDownloadIconColor
        {
            get { return _isDownloadIconColor; }
            set
            {
                _isDownloadIconColor = value;
                OnPropertyChanged(nameof(IsDownloadIconColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
