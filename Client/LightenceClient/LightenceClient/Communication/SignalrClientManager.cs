using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using LightenceClient.Interfaces;
using System.IO;

namespace LightenceClient.Communication
{
    class SignalrClientManager : ISignalrClientManager
    {

        private HubConnection connection;
        public string GroupName { get; set; }

        public event Action<string> TestA;
        public event Action<string> CreatedGroupName;
        public event Action<string> DeletedGroup;
        public event Action<string, bool, bool> AddedToGroup;
        public event Action<string> AddedUserToGroup;
        public event Action<string> RemovedFromGroup;
        public event Action<string> RemovedUserFromGroup;
        public event Action<string, string> TextMsg;
        public event Action<string, string> PrivTextMsg;
        public event Action<List<string>> AllMembersInGroup;
        public event Action<List<string>> AllFilesInGroup;
        public event Action<string, byte[]> AudioDataReceived;
        public event Action<string> MuteGroup;
        public event Action<string> MuteGroupResponse;
        public event Action<string> NewFile;

        public event Action<string, int> ChangeFileIconColor;

        public void BuildConnection()
        {
            GroupName = string.Empty;
            connection = new HubConnectionBuilder()
                .WithUrl(new Uri(Constants._serverAddress + "/lighthub"), options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(JWToken.Token);
                })
                .WithAutomaticReconnect()
                .Build();

            connection.ServerTimeout = TimeSpan.FromSeconds(30);
            connection.On<string>("TestA", (id) => TestA?.Invoke(id));
            connection.On<string>("CreatedGroupName", (id) => CreatedGroupName?.Invoke(id));
            connection.On<string>("DeletedGroup", (id) => DeletedGroup?.Invoke(id));
            connection.On<string, bool, bool>("AddedToGroup", (user, isOwner, isMuted) => AddedToGroup?.Invoke(user, isOwner, isMuted));
            connection.On<string>("AddedUserToGroup", (user) => AddedUserToGroup?.Invoke(user));
            connection.On<string>("RemovedFromGroup", (user) => RemovedFromGroup?.Invoke(user));
            connection.On<string>("RemovedUserFromGroup", (user) => RemovedUserFromGroup?.Invoke(user));
            connection.On<string, string>("TextMsg", (author, text) => TextMsg?.Invoke(author, text));
            connection.On<string, string>("PrivTextMsg", (author, text) => PrivTextMsg?.Invoke(author, text));
            connection.On<List<string>>("AllMembersInGroup", (list) => AllMembersInGroup?.Invoke(list));
            connection.On<List<string>>("AllFilesInGroup", (list) => AllFilesInGroup?.Invoke(list));
            connection.On<string, byte[]>("AudioDataReceived", (user, buffer) => AudioDataReceived?.Invoke(user, buffer));
            connection.On<string>("MuteGroup", (text) => MuteGroup?.Invoke(text));
            connection.On<string>("MuteGroupResponse", (text) => MuteGroupResponse?.Invoke(text));
            connection.On<string>("NewFile", (filename) => NewFile?.Invoke(filename));

            //bind first two functions
            TestA += Test;
            CreatedGroupName += CreatedGroup;
        }

        #region Private event functions
        private void Test(string id)
        {
            string msg = "SignalrClientManager-Test (response): " + id;
            Loggers.Comm_logger.Debug(msg);
        }

        private void CreatedGroup(string name)
        {
            string msg = "SignalrClientManager-CreatedGroupName (response): " + name;
            GroupName = name;
            Loggers.Comm_logger.Debug(msg);
        }
        #endregion

