using LightenceServer.Data;
using LightenceServer.Interfaces;
using LightenceServer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Services
{
    public class ServerLogManager: IServerLogManager
    {
        private readonly AppDbContext _dbContext;

        public ServerLogManager(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddLog(ServerLogModel model)
        {
            LogQueue.Queue.Enqueue(model);
            if (LogQueue.SaveTime < DateTime.Now.AddSeconds(-10))
            {
                LogQueue.UpdateTime();
                var tab = LogQueue.Queue.GetAll();
                _dbContext.ServerLogs.AddRange(tab);
                _dbContext.SaveChanges();
                
            }
        }

        public void AddLog(ServerLogType type, ServerLogResult result = ServerLogResult.OK, string? description = null)
        {
            var model = new ServerLogModel()
            {
                Description = description,
                Type = type,
                Result = result,
                Time = DateTime.Now
            };

            LogQueue.Queue.Enqueue(model);
            if (LogQueue.SaveTime < DateTime.Now.AddSeconds(-10))
            {
                LogQueue.UpdateTime();
                var tab = LogQueue.Queue.GetAll();
                _dbContext.ServerLogs.AddRange(tab);
                _dbContext.SaveChanges();
                
            }
        }
    }
}
