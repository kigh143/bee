using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Date
    {
        public static string Get(String query)
        {
            //now
            if (query.Equals("now"))
            {
                return DateTime.Now.ToString();
            }
            //now.year
            if (query.Equals("now.year"))
            {
                return DateTime.Now.Year.ToString();
            }
            //now.add.-30m
            if (query.StartsWith("now.add."))
            {
                string whatToAdd = query.Substring("now.add.".Length); //-30m
                if (whatToAdd.EndsWith("y"))
                {
                    int val = Convert.ToInt32(whatToAdd.Replace("y", ""));
                    return DateTime.Now.AddYears(val).ToString();
                }else if (whatToAdd.EndsWith("M"))
                {
                    int val = Convert.ToInt32(whatToAdd.Replace("M", ""));
                    return DateTime.Now.AddMonths(val).ToString();
                }
                else if (whatToAdd.EndsWith("d"))
                {
                    double val = Convert.ToDouble(whatToAdd.Replace("d", ""));
                    return DateTime.Now.AddDays(val).ToString();
                }
                else if (whatToAdd.EndsWith("h"))
                {
                    double val = Convert.ToDouble(whatToAdd.Replace("h", ""));
                    return DateTime.Now.AddHours(val).ToString();
                }
                else if (whatToAdd.EndsWith("m"))
                {
                    double val = Convert.ToDouble(whatToAdd.Replace("m", ""));
                    return DateTime.Now.AddMinutes(val).ToString();
                }
                else if (whatToAdd.EndsWith("s"))
                {
                    double val = Convert.ToDouble(whatToAdd.Replace("s", ""));
                    return DateTime.Now.AddSeconds(val).ToString();
                }
            }
            return query;
        }
    }
}