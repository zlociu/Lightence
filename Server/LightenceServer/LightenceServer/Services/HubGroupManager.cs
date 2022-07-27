using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LightenceServer.Data;
using LightenceServer.Interfaces;
using LightenceServer.Models;

namespace LightenceServer.Services
{
    
    public class HubGroupManager: IHubGroupManager
    {
        private readonly object _lock;
        private readonly HashSet<uint> nameManager;
        private ConcurrentDictionary<string, HubGroup> Groups { get; set; }

        /// <summary>
        /// return number of groups
        /// </summary>
        public int Count => Groups.Count;

        /// <summary>
        /// return number of premium groups
        /// </summary>
        public int CountPremium => Groups.Count(group => group.Value.IsPremium);

        public HubGroupManager()
        {
            Groups = new ConcurrentDictionary<string, HubGroup>();
            _lock = new object();
            nameManager = new HashSet<uint>();
        }

        /// <summary>
        /// Add group name to active list<para/>
        /// Algorithm generating random number 
        /// </summary>
        /// <returns>new valid group name as string (number.ToString())</returns>
        private string ReserveName()
        {
            lock (_lock)
            {
                var rnd = new Random();
                int next;
                do
                {
                    next = rnd.Next(100000, 1000000);
                } while (nameManager.Contains((uint)next) == true);
                nameManager.Add((uint)next);
                return next.ToString();
            }
        }

        /// <summary>
        /// Remove group name from active list
        /// </summary>
        /// <param name="name"></param>
        private void ReleaseName(string name)
        {
            lock (_lock) { nameManager.Remove(uint.Parse(name)); }
        }

        /// <summary>
        /// Check if exist group with specified name
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <returns></returns>
        private bool Contains(string groupName)
        {
            return Groups.ContainsKey(groupName);
        }

        /// <summary>
        /// Create new group with normal features, reserve name
        /// </summary>
        /// <param name="ownerName"></param>
        /// <returns>group name</returns>
        public Task<string> AddGroupAsync(string ownerName)
        {
            var name = ReserveName();
            Groups.TryAdd(name, new HubGroupNormal(name, ownerName));
            return Task.FromResult(name);
        }

        /// <summary>
        /// Create new group with premium features and optional password, reserve name
        /// </summary>
        /// <param name="ownerName"></param>
        /// <param name="password">optional password</param>
        /// <returns>group name</returns>
        public Task<string> AddGroupPremiumAsync(string ownerName, string? password = null, bool autoEnd = true)
        {
            var name = ReserveName();
            Groups.TryAdd(name, new HubGroupPremium(name, ownerName, password, autoEnd));
            return Task.FromResult(name);
        }

        /// <summary>
        /// Remove group with specified name from dictionary, release name
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task<bool> RemoveGroupAsync(string groupName)
        {
            var result = Groups.TryRemove(groupName, out _);
            if (result == true)
            {
                if(Directory.Exists("MeetingFiles/" + groupName)) Directory.Delete("MeetingFiles/" + groupName, true);
                ReleaseName(groupName);
            }
            return Task.FromResult(result);
        }

        /// <summary>
        /// Add member with specified name to specified group
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <param name="userName">user name</param>
        /// <param name="userData">user data: connection ID, first & last name</param>
        /// <param name="password">optional password (only with premium)</param>
        /// <returns>true if ok, otherwise false (e.g. too many members, wrong or null password)</returns>
        public Task<bool> AddMemberToGroupAsync(string groupName, string userName, HubGroupMemberModel userData, string? password = null)
        {
            if (Contains(groupName))
            {
                if (!Groups[groupName].IsFull || Groups[groupName].Owner == userName)
                {
                    var result = Groups[groupName].AddMember(userName, userData, password);
                    return Task.FromResult(result);            
                }
            }
            return Task.FromResult(false);            
        }

        /// <summary>
        /// Remove user with specified name from specified group
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <param name="userName"></param>
        /// <returns>true if ok, otherwise false (e.g. invalid group or user name)</returns>
        public Task<bool> RemoveMemberFromGroupAsync(string groupName, string userName)
        {
            if (Contains(groupName))
            {
                var result = Groups[groupName].RemoveMember(userName);
                return Task.FromResult(result);
            }
            return Task.FromResult(false);
        }

        public Task<int> PremiumTestAsync(string name)
        {
            return Task.FromResult(Groups[name].PremiumFunc());
        }

        /// <summary>
        /// Get all participants' ID (email) from specified group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task<List<string>?> GetAllMembersInGroupAsync(string groupName)
        {
            return Task.FromResult(Groups[groupName]?.GetAllMembers());
        }

        /// <summary>
        /// Get all participants' ID (email) from specified group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task<List<string>?> GetAllFilesInGroupAsync(string groupName)
        {
            return Task.FromResult(Groups[groupName]?.GetAllFileNames());
        }

        /// <summary>
        /// Get all participants' first and last name from specified group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task<List<string>?> GetAllMembersNamesInGroupAsync(string groupName)
        {
            return Task.FromResult(Groups[groupName]?.GetAllMembersNames());
        }

        /// <summary>
        /// Check if specified user is owner of group with specified name
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="groupName">group name</param>
        /// <returns>true if is owner, false if not or invalid groupName</returns>
        public bool IsGroupOwner(string userName, string groupName)
        {
            if (Contains(groupName)) return Groups[groupName].Owner == userName;
            else return false;
        }

        /// <summary>
        /// Check if specified user is owner of group with specified name
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="groupName">group name</param>
        /// <returns>true if is owner, false if not or invalid groupName</returns>
        public string? GetMemberConnectionID(string userName, string groupName)
        {
            return Groups[groupName]?.GetUserConnectionID(userName);
        }

        /// <summary>
        /// Add file name to available files in specified group
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task<bool> SaveFileToGroupAsync(string fileName, string groupName)
        {
            return Task.FromResult(Groups[groupName].AddFile(fileName));
        }

        /// <summary>
        /// Mute specified group, only owner can talk
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task MuteGroupAsync(string groupName)
        {
            if(Contains(groupName)) Groups[groupName].Mute();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Unmute specified group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Task UnmuteGroupAsync(string groupName)
        {
            if (Contains(groupName)) Groups[groupName].UnMute();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if specified group is muted
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>true if is muted, otherwise false</returns>
        public bool IsMuted(string groupName)
        {
            return Groups[groupName].IsMuted;
        }

        /// <summary>
        /// Check if specified group is Empty and has AutoExit flag set to true
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>true if is, otherwise false</returns>
        public bool IsEmpty(string groupName)
        {
            return Groups[groupName].Count == 0 && Groups[groupName].AutoEndWhenEmpty == true;
        }

    }
}

/* ----------------< GROUP MECHANIZM DESCRIPTION >------------------
 * 
 * 1. CREATE NEW GORUP:
 * 1.1 Create new group (normal or premium)
 * 1.2 Owner can join group immediately or join later
 * 1.3 From now everybody can join (password maybe needed)
 * 
 * 2. ADD USER TO GROUP
 * 2.1 Check if user is owner or if it is enough "seats" left
 * 2.2 Add to group
 * 
 * 3. REMOVE GROUP
 * 3.1 Everybody have to disconnect from specified group
 * 3.2 Owner have to close meeting
 * 
 */