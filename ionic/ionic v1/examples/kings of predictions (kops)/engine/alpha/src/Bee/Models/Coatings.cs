using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Coatings
    {
        public const string Invisible = "inv_";

        public static List<string> All {
            get{
              List<string> temp = new List<string>();
              temp.Add(Bee.Coatings.Invisible);
              return temp;
            }
        }
    }
}