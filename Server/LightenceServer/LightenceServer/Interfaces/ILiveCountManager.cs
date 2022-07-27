using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightenceServer.Interfaces
{
    public interface ILiveCountManager<Tuser> where Tuser : notnull
    {
        public void Add(Tuser user);
        public void Substract(Tuser user);
        public bool Contain(Tuser user);
        public int Count();
        public int CountIf(Func<Tuser,bool> func);
        public int CountIntersect(IList<Tuser> list);
        public IList<Tuser> Intersect(IList<Tuser> list);
    }
}
