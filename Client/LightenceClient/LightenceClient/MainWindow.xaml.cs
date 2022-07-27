using System.Windows;
using LightenceClient.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace LightenceClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            MainViewModel MainModel = new MainViewModel();
            MainModel.SelectedViewModel = new StartViewModel(MainModel);
            this.DataContext = MainModel;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ICloseWindow vm)
            {
                vm.Close += () =>
                {
                    this.Close();
                };

            }
        }

        void Window_Closing(object sender, EventArgs e)
        {
            var reponse = HttpClientManager.LogoutUserAsync();
            Thread.Sleep(100);
        }
    }

}