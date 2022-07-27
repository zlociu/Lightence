using System;
using System.Threading.Tasks;
using System.Windows.Input;
using LightenceClient.Commands;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace LightenceClient.ViewModels
{
    class AccountSettingsViewModel : BaseViewModel
    {


        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        private MainViewModel _mainViewModel;

        private string _myEmail;
        public string MyEmail
        {
            get { return _myEmail; }

            set
            {
                _myEmail = value;
                OnPropertyChanged(nameof(MyEmail));
            }
        }

        private string _firstLastName;
        public string FirstLastName
        {
            get { return _firstLastName; }

            set
            {
                _firstLastName = value;
                OnPropertyChanged(nameof(FirstLastName));
            }
        }

        private string _isPremium;
        public string IsPremium
        {
            get { return _isPremium; }

            set
            {
                _isPremium = value;
                OnPropertyChanged(nameof(IsPremium));
            }
        }

        private string _premiumCode;
        public string PremiumCode
        {
            get { return this._premiumCode; }

            set
            {
                this._premiumCode = value;
                OnPropertyChanged(nameof(PremiumCode));

            }
        }

        private string _currentPassword;
        public string CurrentPassword
        {
            get { return this._currentPassword; }

            set
            {
                this._currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));

            }
        }

        private string _newPassword;
        public string NewPassword
        {
            get { return this._newPassword; }

            set
            {
                this._newPassword = value;
                OnPropertyChanged(nameof(NewPassword));

            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return this._confirmPassword; }

            set
            {
                this._confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));

            }
        }

        private System.Windows.Media.Brush _infoBlockForeground;
        public System.Windows.Media.Brush InfoBlockForeground
        {
            get { return this._infoBlockForeground; }

            set
            {
                this._infoBlockForeground = value;
                OnPropertyChanged(nameof(InfoBlockForeground));
            }
        }

        private string _infoBlockPremium;
        public string InfoBlockPremium
        {
            get { return this._infoBlockPremium; }

            set
            {
                this._infoBlockPremium = value;
                OnPropertyChanged(nameof(InfoBlockPremium));
            }
        }

        private string _infoBlockChange;
        public string InfoBlockChange
        {
            get { return this._infoBlockChange; }

            set
            {
                this._infoBlockChange = value;
                OnPropertyChanged(nameof(InfoBlockChange));
            }
        }

        private bool? _confirmDeleteCheck;
        public bool? ConfirmDeleteCheck
        {
            get { return _confirmDeleteCheck; }
            set
            {
                _confirmDeleteCheck = value;
                _confirmDeleteCheck = (_confirmDeleteCheck != null) ? value : false;
                OnPropertyChanged(nameof(ConfirmDeleteCheck));
            }
        }

        private string _infoBlockDelete;
        public string InfoBlockDelete
        {
            get { return this._infoBlockDelete; }

            set
            {
                this._infoBlockDelete = value;
                OnPropertyChanged(nameof(InfoBlockDelete));
            }
        }

        public ICommand _backToStartCommand;

        public ICommand BackToStartCommand
        {
            get { return _backToStartCommand ?? (_backToStartCommand = new RelayCommandAsync(() => BackToStartButton_Click())); }
        }

        public ICommand _upgradeAccountCommand;

        public ICommand UpgradeAccountCommand
        {
            get { return _upgradeAccountCommand ?? (_upgradeAccountCommand = new RelayCommandAsync(() => UpgradeAccountButton_Click())); }
        }

        public ICommand _changePasswordCommand;

        public ICommand ChangePasswordCommand
        {
            get { return _changePasswordCommand ?? (_changePasswordCommand = new RelayCommandAsync(() => ChangePasswordButton_Click())); }
        }

        public ICommand _deleteAccountCommand;
        public ICommand DeleteAccountCommand
        {
            get { return _deleteAccountCommand ?? (_deleteAccountCommand = new RelayCommandAsync(() => DeleteAccountButton_Click())); }
        }

        public AccountSettingsViewModel(MainViewModel model)
        {
            _mainViewModel = model;
            MyEmail = Constants.currentUser.Email;
            FirstLastName = Constants.currentUser.FirstLastName;
            if (Constants.currentUser.Premium) IsPremium = "Yes";
            else IsPremium = "No";
            ConfirmDeleteCheck = false;
        }


        private Task BackToStartButton_Click()
        {
            _mainViewModel.SelectedViewModel = new StartViewModel(_mainViewModel);
            return Task.CompletedTask;
        }

        protected async Task UpgradeAccountButton_Click()
        {
            if (string.IsNullOrEmpty(this.PremiumCode) || this.PremiumCode.Length != 25)
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockPremium = "invalid code";
                return;
            }
            else
            {
                this.InfoBlockPremium = string.Empty;
            }
            var response = await HttpClientManager.PremiumUserAsync(this.PremiumCode.Trim().TrimEnd());
            var msg = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // jak jest dobrze to to :::
                Constants.currentUser.Premium = true;
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkGreen;
                this.InfoBlockPremium = "Account upgraded successfully. Please log in again to unlock new features";
            }
            else
            {
                // jak jest błąd to to :::
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockPremium = msg["error"].ToString();
            }
        }

        protected async Task ChangePasswordButton_Click()
        {
            if (string.IsNullOrEmpty(this.CurrentPassword))
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockChange = "No current password input";
                return;
            }
            if (string.IsNullOrEmpty(this.NewPassword))
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockChange = "No new password input";
                return;
            }
            if (string.IsNullOrEmpty(this.ConfirmPassword))
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockChange = "No confirm password input";
                return;
            }
            if (this.NewPassword != this.ConfirmPassword)
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockChange = "Confirm and new passwords are different";
                return;
            }
            var response = await HttpClientManager.ChangepassUserAsync(this.CurrentPassword, this.NewPassword);
            var msg = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // jak jest dobrze to to :::
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkGreen;
                this.InfoBlockChange = "Password changed successfully";
                //TODO close app
            }
            else
            {
                // jak jest błąd to to :::
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockChange = msg["error"].ToString();
            }
        }



        protected async Task DeleteAccountButton_Click()
        {
            //var response = await HttpClientManager.DeleteUserAsync(DeletePassword == string.Empty ? null : DeletePassword);
            if(ConfirmDeleteCheck == true)
            {
                var response = await HttpClientManager.DeleteUserAsync();
                var msg = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // jak jest dobrze to to :::
                    this.InfoBlockDelete = string.Empty;
                    _mainViewModel.Close?.Invoke();
                    //TODO close app
                }
                else
                {
                    // jak jest błąd to to :::
                    this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                    this.InfoBlockDelete = msg["error"].ToString();
                }
            }
            else
            {
                this.InfoBlockForeground = System.Windows.Media.Brushes.DarkRed;
                this.InfoBlockDelete = "Please confirm deletion";
            }
        }

    }
}
