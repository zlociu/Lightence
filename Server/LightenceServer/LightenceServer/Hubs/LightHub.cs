using LightenceServer.Controllers;
using LightenceServer.Interfaces;
using LightenceServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LightenceServer.Hubs
{
    [Authorize]
    public class LightHub: Hub
    {
        private readonly IHubGroupManager _hubGroupManager;
        private readonly IServerLogManager _serverLogManager;

        public LightHub(IHubGroupManager hubGroupManager,
                        IServerLogManager serverLogManager)
        {
            _hubGroupManager = hubGroupManager;
            _serverLogManager = serverLogManager;
        }

        public async Task Test()
        {
            await Clients.Caller.SendAsync("TestA", "this is testing");
        }

        /// <summary>
        /// Create new group, send groupname to Caller
        /// </summary>
        /// <param name="password">optional password if premium</param>
        /// <returns></returns>
        public async Task CreateGroup(string? password = null, bool autoEnd = true)
        {
            string groupName;
            var owner = Context.User.Identity.Name ?? Context.User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                     .Select(c => c.Value).FirstOrDefault();
            if (Context.User.IsInRole("Premium"))
            {
                groupName = await _hubGroupManager.AddGroupPremiumAsync(owner, password, autoEnd);

                //if user can create group without connecting, below should be commented 
                var userData = new HubGroupMemberModel
                {
                    ConnectionID = Context.ConnectionId,
                    Name = Context.User.Claims.Where(c => c.Properties.ContainsKey("given_name")).Select(c => c.Value).FirstOrDefault()
                };
                await _hubGroupManager.AddMemberToGroupAsync(groupName, owner, userData, password);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            else
            {
                groupName = await _hubGroupManager.AddGroupAsync(owner);

                //if user can create group without connecting, below should be commented 
                var userData = new HubGroupMemberModel
                {
                    ConnectionID = Context.ConnectionId,
                    Name = Context.User.Claims.Where(c => c.Properties.ContainsKey("given_name")).Select(c => c.Value).FirstOrDefault()
                };
                await _hubGroupManager.AddMemberToGroupAsync(groupName, owner, userData);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
            _serverLogManager.AddLog(ServerLogType.CreateGroup, ServerLogResult.OK);
            await Clients.Caller.SendAsync("CreatedGroupName", groupName);
        }

        /// <summary>
        /// Remove group, disconnect all group members
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task DeleteGroup(string groupName)
        {
            var owner = Context.User.Identity.Name ?? Context.User.Claims.Where(c => c.Properties.ContainsKey("unique_name"))
                                                                     .Select(c => c.Value).FirstOrDefault();
            if (_hubGroupManager.IsGroupOwner(owner, groupName))
            {
                await Clients.OthersInGroup(groupName).SendAsync("DeletedGroup", "End of transmission");
                await Clients.Caller.SendAsync("DeletedGroup", "deleted");

                var memberList = await _hubGroupManager.GetAllMembersInGroupAsync(groupName);
                if(memberList != null)
                {
                    foreach (var elem in memberList)
                    {
                        var id = _hubGroupManager.GetMemberConnectionID(elem, groupName);
                        await Groups.RemoveFromGroupAsync(id, groupName);
                    }
                    await _hubGroupManager.RemoveGroupAsync(groupName);

                    _serverLogManager.AddLog(ServerLogType.DeleteGroup, ServerLogResult.OK);
                }  
            }
            else
            {
                _serverLogManager.AddLog(ServerLogType.DeleteGroup, ServerLogResult.Error);
            }
        }

        /// <summary>
        /// Add Caller to specified group with optional password <para/>
        /// Can reject if already max users, or wrong passowrd
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task AddToGroup(string groupName, string? password = null)
        {
            var userName = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            var userData = new HubGroupMemberModel
            {
                ConnectionID = Context.ConnectionId,
                Name = Context.User.Claims.Where(c => c.Properties.ContainsKey("given_name")).Select(c => c.Value).FirstOrDefault()
            };
            var result = await _hubGroupManager.AddMemberToGroupAsync(
                groupName, userName, userData, password);

            if (result == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.Caller.SendAsync("AddedToGroup", "Successfully added", _hubGroupManager.IsGroupOwner(userName, groupName), _hubGroupManager.IsMuted(groupName));
                await Clients.OthersInGroup(groupName).SendAsync("AddedUserToGroup", $"{userName}");
            }
            else
            {
                await Clients.Caller.SendAsync("AddedToGroup", "Error to add to group");
            }
        }

        /// <summary>
        /// Remove Caller from specified group <para/>
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task RemoveFromGroup(string groupName)
        {
            var name = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            var result = await _hubGroupManager.RemoveMemberFromGroupAsync(groupName, name);

            await Clients.Caller.SendAsync("RemovedFromGroup", (result == true) ? "Successfully removed" : $"Error to remove from group {groupName}");
            if (result == true) await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (result == true) await Clients.OthersInGroup(groupName).SendAsync("RemovedUserFromGroup", $"{name}");
            if (result == true && _hubGroupManager.IsEmpty(groupName)) await _hubGroupManager.RemoveGroupAsync(groupName);
        }

        /// <summary>
        /// Send audio to all group members
        /// </summary>
        /// <param name="data"> passing data</param>
        /// <param name="groupName">group name</param>
        /// <param name="token">optional cancellation token</param>
        /// <returns></returns>
        public async Task UploadAudio(byte[] data, string groupName)
        {
            var name = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            //if ((await _hubGroupManager.GetAllMembersInGroupAsync(groupName))?.Contains(name) == true)
            //{
            //  if (_hubGroupManager.IsGroupOwner(name, groupName) == true || _hubGroupManager.IsMuted(groupName) == false)
            //    {
                    await Clients.OthersInGroup(groupName).SendAsync("AudioDataReceived", name, data);
            //    }
            //}
        }

        /// <summary>
        /// Stream video data to all group members
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="groupName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UploadVideo(byte[] stream, string groupName)
        {
            await Clients.OthersInGroup(groupName).SendAsync("StreamVideoDataReceived", stream);
        }

        /// <summary>
        /// Send text to group chat 
        /// </summary>
        /// <param name="text">message</param>
        /// <param name="groupName">group name</param>
        /// <returns></returns>
        public async Task SendText(string text, string groupName)
        {
            var author = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            await Clients.OthersInGroup(groupName).SendAsync("TextMsg", author, text);
        }

        /// <summary>
        /// Send file to server, after saving is available for users in group to download <para/>
        /// Every group has own folder to protect from strange file names (cause of similar names)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task SendFile(IAsyncEnumerable<byte[]> stream, string fileName, string groupName)
        {
            if (Directory.Exists("MeetingFiles/" + groupName) == false) Directory.CreateDirectory("MeetingFiles/" + groupName);
            string validName = fileName;
            if (File.Exists("MeetingFiles/" + groupName + "/" + validName) == true)
            {
                int i = 1;
                do
                {
                    validName = $"{i}_" + fileName;
                    i++;
                } while (File.Exists("MeetingFiles/" + groupName + "/" + validName));
            }
            var canUpload = await _hubGroupManager.SaveFileToGroupAsync(validName, groupName);
            var file = File.Create("MeetingFiles/" + groupName + "/" + validName);
            await foreach (var data in stream)
            {
                file.Write(data);
                //token.ThrowIfCancellationRequested();
            }
            file.Flush();
            file.Close();
            if (canUpload == true)
            {
                await Clients.Group(groupName).SendAsync("NewFile", validName);
            }
            else
            {
                File.Delete("MeetingFiles/" + groupName + "/" + validName);
            }
        }

        /// <summary>
        /// Send file to caller as stream.
        /// Every group has own folder
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="groupName"></param>
        /// <param name="token">cancellation token</param>
        /// <returns></returns>
        public async IAsyncEnumerable<byte[]> GetFile(string fileName, string groupName)
        {
            var result = await _hubGroupManager.GetAllMembersInGroupAsync(groupName);
            var name = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            if (result?.Contains(name) == true)
            {
                var data = await File.ReadAllBytesAsync("MeetingFiles/" + groupName + "/" + fileName);

                byte[] segment = new byte[8000];
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
        }

        /// <summary>
        /// send text to specified user in group
        /// </summary>
        /// <param name="text">message</param>
        /// <param name="groupName">group name</param>
        /// <param name="userName">user name</param>
        /// <returns></returns>
        public async Task SendTextToUser(string text, string groupName, string userName)
        {
            var author = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();

            var list = await _hubGroupManager.GetAllMembersInGroupAsync(groupName);
            if (list?.Contains(userName) == true && list?.Contains(author) == true)
            {
                await this.Clients.Client(_hubGroupManager.GetMemberConnectionID(userName, groupName) ?? string.Empty).SendAsync("PrivTextMsg", author, text);
            }
        }

        /// <summary>
        /// send to Caller members list 
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <returns></returns>
        public async Task GetAllMembers(string groupName)
        {
            var name = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            var list = await _hubGroupManager.GetAllMembersInGroupAsync(groupName);
            if (list?.Contains(name) == true) await Clients.Caller.SendAsync("AllMembersInGroup", list);
        }


        /// <summary>
        /// send to Caller all file names, already exists in group 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task GetAllFiles(string groupName)
        {
            var name = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            var list = await _hubGroupManager.GetAllFilesInGroupAsync(groupName);
            if (list?.Count > 0) await Clients.Caller.SendAsync("AllFilesInGroup", list);
        }

        public async Task Mute(string groupName)
        {
            var user = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            if (_hubGroupManager.IsGroupOwner(user, groupName))
            {
                await _hubGroupManager.MuteGroupAsync(groupName);
                await Clients.OthersInGroup(groupName).SendAsync("MuteGroup", "Muted");
                await Clients.Caller.SendAsync("MuteGroupResponse", "Muted");
            }
        }

        public async Task UnMute(string groupName)
        {
            var user = Context.User.Identity.Name ?? Context.User.Claims
                    .Where(c => c.Properties.ContainsKey("unique_name"))
                    .Select(c => c.Value).FirstOrDefault();
            if (_hubGroupManager.IsGroupOwner(user, groupName))
            {
                await _hubGroupManager.UnmuteGroupAsync(groupName);
                await Clients.OthersInGroup(groupName).SendAsync("MuteGroup", "Unmuted");
                await Clients.Caller.SendAsync("MuteGroupResponse", "Unmuted");
            }
        }
    }
}
