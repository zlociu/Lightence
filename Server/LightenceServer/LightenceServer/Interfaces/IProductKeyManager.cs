using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Interfaces
{
    public interface IProductKeyManager
    {
        public bool VerifyKey(string key);

        public Task<int> AddKeyAsync(string key);

    }
}
