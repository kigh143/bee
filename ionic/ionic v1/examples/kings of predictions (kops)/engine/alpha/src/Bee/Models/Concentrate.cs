using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Concentrate
    {
        public string sql { get; set; }
        public Dictionary<string, Concentrate> childrenCsqls { get; set; }
        public Trap ConcetrateTrap;
    }
}