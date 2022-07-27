using LightenceServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Interfaces
{
    public interface IServerLogManager
    {
        public void AddLog(ServerLogModel model);

        public void AddLog(ServerLogType type, ServerLogResult success = ServerLogResult.OK, string? description = null);

    }
}
