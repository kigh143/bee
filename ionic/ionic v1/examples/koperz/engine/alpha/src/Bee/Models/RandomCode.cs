using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    
   
    public class RandomCode
    {
        
        public static string Code
        {
            get
            {
                RandomStringGenerator RSG = new RandomStringGenerator(false, false, true, false);
                RSG.UniqueStrings = true;
                String code = RSG.Generate(4);
                return code;
            }
        }
    }
}