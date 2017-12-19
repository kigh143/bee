using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Spice
    {
        public string Name { get; set; }
        public string Smell { get; set; }
        public string Value { get; set; }
        public Spread Spread { get; set; }

        public Spice(string Name, string Smell, string Value, Spread Spread)
        {
            this.Name = Name;
            this.Smell = Smell;
            this.Value = Value;
            this.Spread = Spread;
        }
    }
}

