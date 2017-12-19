using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Trap
    {
        
        public List<string> Attributes;
        public Dictionary<string, string> immediateFunctionValues;
        public Dictionary<string, List<Object>> immediateListFunctionValues;
        public JObject ExternalParameter;
        public Dictionary<string, string> afterMathFunctions;
        public Dictionary<string, string> afterMathFunctionValues;
        public List<string> TreatAsInvisible;

        public Trap()
        {
            Attributes = new List<string>();
            immediateFunctionValues = new Dictionary<string, string>();
            immediateListFunctionValues = new Dictionary<string, List<Object>>();
            ExternalParameter = new JObject();
            afterMathFunctions = new Dictionary<string, string>();
            TreatAsInvisible = new List<string>();
            afterMathFunctionValues = new Dictionary<string, string>();
        }

       

        public string GetImmediateValuesIn(string hay)
        {
            foreach (KeyValuePair<string,string> ifv in immediateFunctionValues)
            {
                string fncName = ifv.Key;
                string fncValue = ifv.Value;
                if (hay.Contains(fncName))
                {
                    //the first three characters determne its data types
                    string type = fncName.Substring(2,3);
                    string qFncValue = Bee.Hive.GetQuoted(fncValue, type, null);
                    hay = hay.Replace(fncName, qFncValue);
                }
            }
            return hay;
        }
    }
}