        #region Invokable Functions
        public async Task StartConnection()
        {
            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                string msg = "SignalrClientManager-startConnection | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(msg);
            }
        }

        public async Task TestConnection()
        {
            try
            {
                await connection.InvokeAsync<string>("Test");
            }
            catch (Exception ex)
            {
                string msg = "SignalrClientManager-testConnection | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(msg);
            }
        }

        public async Task CreateGroup(string? password, bool autoEnd)
        {
            try
            {
                await connection.InvokeAsync<string>("CreateGroup", password, autoEnd);
            }
            catch (Exception ex)
            {
                string msg = "SignalrClientManager-createGroup | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(msg);
            }
        }

        public async Task JoinGroup(string id, string? password)
        {
            try
            {
                await connection.InvokeAsync<string>("AddToGroup", id, password);
                GroupName = id;
            }
            catch (Exception ex)
            {
                string msg = "SignalrClientManager-joinGroup | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(msg);
            }

        }

        public async Task RemoveFromGroup()
        {
            try
            {
                await connection.InvokeAsync<string>("RemoveFromGroup", GroupName);
                string log = "SignalrClientManager-RemoveFromGroup:";
                Loggers.Comm_logger.Info(log);
            }
            catch (Exception ex)
            {
                string log = "SignalrClientManager-RemoveFromGroup | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(log);
            }
        }

        public async Task DeleteGroup()
        {
            try
            {
                await connection.InvokeAsync<string>("DeleteGroup", GroupName);
            }
            catch (Exception ex)
            {
                string log = "SignalrClientManager-DeleteGroup | Exception | (response): " + ex.Message;
                Loggers.Comm_logger.Error(log);
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                string log = "GLOBAL: " + Constants.currentUser.Email + " wrote: " + message;
                Loggers.Chat_logger.Info(log);
                await connection.InvokeAsync<string>("SendText", message, GroupName);
            }
            catch (Exception)
            {
                string log = Constants.currentUser.Email + " wrote: " + message;
                Loggers.Chat_logger.Error(log);
            }

        }

        public async Task SendPrivMessage(string message, string email)
        {
            try
            {
                string log = "PRIV: " + Constants.currentUser.Email + " wrote: " + message;
                Loggers.Chat_logger.Info(log);
                await connection.InvokeAsync<string>("SendTextToUser", message, GroupName, email);
            }
            catch (Exception)
            {
                string log = "PRIV: " + Constants.currentUser.Email + " wrote: " + message;
                Loggers.Chat_logger.Error(log);
            }

        }

        public async Task SendAudio(byte[] buffer)
        {
            await connection.InvokeAsync<string>("UploadAudio", buffer, GroupName);
        }

        public async Task GetAllMembers()
        {
            try
            {
                await connection.InvokeAsync<string>("GetAllMembers", GroupName);
            }
            catch (Exception)
            {
                string log = Constants.currentUser.Email + " sent all members list request:";
                Loggers.Chat_logger.Error(log);
            }
        }

        public async Task GetAllFiles()
        {
            try
            {
                await connection.InvokeAsync<string>("GetAllFiles", GroupName);
            }
            catch (Exception)
            {
                string log = Constants.currentUser.Email + " sent all files list request:";
                Loggers.Chat_logger.Error(log);
            }

        }

        public async Task Mute()
        {
            await connection.InvokeAsync<string>("Mute", GroupName);
        }

        public async Task Unmute()
        {
            await connection.InvokeAsync<string>("UnMute", GroupName);
        }

        private async IAsyncEnumerable<byte[]> SendData(byte[] data)
        {
            int offset = 0;
            int count = Math.Min(8000, data.Length - offset);
            while (count > 0)
            {
                var tmp = offset;
                offset += count;
                count = Math.Min(8000, data.Length - offset);
                yield return data[tmp..(offset)];
            }
        }

        public async Task UploadFile(string filename)
        {
            try
            {
                var data = await File.ReadAllBytesAsync(filename);
                await connection.SendAsync("SendFile", SendData(data), Path.GetFileName(filename), GroupName);
            }
            catch (Exception)
            {
                string log = Constants.currentUser.Email + " cannot sent file";
                Loggers.Comm_logger.Error(log);
            }

        }

        public async Task DownloadFile(string filename)
        {
            try
            {
                var file = File.Create(Settings.DownloadedFilesPath + filename);
                var stream = connection.StreamAsync<byte[]>("GetFile", filename, GroupName);

                await foreach (var data in stream)
                {
                    file.Write(data);
                }
                file.Flush();
                file.Close();

                ChangeFileIconColor?.Invoke(filename, 1);
            }
            catch (Exception)
            {
                ChangeFileIconColor?.Invoke(filename, 2);
            }
        }

        public async Task SendFrame(byte[] frame)
        {
            await connection.InvokeAsync<string>("UploadFrame", frame, GroupName);
        }


        public void Dispose()
        {
            connection.DisposeAsync();
        }
        #endregion
    }
}