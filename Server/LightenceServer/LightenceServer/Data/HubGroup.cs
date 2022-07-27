using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightenceServer.Models;

namespace LightenceServer.Data
{
    internal static class HubGroupValues
    {
        //members maximum count
        public static int PremiumMaxUsers = 13;
        public static int NormalMaxUsers = 7;

        //files maximum count
        public static int PremiumMaxFiles = 5;
        public static int NormalMaxFiles = 3;
    }

    public abstract class HubGroup
    {
        protected readonly Dictionary<string, HubGroupMemberModel> members;
        protected readonly HashSet<string> fileNames;
        protected int maxUsers;
        protected int maxFiles;

        public virtual bool AutoEndWhenEmpty { get; }

        public string GroupName { get; }
        public string Owner { get; }
        public virtual bool IsPremium { get; }
        public virtual bool IsPassword { get; }
        public int Count { get => members.Count; }

        //if members contains Owner -> can fill all users
        //else have to save one "seat" for owner
        public bool IsFull { get => members.ContainsKey(Owner) ? (maxUsers - members.Count <= 0) : (maxUsers - members.Count <= 1); }
                
        public bool IsFullFiles { get => (maxFiles - fileNames.Count) <= 0; }
        public bool IsMuted { get; private set; }

        public HubGroup(string name, string ownerName)
        {
            GroupName = name;
            Owner = ownerName;
            members = new Dictionary<string, HubGroupMemberModel>();
            fileNames = new HashSet<string>();
            IsMuted = false;
        }

        public virtual bool AddMember(string userName, HubGroupMemberModel userData, string? password = null)
        {
            return members.TryAdd(userName, userData);
        }

        public virtual bool RemoveMember(string userName)
        {
            return members.Remove(userName);
        }

        public virtual bool AddFile(string fileName)
        {
            if(fileNames.Count < HubGroupValues.NormalMaxFiles) return fileNames.Add(fileName);
            return false;
        }

        public string? GetUserConnectionID(string userName)
        {
            return members[userName]?.ConnectionID;
        }

        public void Mute()
        {
            IsMuted = true;
        }

        public void UnMute()
        {
            IsMuted = false;
        }

        public abstract int PremiumFunc();

        public List<string> GetAllMembers()
        {
            var tab = members.Keys.ToList();
            return tab;
        }

        public List<string> GetAllMembersNames()
        {
            var tab = members.Values;
            List<string> names = new List<string>();
            foreach(var elem in tab)
            {
                names.Add(elem.Name ?? string.Empty);
            }
            return names;
        }

        public List<string> GetAllFileNames()
        {
            var tab = fileNames.ToList();
            return tab;
        }
    }
}
