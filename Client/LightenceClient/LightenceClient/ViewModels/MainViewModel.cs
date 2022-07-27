using System;

namespace LightenceClient.ViewModels 
{
    public class MainViewModel : BaseViewModel , ICloseWindow
    {
        private BaseViewModel _selectedViewModel;
        
        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public Action Close { get; set; }

        public Action Open { get; set; }

        public MainViewModel()
        {
            
        }

  /*      private async Task LogoutButton_Click()
        {
            var MyWindow = Window.GetWindow(this);
            var response = await HttpClientManager.LogoutUserAsync();
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                new LoginWindow().Show();
                this.Close();
            }

        }*/
    }

    interface ICloseWindow
    {
        Action Close { get; set; }
        Action Open { get; set; }
    }

}
