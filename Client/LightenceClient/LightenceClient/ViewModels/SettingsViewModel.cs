using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LightenceClient.Commands;
using LightenceClient.Services;
using Microsoft.Win32;

namespace LightenceClient.ViewModels
{
    class SettingsViewModel : BaseViewModel
    {
        private MainViewModel _mainViewModel;

        private string _audioInputSelectedDevice;
        public string AudioInputSelectedDevice
        {
            get { return _audioInputSelectedDevice; }

            set
            {
                _audioInputSelectedDevice = value;
                OnPropertyChanged(nameof(AudioInputSelectedDevice));
            }
        }

        private string _audioOutputSelectedDevice;
        public string AudioOutputSelectedDevice
        {
            get { return _audioOutputSelectedDevice; }

            set
            {
                _audioOutputSelectedDevice = value;
                OnPropertyChanged(nameof(AudioOutputSelectedDevice));
            }
        }


        public ObservableCollection<string> AudioInputDevices { get; set; }
        public ObservableCollection<string> AudioOutputDevices { get; set; }

        public ICommand _backToStartCommand;
        public ICommand BackToStartCommand
        {
            get { return _backToStartCommand ?? (_backToStartCommand = new RelayCommandAsync(() => BackToStartButton_Click())); }
        }

        public ICommand _changeFilePathCommand;
        public ICommand ChangeFilePathCommand
        {
            get { return _changeFilePathCommand ?? (_changeFilePathCommand = new RelayCommandAsync(() => ChangeFilePathCommand_Click())); }
        }

        public ICommand _changeAudioInputDevice;
        public ICommand ChangeAudioInputDevice
        {
            get { return _changeAudioInputDevice ?? (_changeAudioInputDevice = new RelayCommandAsync(() => ChangeAudioInput_Click())); }
        }

        public ICommand _changeAudioOutputDevice;
        public ICommand ChangeAudioOutputDevice
        {
            get { return _changeAudioOutputDevice ?? (_changeAudioOutputDevice = new RelayCommandAsync(() => ChangeAudioOutput_Click())); }
        }

        public string CurrentFilePath
        {
            get { return Settings.DownloadedFilesPath; }
            set 
            { 
                Settings.DownloadedFilesPath = value;
                OnPropertyChanged(nameof(CurrentFilePath));
            }
        }

        public bool MicroMutedStartMeeting
        {
            get { return Settings.MicroMutedStartMeeting; }
            set
            {
                Settings.MicroMutedStartMeeting = value;
                OnPropertyChanged(nameof(MicroMutedStartMeeting));
            }
        }

        public bool AutoCopyID
        {
            get { return Settings.AutoCopyID; }
            set
            {
                Settings.AutoCopyID = value;
                OnPropertyChanged(nameof(AutoCopyID));
            }
        }

        public bool AutostartEnabled
        {
            get { return Settings.AutostartEnabled; }
            set
            {
                Settings.AutostartEnabled = value;
                OnPropertyChanged(nameof(AutostartEnabled));
            }
        }

        public SettingsViewModel(MainViewModel model)
        {
            _mainViewModel = model;

            var inputList = AudioInputManager.GetAudioInputDevicesInfo();
            var outputList = AudioOutputManager.GetAudioOutputDevicesInfo();

            AudioInputDevices = new ObservableCollection<string>(inputList);
            AudioOutputDevices = new ObservableCollection<string>(outputList);

            //add elements to 
        }

        private Task ChangeFilePathCommand_Click()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.CheckPathExists = true;
            fileDialog.CheckFileExists = false;
            fileDialog.FileName = "Choose folder";
            if(fileDialog.ShowDialog() == true)
            {
                int idx = fileDialog.FileName.LastIndexOf('\\');
                CurrentFilePath = fileDialog.FileName[0..(idx+1)];
            }
            return Task.CompletedTask;
        }

        private Task ChangeAudioInput_Click()
        {
            AudioInputManager.SetAudioInputDevice(AudioInputManager.GetAudioInputDeviceNumberByName(AudioInputSelectedDevice));
            return Task.CompletedTask;
        }

        private Task ChangeAudioOutput_Click()
        {
            AudioOutputManager.SetAudioOutputDevice(AudioOutputManager.GetAudioOutputDeviceNumberByName(AudioOutputSelectedDevice));
            return Task.CompletedTask;
        }
       

        private async Task BackToStartButton_Click()
        {
            await Settings.SaveSettings();
            _mainViewModel.SelectedViewModel = new StartViewModel(_mainViewModel);
        }
    }
}
