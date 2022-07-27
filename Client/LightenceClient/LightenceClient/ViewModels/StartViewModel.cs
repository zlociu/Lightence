using System.Windows.Input;
using LightenceClient.Commands;
using System.Threading.Tasks;
using LightenceClient.Communication;
using LightenceClient.Interfaces;
using System.Threading;

namespace LightenceClient.ViewModels
{
    class StartViewModel : BaseViewModel
    {
        private bool successJoin;
        private bool isOwner;
        private bool isMuted;
        
        public ICommand UpdateViewCommand { set; get; }
        public ICommand _joinMeetingCommand;
        
        public ICommand JoinMeetingCommand
        {
            get { return _joinMeetingCommand ?? (_joinMeetingCommand = new RelayCommandAsync(()=>JoinMeetingButton_Click())); }
        }


        public ICommand _goToCreateCommand;

        public ICommand GoToCreateCommand
        {
            get { return _goToCreateCommand ?? (_goToCreateCommand = new RelayCommandAsync(() => GoToCreateView())); }
        }

        public ICommand _userAccountCommand;
        public ICommand UserAccountCommand
        {
            get { return _userAccountCommand ?? (_userAccountCommand = new RelayCommandAsync(() => AccountSettingsButton_Click())); }
        }

        public ICommand _settingsCommand;

        public ICommand SettingsCommand
        {
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommandAsync(() => SettingsButton_Click())); }
        }

        public ICommand _logoutCommand;

        public ICommand LogoutCommand
        {
            get { return _logoutCommand ?? (_logoutCommand = new RelayCommandAsync(() => LogoutButton_Click())); }
        }

        private MainViewModel _mainViewModel;
        public StartViewModel(MainViewModel model)
        {
            _mainViewModel = model;
            successJoin = false;
        }

        private string _joinpassword;
        public string JoinPassword
        {
            get { return this._joinpassword; }

            set {
                this._joinpassword = value;
                OnPropertyChanged(nameof(JoinPassword));
                
            }
        }

        private string _createpassword;
        public string CreatePassword
        {
            get { return this._createpassword; }

            set
            {
                this._createpassword = value;
                OnPropertyChanged(nameof(CreatePassword));

            }
        }

        private string _meetingID;
        public string MeetingID
        {
            get { return this._meetingID; }

            set
            {
                this._meetingID = value;
                OnPropertyChanged(nameof(MeetingID));

            }
        }

        private string _errorBlock;
        public string ErrorBlock
        {
            get { return this._errorBlock; }

            set
            {
                this._errorBlock = value;
                OnPropertyChanged(nameof(ErrorBlock));
            }
        }

        
        public StartViewModel()
        {
            successJoin = false;
            isOwner = false;
        }

        //HubConnection connection;

        private void AddedToGroup(string user, bool isOwner, bool isMuted)
        {
            if (user == "Successfully added")
            {
                successJoin = true;
                this.isOwner = isOwner;
                this.isMuted = isMuted;
            }
            string msg = "Signalr-ConnectionBuilder-AddedToGroup (response): " + user;
            Loggers.Comm_logger.Debug(msg);
        }

        private Task AccountSettingsButton_Click()
        {
            _mainViewModel.SelectedViewModel = new AccountSettingsViewModel(_mainViewModel);
            return Task.CompletedTask;
        }

        private Task SettingsButton_Click()
        {
            _mainViewModel.SelectedViewModel = new SettingsViewModel(_mainViewModel);
            return Task.CompletedTask;
        }

        private async Task LogoutButton_Click()
        {
            await HttpClientManager.LogoutUserAsync();
            _mainViewModel.Close?.Invoke();
            //exit app
        }

        private Task GoToCreateView()
        {
            _mainViewModel.SelectedViewModel = new CreateMeetingViewModel(_mainViewModel);
            return Task.CompletedTask;
        }

        protected async Task JoinMeetingButton_Click()
        {
            ErrorBlock = "";
            if (string.IsNullOrEmpty(this.MeetingID))
            {
                this.ErrorBlock = "No id input";
                return;
            }
            ISignalrClientManager signalrClientManager = new SignalrClientManager();
            signalrClientManager.BuildConnection();
            signalrClientManager.AddedToGroup += AddedToGroup;

            await signalrClientManager.StartConnection();
            //await signalrClientManager.TestConnection();
            await signalrClientManager.JoinGroup(MeetingID, JoinPassword == string.Empty ? null : JoinPassword);

            Thread.Sleep(50);
            for (int i = 0; i < 3; i++)
            {
                if (successJoin == true)
                {
                    _mainViewModel.SelectedViewModel = new ChatViewModel(signalrClientManager, isOwner, isMuted, _mainViewModel);
                    break;
                }
                Thread.Sleep(50);
            }
            ErrorBlock = "Wrong meeting id or password";
        }

    }


}
