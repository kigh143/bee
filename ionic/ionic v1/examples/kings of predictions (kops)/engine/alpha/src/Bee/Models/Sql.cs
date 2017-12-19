using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Sql
    {
        public string Select { get; set; }
        public string InnerJoin { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string GroupBy { get; set; }
        public string EvaluatedWhere { get; set; }
        public string realCombName { get; set; }
        public bool IsCount { get; set; }
        public bool IsString { get; set; }
        public string CountAs { get; set; }
        public string StringAs { get; set; }
        public bool isRootSql { get; set; }
        public bool rootSqlIsObject { get; set; }
        public string path { get; set; }
        public string page { get; set; }
        public string pageSize { get; set; }
        public string Query{
            get
            {
                String str = "";
                if (this.IsCount)
                {
                    string pk = Bee.Hive.GetPK(this.realCombName);
                    str = "SELECT COUNT([dbo].[" + this.realCombName + "].[" + pk + "]) AS "+ CountAs +" FROM [dbo].[" + this.realCombName + "] ";
                    this.EvaluatedWhere = (!String.IsNullOrEmpty(this.EvaluatedWhere)) ? this.EvaluatedWhere.Trim() : "";
                    str += (!String.IsNullOrEmpty(this.EvaluatedWhere)) ? " WHERE " + this.EvaluatedWhere : "";
                }
                else
                {
                    str = "SELECT " + this.Select.Trim(',') + " FROM [dbo].[" + this.realCombName + "] " + this.InnerJoin;
                    this.EvaluatedWhere = (!String.IsNullOrEmpty(this.EvaluatedWhere)) ? this.EvaluatedWhere.Trim() : "";
                    

                    if(!String.IsNullOrEmpty(this.EvaluatedWhere)){
                        str +=  " WHERE " + this.EvaluatedWhere;
                    }
                    str += (!String.IsNullOrEmpty(this.GroupBy)) ? " GROUP BY " + this.GroupBy.Trim(',') : "";

                    if (!String.IsNullOrEmpty(this.page))
                    {
                        if (String.IsNullOrEmpty(this.OrderBy))
                        {
                            string pk = Bee.Hive.GetPK(this.realCombName);
                            str += " ORDER BY " + pk + " ";
                        }
                        else
                        {
                            str += " ORDER BY " + this.OrderBy.Trim(',') ;
                        }
                        int pgNumber = Convert.ToInt32(this.page) - 1;
                        int offset = pgNumber * Convert.ToInt32(this.pageSize);
                        str += " OFFSET "+ offset.ToString() +" ROWS FETCH NEXT "+ this.pageSize +" ROWS ONLY ";
                    }
                    else
                    {
                        str += (!String.IsNullOrEmpty(this.OrderBy)) ? " ORDER BY " + this.OrderBy.Trim(',') : "";

                        if (String.IsNullOrEmpty(this.EvaluatedWhere) && this.isRootSql == true && this.rootSqlIsObject == true)
                        {
                            //the idea is that all parents will be part of the select phrase
                            //and all child queries will have a where clause
                            //so only root objects will get to this part
                            if (String.IsNullOrEmpty(this.OrderBy))
                            {
                                string pk = Bee.Hive.GetPK(this.realCombName);
                                str += " ORDER BY " + pk + " ";
                            }
                            str += " OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY ";
                        }
                    }

                    
                }
                return str;
            }
        }

        public void Prepare(ref Environment env)
        {
            this.EvaluatedWhere = Bee.Workers.Blend(this.Where, ref env);
        }

        public Dictionary<string, Sql> childSqls = new Dictionary<string, Sql>();
    }
}