using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Cell
    {
        public String Type { get; set; }
        public String TypeValue { get; set; }
        public String Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Coating { get; set; }
    }
}