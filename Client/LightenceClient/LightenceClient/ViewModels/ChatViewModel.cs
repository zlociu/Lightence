using LightenceClient.Commands;
using LightenceClient.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightenceClient.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using LightenceClient.Interfaces;
using System.Linq;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;

namespace LightenceClient.ViewModels
{
    
    class ChatViewModel : BaseViewModel
    {
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<Participant> Participants { get; set; }
        public ObservableCollection<FileModel> Files { get; set; }
        public Participant MainChatName; 
        #region Managers
        public ISignalrClientManager signalrClientManager;
        public ChatManager chatManager;
        public AudioInputManager audioSendManager;
        public AudioOutputManager audioReceiveManager;
        #endregion
        #region Flags
        private bool isMeetingMuted;
        #endregion

        private MainViewModel _mainViewModel;

        #region ICommands and other GUI bindings

        #region ICommand
        public ICommand _sendMessageCommand;
        public ICommand SendMessageCommand
        {
            get { return _sendMessageCommand ??= new RelayCommandAsync(() => SendMessageCommand_Click()); }
        }

        public ICommand _microphoneButtonCommand;
        public ICommand MicrophoneButtonCommand
        {
            get { return _microphoneButtonCommand ??= new RelayCommandAsync(() => MicrophoneButtonCommand_Click()); }
        }

        public ICommand _audioMuteButtonCommand;
        public ICommand AudioMuteButtonCommand
        {
            get { return _audioMuteButtonCommand ??= new RelayCommandAsync(() => AudioMuteButtonCommand_Click()); }
        }

        public ICommand _groupMuteButtonCommand;
        public ICommand GroupMuteButtonCommand
        {
            get { return _groupMuteButtonCommand ??= new RelayCommandAsync(() => GroupMuteButtonCommand_Click()); }
        }

        public ICommand _leaveMeetingCommand;
        public ICommand LeaveMeetingCommand
        {
            get { return _leaveMeetingCommand ??= new RelayCommandAsync(() => LeaveGroupCommand_Click()); }
        }

        public ICommand _endMeetingCommand;
        public ICommand EndMeetingCommand
        {
            get { return _endMeetingCommand ??= new RelayCommandAsync(() => EndMeetingCommand_Click()); }
        }

        public ICommand _changeChat;
        public ICommand ChangeChat
        {
            get { return _changeChat ??= new RelayCommandAsync(() => ChangeChatCommand()); }
        }

        public ICommand _fileDrop;
        public ICommand FileDrop
        {
            get { return _fileDrop ??= new RelayCommandAsync<DragEventArgs>((e) => FileDropCommand(e)); }
        }

        public ICommand _addFile;
        public ICommand AddFile
        {
            get { return _addFile ??= new RelayCommandAsync(() => AddFileCommand()); }
        }

        public ICommand _downloadSelectedFile;
        public ICommand DownloadSelectedFile
        {
            get { return _downloadSelectedFile ??= new RelayCommandAsync(() => DownloadSelectedFile_Click()); }
        }


        #endregion 

        #region Binding elements

        private bool _ownerFlag;
        public bool OwnerFlag
        {
            get { return _ownerFlag; }

            set
            {
                _ownerFlag = value;
                OnPropertyChanged(nameof(OwnerFlag));
            }
        }

        private string _meetingNumber;
        public string MeetingNumber
        {
            get { return _meetingNumber; }

            set
            {
                _meetingNumber = value;
                OnPropertyChanged(nameof(MeetingNumber));
            }
        }

        private bool _microphoneIsEnabled;
        public bool MicrophoneIsEnabled
        {
            get { return _microphoneIsEnabled; }

            set
            {
                _microphoneIsEnabled = value;
                OnPropertyChanged(nameof(MicrophoneIsEnabled));
            }
        }

        private PackIconKind _microphoneIcon;
        public PackIconKind MicrophoneIcon
        {
            get { return _microphoneIcon; }

            set
            {
                _microphoneIcon = value;
                OnPropertyChanged(nameof(MicrophoneIcon));
            }
        }

        private PackIconKind _audioMuteIcon;
        public PackIconKind AudioMuteIcon
        {
            get { return _audioMuteIcon; }

            set
            {
                _audioMuteIcon = value;
                OnPropertyChanged(nameof(AudioMuteIcon));
            }
        }

