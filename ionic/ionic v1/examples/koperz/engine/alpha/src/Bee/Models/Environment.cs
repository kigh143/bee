using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Bee
{
    /// <summary>
    /// All processes must have an environment where they extract 
    /// Values and also push values
    /// </summary>
    public class Environment
    {
        public static int queryNo = 0;

        public Dictionary<String, String> GeneratedSql = new Dictionary<String, String>();
        public Dictionary<String, Object> Ninches = new Dictionary<String, Object>();
        public Dictionary<String, Sql> ChildSqls = new Dictionary<String, Sql>();
        public List<String> FinishedChildSqls = new List<String>();
        public Dictionary<String, Sql> RootSqls = new Dictionary<String, Sql>();

        public String CurrentChildRealCombName = "";
        public String CurrentParentRealCombName = "";
        public String CurrentPath = "";
        public List<String> GeneratedInvisiblePaths = new List<String>();
        //thes are cells that will be added laiter they begin with +
        public List<String> FutureDynamicCells = new List<String>();
        public Dictionary<String, String> ImmediateJellyValues = new Dictionary<String, String>();
        public Dictionary<String, String> Pots = new Dictionary<String, String>();
        public Dictionary<String, Object> ImmediateListJellyValues = new Dictionary<String, Object>();
        public Dictionary<String, Object> AfterMathListJellyValues = new Dictionary<String, Object>();
        public Dictionary<String, JObject> RetainedObjects = new Dictionary<String, JObject>();

        public Dictionary<String, String> AfterMathJellyValues = new Dictionary<String, String>();
        public List<String> EatenAfterMathJellyValues = new List<String>();
        public List<String> EatenAfterMathListJellyValues = new List<String>();

        public JObject CurrentUser;
        public bool IsGettingCurrentUser;
        public JObject ContextObject;
        public bool ContextIsFlower;
        public JObject Nector;
        public bool IsFlowerCall;


        public JObject Honey = new JObject();
        public JToken CurrentHoneyRef = null;
        public bool IsEvaluatingCsql;
        public bool IsSelf;
        public string Auth;
        public bool IsPosting;

        public SqlConnection conn;

        public List<Upload> Uploads = new List<Upload>();

        public string searchAfterJellyKey = "";
    }
}



