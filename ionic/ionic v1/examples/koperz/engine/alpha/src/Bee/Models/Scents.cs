using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Scents
    {


        public const string Scent = "_";
        public const string Ascend = "_asc_";
        public const string Attribute = "_a";
        public const string GroupBy = "_g";
        
        public const string Where = "_w";
        public const string Pot = "_p";
        public const string Quiso = "_q";
        public const string QuisoCount = "_qCount";
        public const string QuisoString = "_qString";
        public const string QuisoFirst = "_qFirst";
        public const string QuisoLast = "_qLast";
        public const string QuisoSum = "_qSum";
        public const string Errors = "_errors";
        

        /// <summary>
        /// which could also be pronouced as zeep instead of the usuall zip
        /// </summary>
        public const string Zee = "_z";
        public const string QuisoPage = "_pg_";
        public const string As = "->";
        
        public const string Descend = "_dsc_";

        public const string Space = "_s";

        public const string Everything = "*";
        public const string Plus = "+";
        public const string Minus = "-";
        




        //a reference to a parents cellName
        //which has to be replaced by a quoted value
        public const string Valuable = "_v";
        public const string CurrentUser = "_u";
        
        public const string At = "_@";

        public const string ThisNodesRealCombName = "_$";
            
        public const string JellyImmediate = "_j";
        public const string JellyAfterMath = "_j_";
        public const string JellyImmediateList = "_l";
        public const string JellyAfterMathList = "_l_";
        public const string AtHive = "_@h_";
        public const string AtUser = "_@u_";
        public const string AtDateTime = "_@d_";
        public const string Flower = "_f";



        /// <summary>
        /// Usaully only inserted primary keys are retained
        /// </summary>
        /// 
        public const string RetainObject = "_ro_";
        public const string Retain = "_r";
        
        /// <summary>
        /// Usaully obtain a value of a retained primary key
        /// </summary>
        public const string ObtainObject = "_oo_";
        public const string Obtain = "_o";
        
        
        /// <summary>
        /// A spice has a name, a formula KeyValuePair<string, KeyValuePair<string,string> >
        /// a formula has a scent and a structure  KeyValuePair<string,string>
        /// the structur 
        /// </summary>
        public static List<Bee.Spice> Spices = new List<Bee.Spice>()
        {
            new Bee.Spice("And",                 "_and_", " AND ", Bee.Spread.Right),
            new Bee.Spice("And Bracket",         "_andb_", " AND ( ", Bee.Spread.Right),
            new Bee.Spice("Or",                  "_or_",  " OR ",  Bee.Spread.Right),
            new Bee.Spice("Or",                  "_orb_",  " OR ( ",  Bee.Spread.Right),
            new Bee.Spice("LessThanOrEqualTo",   "_ltoe", "] <= ",  Bee.Spread.None),
            new Bee.Spice("LessThanOrEqualTo",   "_lte", "] <= ",  Bee.Spread.None),
            new Bee.Spice("GreaterThanOrEqualTo","_gtoe", "] >= ",  Bee.Spread.None),
            new Bee.Spice("GreaterThanOrEqualTo","_gte", "] >= ",  Bee.Spread.None),
            new Bee.Spice("LessThan",            "_lt",   "] < ",   Bee.Spread.None),
            new Bee.Spice("GreaterThan",         "_gt",   "] > ",   Bee.Spread.None),
            new Bee.Spice("EqualTo",             "_e",    "] = ",   Bee.Spread.None),
            new Bee.Spice("NotEqualTo",          "_ne",   "] != ",  Bee.Spread.None),
            new Bee.Spice("Bracket",             "_b_",   " ( ",   Bee.Spread.Right),
            new Bee.Spice("Dracket",             "_d_",    " ) ",  Bee.Spread.None),
            new Bee.Spice("Space",               "_s",    " ",  Bee.Spread.None),
            new Bee.Spice("Like",               "_has",    "] LIKE ",  Bee.Spread.None),
            new Bee.Spice("Like",               "_lke",    "] LIKE ",  Bee.Spread.None)
        };
           
    }
}