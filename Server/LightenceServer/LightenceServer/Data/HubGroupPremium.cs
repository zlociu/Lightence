using System;
using LightenceServer.Models;

namespace LightenceServer.Data
{
    public sealed class HubGroupPremium: HubGroup
    {
        private readonly string? _password;
        private readonly bool _autoExit;

        public override bool IsPremium { get => true; }
        public override bool IsPassword { get => _password != null; }
        public override bool AutoEndWhenEmpty { get => _autoExit; }

        public HubGroupPremium(string name, string ownerName, string? password = null, bool autoEnd = true) : base(name, ownerName)
        {
            maxUsers = HubGroupValues.PremiumMaxUsers;
            _password = password;
            _autoExit = autoEnd;
        }

        public override bool AddMember(string userName, HubGroupMemberModel userData, string? password = null)
        {
            if (IsPassword)
            {
                if (_password == password)
                {
                    return members.TryAdd(userName, userData);
                }
            }
            else
            {
                return members.TryAdd(userName,userData);
            }
            return false;
        }

        public override bool AddFile(string fileName)
        {
            if (fileNames.Count < HubGroupValues.PremiumMaxFiles) return fileNames.Add(fileName);
            return false;
        }

        public override int PremiumFunc()
        {
            return 17654327;
        }
    }
}
