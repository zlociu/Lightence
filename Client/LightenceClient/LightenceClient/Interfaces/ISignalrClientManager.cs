using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LightenceClient.Interfaces
{
    public interface ISignalrClientManager : IDisposable
    {
        public string GroupName { get; set; }

        event Action<string> TestA;
        event Action<string> CreatedGroupName;
        event Action<string> DeletedGroup;
        event Action<string, bool, bool> AddedToGroup;
        event Action<string> AddedUserToGroup;
        event Action<string> RemovedFromGroup;
        event Action<string> RemovedUserFromGroup;
        event Action<string, string> TextMsg;
        event Action<string, string> PrivTextMsg;
        event Action<string, byte[]> AudioDataReceived;
        event Action<List<string>> AllMembersInGroup;
        event Action<List<string>> AllFilesInGroup;
        event Action<string> MuteGroup;
        event Action<string> MuteGroupResponse;
        event Action<string> NewFile;

        public event Action<string, int> ChangeFileIconColor;

        void BuildConnection();
        Task TestConnection();
        Task StartConnection();

        Task CreateGroup(string? password, bool autoEnd);
        Task JoinGroup(string groupId, string? password);
        Task DeleteGroup();
        Task RemoveFromGroup();
        Task SendMessage(string message);
        Task SendPrivMessage(string message, string email);
        Task SendAudio(byte[] buffer);
        Task GetAllMembers();
        Task GetAllFiles();
        Task Mute();
        Task Unmute();
        Task UploadFile(string filename);
        Task DownloadFile(string filename);
        Task SendFrame(byte[] frame);

        
    }
}
