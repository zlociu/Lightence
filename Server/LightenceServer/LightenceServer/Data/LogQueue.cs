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
    public static class LogQueue
    {
        private static readonly object _lock = new object();
        public static ConcurrentQueue<ServerLogModel> Queue = new ConcurrentQueue<ServerLogModel>();
        public static DateTime SaveTime = DateTime.Now.AddSeconds(-10);

        public static void UpdateTime()
        {
            lock(_lock)
            {
                SaveTime = DateTime.Now;
            }
        }
    }

    public static class QueueExtension
    {
        public static IEnumerable<T> TryDequeueMany<T>(this ConcurrentQueue<T> queue, int size)
        {
            for (int i = 0; i < size && queue.Count > 0; i++)
            {
                queue.TryDequeue(out T result);
                yield return result;
            }
        }

        public static IEnumerable<T> TryDequeueAll<T>(this ConcurrentQueue<T> queue)
        {
            while( queue.Count > 0)
            {
                queue.TryDequeue(out T result);
                yield return result;
            }
        }

        public static IEnumerable<T> GetAll<T>(this ConcurrentQueue<T> queue)
        {
            T[] tab = new T[queue.Count];
            queue.CopyTo(tab, 0);
            queue.Clear();
            return tab;
        }
    }
}
