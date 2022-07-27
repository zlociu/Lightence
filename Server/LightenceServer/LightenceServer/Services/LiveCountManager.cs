using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LightenceServer.Interfaces;

namespace LightenceServer.Services
{
    public class LiveCountManager<Tuser>: ILiveCountManager<Tuser> where Tuser : notnull
    {
        private object _lock = new object();
        private HashSet<Tuser> _userCount = new HashSet<Tuser>();

        public void Add(Tuser user)
        {
            lock(_lock)
            {
                _userCount.Add(user);
            }  
        }

        public void Substract(Tuser user)
        {
            lock (_lock)
            {
                _userCount.Remove(user);
            }
        }

        public bool Contain(Tuser user)
        {
            return _userCount.Contains(user);
        }

        public int Count()
        {
            return _userCount.Count;
        }

        public int CountIf(Func<Tuser, bool> func)
        {
           return _userCount.Where(func).Count();
        }

        public int CountIntersect(IList<Tuser> list)
        {
            return _userCount.Intersect(list).Count();
        }

        public IList<Tuser> Intersect(IList<Tuser> list)
        {
            return _userCount.Intersect(list).ToList();
        }
    }
}
