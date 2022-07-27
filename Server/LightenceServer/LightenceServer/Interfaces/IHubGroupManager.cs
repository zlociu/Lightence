using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightenceServer.Models;

namespace LightenceServer.Interfaces
{
    public interface IHubGroupManager
    {
        public int Count { get; }
        public int CountPremium { get; }

        public Task<string> AddGroupAsync(string ownerName);
        public Task<string> AddGroupPremiumAsync(string ownerName, string? password = null, bool autoEnd = true);
        public Task<bool> RemoveGroupAsync(string groupName);
        public Task<bool> AddMemberToGroupAsync(string groupName, string userName, HubGroupMemberModel userData, string? password = null);
        public Task<bool> RemoveMemberFromGroupAsync(string groupName, string userName);
        public Task<int> PremiumTestAsync(string name);
        public Task<List<string>?> GetAllMembersInGroupAsync(string groupName);
        public Task<List<string>?> GetAllMembersNamesInGroupAsync(string groupName);
        public Task<List<string>?> GetAllFilesInGroupAsync(string groupName);
        public bool IsGroupOwner(string userName, string groupName);
        public string? GetMemberConnectionID(string userName, string groupName);
        public Task<bool> SaveFileToGroupAsync(string fileName, string groupName);
        public Task MuteGroupAsync(string groupName);
        public Task UnmuteGroupAsync(string groupName);
        public bool IsMuted(string groupName);
        public bool IsEmpty(string groupName);
    }
}
