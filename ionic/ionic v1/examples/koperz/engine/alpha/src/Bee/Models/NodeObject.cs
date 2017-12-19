using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class NodeObject
    {
        public JObject Node { get; set; }
        public string PartialSql { get; set; }
        public string InnerJoin { get; set; }
    }
}