        private PackIconKind _groupMuteIcon;
        public PackIconKind GroupMuteIcon
        {
            get
            {
                return _groupMuteIcon;
            }
            set
            {
                _groupMuteIcon = value;
                OnPropertyChanged(nameof(GroupMuteIcon));
            }
        }

        private Participant _currentChat;
        public Participant CurrentChat
        {
            get { return this._currentChat; }

            set
            {
                this._currentChat = value;
                OnPropertyChanged(nameof(CurrentChat));
            }
        }

        private string _messageText;
        public string MessageText
        {
            get { return this._messageText; }

            set
            {
                this._messageText = value;
                OnPropertyChanged(nameof(MessageText));

            }
        }

        public FileModel _currentFile;
        public FileModel CurrentFile
        {
            get { return this._currentFile; }

            set
            {
                this._currentFile = value;
                OnPropertyChanged(nameof(CurrentFile));

            }
        }

        public IDataObject _dropContent;
        public IDataObject DropContent
        {
            get { return this._dropContent; }

            set
            {
                this._dropContent = value;
                OnPropertyChanged(nameof(DropContent));

            }
        }

        #endregion

        #endregion

        public ChatViewModel()
        {
            chatManager = new ChatManager();
            audioSendManager = new AudioInputManager();
            audioReceiveManager = new AudioOutputManager();
            audioReceiveManager.Play();
            Messages = new ObservableCollection<Message>();
            Participants = new ObservableCollection<Participant>();
            Files = new ObservableCollection<FileModel>();
            CurrentChat = MainChatName;
            Message msg = new Message
            {
                email = "LightBot",
                message = "Hello " + Constants.currentUser.Email + "!"
            };
            AddMessage(msg);
            msg = new Message
            {
                email = "LightBot",
                message = "Remember not to give anyone private data such as password, even if someone claims to be the administrator." + Constants.currentUser.Email + "!"
            };
            AddMessage(msg);
        }

        public ChatViewModel(ISignalrClientManager _signalrClientManager, bool Owner, bool Muted, MainViewModel model)
        {
            _mainViewModel = model;
            OwnerFlag = Owner;
            if (Owner == true) isMeetingMuted = false;
            else isMeetingMuted = Muted;

            if(isMeetingMuted == false) MicrophoneIsEnabled = true;
            else MicrophoneIsEnabled = false;

            audioSendManager = new AudioInputManager();
            audioSendManager.SendBuffer += SendAudio;
            audioReceiveManager = new AudioOutputManager();
            audioReceiveManager.Play();

            AudioMuteIcon = PackIconKind.VolumeHigh;

            if (Settings.MicroMutedStartMeeting == true || isMeetingMuted == true) MicrophoneIcon = PackIconKind.MicrophoneOff;
            else
            {
                MicrophoneIcon = PackIconKind.Microphone; 
                audioSendManager.Start();
            }

            if (Muted == true) GroupMuteIcon = PackIconKind.VoiceOff;
            else GroupMuteIcon = PackIconKind.RecordVoiceOver;

            MainChatName = new Participant { Name = "Global", IsNewMessageColor = Brushes.Transparent };

            chatManager = new ChatManager();

            Messages = new ObservableCollection<Message>();
            Participants = new ObservableCollection<Participant>();
            Files = new ObservableCollection<FileModel>();

            CurrentChat = MainChatName;
            Message msg = new Message
            {
                email = "LightBot",
                message = "Hello " + Constants.currentUser.Email + "!"
            };
            AddMessage(msg);
            msg = new Message
            {
                email = "LightBot",
                message = "Never share your passwords!"
            };
            AddMessage(msg);

            signalrClientManager = _signalrClientManager;

            //add bind functions to signalrClientManager events
            signalrClientManager.AddedUserToGroup += AddedUserToGroup;
            signalrClientManager.RemovedUserFromGroup += RemovedUserFromGroup;
            signalrClientManager.RemovedFromGroup += RemovedFromGroup;
            signalrClientManager.DeletedGroup += DeletedGroup;
            signalrClientManager.TextMsg += TextMsg;
            signalrClientManager.PrivTextMsg += PrivTextMsg;
            signalrClientManager.AllMembersInGroup += GetAllGroupMembers;
            signalrClientManager.AllFilesInGroup += AllFilesInGroup;
            signalrClientManager.AudioDataReceived += AudioDataReceived;
            signalrClientManager.MuteGroup += GroupMuted;
            signalrClientManager.MuteGroupResponse += GroupMutedResponse;
            signalrClientManager.NewFile += NewFileAdded;
            signalrClientManager.GetAllMembers();
            signalrClientManager.GetAllFiles();

            signalrClientManager.ChangeFileIconColor += ChangeFileIconColor;

            MeetingNumber = signalrClientManager.GroupName;
            if (Settings.AutoCopyID == true) Clipboard.SetText(signalrClientManager.GroupName);
        }

