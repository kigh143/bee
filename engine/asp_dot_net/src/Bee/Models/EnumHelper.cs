using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Bee
{

    /// <summary>
    ///     EnumHelper
    ///     This code was from this post 
    ///     http://stackoverflow.com/questions/13099834/how-to-get-the-display-name-attribute-of-an-enum-member-via-mvc-razor-code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EnumHelper<T>
    {
        public static IList<T> GetValues(Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static Dictionary<int, string> GetValueNamePairs(Enum value)
        {
            Dictionary<int, string> dc = new Dictionary<int, string>();
            int val = 0;
            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                val = (int)Enum.Parse(value.GetType(), fi.Name, false);
                dc.Add(val, fi.Name);
            }
            return dc;
        }

        public static List<EnumObject> GetValueNameDisplayPairs(Enum value)
        {
            List<EnumObject> dc = new List<EnumObject>();
            int val = 0;
            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var descriptionAttributes = fi.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                string displayValue = "";
                if (descriptionAttributes == null)
                {
                    displayValue = "";
                }
                else
                {
                    displayValue =  (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
                }

                val = (int)Enum.Parse(value.GetType(), fi.Name, false);
                EnumObject eno = new EnumObject()
                {
                    Display = displayValue,
                    Name = fi.Name,
                    Value = val
                };
                dc.Add(eno);
            }
            return dc;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        public static string GetDisplayValue(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }
    }
}