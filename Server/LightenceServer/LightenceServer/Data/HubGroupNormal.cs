using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Data
{
    public sealed class HubGroupNormal: HubGroup
    {
        public override bool IsPremium { get => false; }
        public override bool IsPassword { get => false; }
        public override bool AutoEndWhenEmpty { get => true; }

        public HubGroupNormal(string name, string ownerName) : base(name, ownerName) 
        {
            maxUsers = HubGroupValues.NormalMaxUsers;
        }

        public override int PremiumFunc()
        {
            return 0;
        }
    }
}