        public Task ChangeChatCommand()
        {
            Messages.Clear();
            if (CurrentChat.Name == MainChatName.Name)
            {  
                foreach(Message m in chatManager.messages)
                {
                    Messages.Add(m);
                }
            }
            else
            {
                if (chatManager.privMessages.ContainsKey(CurrentChat.Name))
                {
                    foreach (Message m in chatManager.privMessages[CurrentChat.Name])
                    {
                        Messages.Add(m);
                    }
                }
                else
                {
                    Message[] array = { };
                    chatManager.privMessages.TryAdd(CurrentChat.Name, new List<Message>(array));
                }
                
            }
            for (int i = 0; i < Participants.Count; i++)
            {
                if (Participants[i].Name == CurrentChat.Name)
                {
                    if (Participants[i].IsNewMessageColor == Brushes.Red)
                    {
                        Participants[i].IsNewMessageColor = Brushes.Transparent;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task MicrophoneButtonCommand_Click()
        {
            
            if(MicrophoneIcon != PackIconKind.Microphone)
            {
                //turn ON microphone
                MicrophoneIcon = PackIconKind.Microphone;
                audioSendManager.Start();
            }
            else
            {
                //turn OFF microphone
                MicrophoneIcon = PackIconKind.MicrophoneOff;
                audioSendManager.Stop();
            }
            return Task.CompletedTask;
        }

        public Task AudioMuteButtonCommand_Click()
        {
            if (AudioMuteIcon != PackIconKind.VolumeHigh)
            {
                //turn ON audio
                AudioMuteIcon = PackIconKind.VolumeHigh;
                audioReceiveManager.Play();
            }
            else
            {
                //turn OFF audio
                AudioMuteIcon = PackIconKind.VolumeOff;
                audioReceiveManager.Stop();
            }
            return Task.CompletedTask;
        }

        public async Task GroupMuteButtonCommand_Click()
        {
            if (OwnerFlag)
            {
                if (GroupMuteIcon == PackIconKind.RecordVoiceOver)
                {
                    //mute group members
                    await signalrClientManager.Mute();
                }
                else
                {
                    //unmute group members
                    await signalrClientManager.Unmute();
                }
            }
        }

        public async Task LeaveGroupCommand_Click()
        {
            audioSendManager.Dispose();
            audioReceiveManager.Dispose();
            await signalrClientManager.RemoveFromGroup();
            signalrClientManager.Dispose();
        }

        public async Task EndMeetingCommand_Click()
        {
            if (OwnerFlag)
            {
                audioReceiveManager.Dispose();
                audioSendManager.Dispose();
                await signalrClientManager.DeleteGroup();
                signalrClientManager.Dispose();
            }   
        }

        public async Task SendMessageCommand_Click()
        {
            if(MessageText != string.Empty)
            {
                Message msg = new Message
                {
                    email = Constants.currentUser.Email,
                    message = MessageText ?? String.Empty
                };
                MessageText = String.Empty;
                if (CurrentChat.Name == MainChatName.Name)
                {
                    AddMessage(msg);
                    await signalrClientManager.SendMessage(msg.message);
                }
                else
                {
                    if (CurrentChat.Name == Constants.currentUser.Email)
                    {
                        AddPrivMessage(CurrentChat.Name, msg);
                    }
                    else
                    {
                        AddPrivMessage(CurrentChat.Name, msg);
                        await signalrClientManager.SendPrivMessage(msg.message, CurrentChat.Name);
                    }

                }
            }
        }

        public async Task AddFileCommand()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                await signalrClientManager.UploadFile(openFileDialog.FileName);
            }
            
        }

        public async Task FileDropCommand(DragEventArgs e) 
        {
            try
            {
                var data = e.Data.GetData(DataFormats.FileDrop) as string[];
                string msg = data[0];

                await signalrClientManager.UploadFile(msg);
            }
            catch(Exception ex)
            {
                string msg = "Signalr-File-UploadFile: error to upload file: " + ex.Message;
                Loggers.Chat_logger.Debug(msg);
            }
        }

        public async Task DownloadSelectedFile_Click()
        {
            //Files.First(x => x.Name == CurrentFile.Name).IconColor =
            //            new SolidColorBrush(Color.FromRgb(0x38, 0x3f, 0x51));
            await signalrClientManager.DownloadFile(CurrentFile.Name);
        }


        #region Event handlers to SignalR manager

        private void DeletedGroup(string id)
        {
            string msg = "Signalr-ConnectionBuilder-DeletedGroup (response): " + id;
            Loggers.Comm_logger.Debug(msg);
            _mainViewModel.SelectedViewModel = new StartViewModel(_mainViewModel);
        }

        private void AddedUserToGroup(string user)
        {
            bool findFlag = false;
            for(int i = 1; i < Participants.Count; i++)
            {
                if(String.Compare(Participants[i].Name, user) > 0)
                {
                    //dodawanie do listboxa z osobami
                    Participants.Insert(i, new Participant
                    {
                        Name = user,
                        IsNewMessageColor = Brushes.Transparent
                    });
                    findFlag = true;
                    break;   
                }
            }
            if(findFlag == false)
            {
                Participants.Add(new Participant
                {
                    Name = user,
                    IsNewMessageColor = Brushes.Transparent
                });
            }
            string msg = "Signalr-ConnectionBuilder-AddedUserToGroup (response): " + user;
            Loggers.Comm_logger.Debug(msg);
        }

        private void RemovedFromGroup(string message)
        { 
            string msg = "Signalr-ConnectionBuilder-RemovedFromGroup (response): " + message;
            Loggers.Comm_logger.Debug(msg);
            // TODO exception when error returned from server
            _mainViewModel.SelectedViewModel = new StartViewModel(_mainViewModel);
        }

        private void RemovedUserFromGroup(string user)
        {
            var isDeleted = Participants.Remove(Participants.SingleOrDefault(id => id.Name == user));
            string msg = "Signalr-ConnectionBuilder-RemovedUserFromGroup (response): " + user + " " + isDeleted;
            Loggers.Comm_logger.Debug(msg);
        }

        private void TextMsg(string author, string text)
        {
            Message msg = new Message
            {
                email = author,
                message = text
            };
            AddMessage(msg);
            if(CurrentChat.Name != MainChatName.Name)
            {
                if(Participants[0].Name == MainChatName.Name)
                {
                    Participants[0].IsNewMessageColor = Brushes.Red;
                }
                else
                {
                    for (int i = 1; i < Participants.Count; i++)
                    {
                        if (Participants[i].Name == MainChatName.Name)
                        {
                            Participants[i].IsNewMessageColor = Brushes.Red;
                        }
                    }
                }
            }
        }

        private void PrivTextMsg(string author, string text)
        {
            Message msg = new Message
            {
                email = author,
                message = text
            };
            AddPrivMessage(author, msg);
            if (CurrentChat.Name != author)
            {
                for (int i = 1; i < Participants.Count; i++)
                {
                    if (Participants[i].Name == author)
                    {
                        Participants[i].IsNewMessageColor = Brushes.Red;
                    }
                }
            }
            string log = "Signalr-ConnectionBuilder-PrivTextMsg (response): " + text;
            Loggers.Comm_logger.Debug(log);
        }

        private void GetAllGroupMembers(List<string> groupList)
        {
            List<string> temp = new List<string>(groupList);
            temp.Sort((a, b) => a.CompareTo(b));
            temp.Insert(0, MainChatName.Name);
            foreach(var elem in temp)
            {
                Participants.Add(new Participant
                {
                    Name = elem,
                    IsNewMessageColor = Brushes.Transparent
                });
            }
            string msg = "Signalr-ConnectionBuilder-GroupList (response)";
            Loggers.Comm_logger.Debug(msg);
        }

        private void AllFilesInGroup(List<string> fileList)
        {
            List<string> temp = new List<string>(fileList);
            temp.Sort((a, b) => a.CompareTo(b));
            foreach (var elem in temp)
            {
                var idx = elem.LastIndexOf('.');
                Files.Add(new FileModel
                {
                    Name = elem,
                    IconKind = ExtensionIconCheck.GetIcon(elem[idx..]),
                    IsDownloadIconColor = Brushes.Transparent
                });
            }
            string msg = "Signalr-ConnectionBuilder-GroupList (response)";
            Loggers.Comm_logger.Debug(msg);
        }

        private void AudioDataReceived(string name, byte[] buffer)
        {
            if(AudioMuteIcon == PackIconKind.VolumeHigh) audioReceiveManager.AddData(buffer);
            string log = Constants.currentUser.Email + " get: " + buffer.Length;//Convert.ToBase64String(data);
            Loggers.Audio_logger.Info(log);
            //coś zrobić z dostarczanym imieniem, jakoś oznaczyć że mówi
        }
        
        private void GroupMuted(string text)
        {
            if (text == "Muted")
            {
                MicrophoneIcon = PackIconKind.MicrophoneOff;
                MicrophoneIsEnabled = false;
                audioSendManager.Stop();
            }
            else
            {
                MicrophoneIsEnabled = true;
            }
        }

        private void GroupMutedResponse(string text)
        {
            string msg = "Signalr-ConnectionBuilder-GroupMutedResponse (response): " + text;
            Loggers.Comm_logger.Debug(msg);

            if (text == "Muted") GroupMuteIcon = PackIconKind.VoiceOff;
            else GroupMuteIcon = PackIconKind.RecordVoiceOver;
        }

        private void NewFileAdded(string filename)
        {
            var idx = filename.LastIndexOf('.');
            string log = "new file available: " + filename;
            Loggers.Comm_logger.Info(log);
            try
            {
                var newFile = new FileModel()
                {
                    Name = filename,
                    IconKind = ExtensionIconCheck.GetIcon(filename[idx..]),
                    IsDownloadIconColor = Brushes.Transparent
                };
                Files.Add(newFile);
            }
            catch(Exception)
            {
                log = "error: new file available: cannot add to list: " + filename;
                Loggers.Logger.Info(log);
            }
            
        }

        private void ChangeFileIconColor(string filename, int status)
        {
            for(var i=0; i<Files.Count;i++)
            {
                if(Files[i].Name == filename)
                {
                    switch (status)
                    {
                        case 1: Files[i].IsDownloadIconColor = new SolidColorBrush(Color.FromRgb(0x02, 0x64, 0x33)); break;
                        case 2: Files[i].IsDownloadIconColor = Brushes.Red; break;     
                    }
                    break;
                }
            }                        
        }

        #endregion

        #region Event handlers to Audio managers (Input & Output)

        private void SendAudio(byte[] buffer, int count)
        {
            var data = buffer[0..count];
            //string log = Constants.currentUser.Email + " send: " + data.Length;//Convert.ToBase64String(data);
            //Loggers.Audio_logger.Info(log);
            signalrClientManager.SendAudio(data);
        }

        #endregion


        private void AddMessage(Message msg)
        {
            chatManager.AddMessage(msg);
            if (CurrentChat.Name == MainChatName.Name)
            {
                Messages.Add(msg);
            }      
            
        }

        private void AddPrivMessage(string receiver, Message msg)
        {
            chatManager.AddPrivMessage(receiver, msg);
            if(CurrentChat.Name == receiver)
            {
                Messages.Add(msg);
            }
        }

        // calculating file size and unit
        // NOT USED, METHOD DEPRECATED
        public void calculateFileSize(ref FileModel file)
        {
            int size = 0;
            // 0-B 1-KB 2-MB 3-GB
            int which_unit = 0;
            while(size >= 1000)
            {
                if(which_unit <= 3)
                {
                    size /= 1000;
                    which_unit++;
                }
                else
                {
                    break;
                }
            }
            file.Size = size;
            switch (which_unit)
            {
                case 0: file.SizeUnit = "B"; break;
                case 1: file.SizeUnit = "KB"; break;
                case 2: file.SizeUnit = "MB"; break;
                case 3: file.SizeUnit = "GB"; break;
            }
        }
    }
}
