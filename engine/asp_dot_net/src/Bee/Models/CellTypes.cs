using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class CellTypes
    {
        public const string pk = "pk";

        public static String[] Quotables {
            get { 
                List<string> quotableList = new List<String>(){ "str", "str_", "dte", "dte_" };
                List<string> temp = new List<string>(quotableList);
                //add the coating
                foreach (string coating in Bee.Coatings.All)
                {
                    foreach (string quotable in quotableList)
                    {
                        temp.Add(coating + quotable);
                    }
                }
                //add the linings
                return temp.ToArray();
            }
        }

        public static Bee.Cell Resolve(string cellDefinition)
        {
            KeyValuePair<string, string> honeyTypeDefinition = Bee.Hive.HoneyTypes.Where(
                            ht => cellDefinition.StartsWith(ht.Key)
                        ).FirstOrDefault();
            string cellType = honeyTypeDefinition.Value;
            string cellName = cellDefinition.Substring(honeyTypeDefinition.Key.Length).Trim();
            bool isPk = Bee.CellTypes.Is(honeyTypeDefinition.Key, Bee.CellTypes.pk);
            Bee.Cell cell = new Bee.Cell() { Name = cellName, Type = honeyTypeDefinition.Key, TypeValue = cellType, IsPrimaryKey = isPk };
            return cell;
        }

        //nyd
        //this is not extensible as it does not
        //loop through all  the coatings
        //and linings
        public static bool Is(String cellType, String type)
        {
            if (cellType.Equals(type))
            {
                return true;
            }
            //check against coatings
            if (cellType.Equals(Bee.Coatings.Invisible + type))
            {
                return true;
            }

            return false;
        }
    }
}