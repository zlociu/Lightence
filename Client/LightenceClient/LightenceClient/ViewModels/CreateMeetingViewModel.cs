using LightenceClient.Commands;
using System.Windows.Input;
using System.Threading.Tasks;
using LightenceClient.Interfaces;
using LightenceClient.Communication;

namespace LightenceClient.ViewModels
{
     class CreateMeetingViewModel : BaseViewModel
    {

        private MainViewModel _mainViewModel;
        public CreateMeetingViewModel(MainViewModel model)
        {
            this._mainViewModel = model;
            AutoEndCheck = true;
            AnonymousCheck = false;
        }

        public ICommand _createMeetingCommand;

        public ICommand CreateMeetingCommand
        {
            get { return _createMeetingCommand ?? (_createMeetingCommand = new RelayCommandAsync(() => CreateMeetingButton_Click())); }
        }

        public ICommand _backToStartCommand;

        public ICommand BackToStartCommand
        {
            get { return _backToStartCommand ?? (_backToStartCommand = new RelayCommandAsync(() => BackToStartButton_Click())); }
        }

        public ICommand UpdateViewCommand { set; get; }
        private bool? _anonymousCheck;
        public bool? AnonymousCheck
        {
            get { return _anonymousCheck; }
            set
            {
                _anonymousCheck = value;
                _anonymousCheck = (_anonymousCheck != null) ? value : false;
                OnPropertyChanged(nameof(AnonymousCheck));
            }
        }

        private bool? _autoEndCheck;
        public bool? AutoEndCheck
        {
            get { return _autoEndCheck; }
            set
            {
                _autoEndCheck = value;
                _autoEndCheck = (_autoEndCheck != null) ? value : false;
                OnPropertyChanged(nameof(AutoEndCheck));
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

        private Task BackToStartButton_Click()
        {
            _mainViewModel.SelectedViewModel = new StartViewModel(_mainViewModel);
            return Task.CompletedTask;
        }

        private async Task CreateMeetingButton_Click()
        {
            ISignalrClientManager signalrClientManager = new SignalrClientManager();
            signalrClientManager.BuildConnection();

            await signalrClientManager.StartConnection();
            //await signalrClientManager.TestConnection();
            await signalrClientManager.CreateGroup(CreatePassword == string.Empty ? null : CreatePassword, AutoEndCheck ?? true);

            _mainViewModel.SelectedViewModel = new ChatViewModel(signalrClientManager, true, false, _mainViewModel);
        }

    }
}
