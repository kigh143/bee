using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Bee
{


    public class Workers
    {
        public static string Decorate(string sqlFragment, string combName)
        {
            string stich = " ";  //" [dbo].[" + combName + "].[";
            foreach (Spice spice in Bee.Scents.Spices)
            {
                string deco = "";
                if (spice.Spread == Bee.Spread.Both)
                {
                    deco = stich + spice.Smell + stich;
                }
                else if (spice.Spread == Bee.Spread.Left)
                {
                    deco = stich + spice.Smell ;
                }
                else if (spice.Spread == Bee.Spread.Right)
                {
                    deco = spice.Smell + stich;
                }
                else if (spice.Spread == Bee.Spread.None)
                {
                    deco = spice.Smell;
                }
                sqlFragment = sqlFragment.Replace(spice.Smell, deco);
            }
            sqlFragment = sqlFragment.Replace("_$", " [dbo].[" + combName + "].[");
            sqlFragment = sqlFragment.Replace("$", "].[");
            return sqlFragment;
        }

        public static string Ice(string sqlFragment)
        {
            string stich = " "; 
            foreach (Spice spice in Bee.Scents.Spices)
            {
                string deco = "";
                if (spice.Spread == Bee.Spread.Both)
                {
                    deco = stich + spice.Smell + stich;
                }
                else if (spice.Spread == Bee.Spread.Left)
                {
                    deco = stich + spice.Smell;
                }
                else if (spice.Spread == Bee.Spread.Right)
                {
                    deco = spice.Smell + stich;
                }
                else if (spice.Spread == Bee.Spread.None)
                {
                    deco = spice.Smell;
                }
                sqlFragment = sqlFragment.Replace(spice.Smell, deco);
            }
            return sqlFragment;
        }

        public static string Spice(string sqlFragment)
        {
            foreach (Spice spice in Bee.Scents.Spices)
            {
                sqlFragment = sqlFragment.Replace(spice.Smell, spice.Value);
            }
            return sqlFragment;
        }


        public static string FermetCellsAttributeScent(string cellsQuery, string realCombName, ref Environment env)
        {
            String sql = "";
            
            //work on the attributes/cells requested for
            String[] cellNames = cellsQuery.Trim().Split(' ').Where(str => !String.IsNullOrEmpty(str.Trim())).ToArray<string>();
            foreach (String cellName in cellNames)
            {
                //check if we have an inline function eg _qOrderYear_YEAR(OrderDate)
                if (cellName.StartsWith("_q"))
                {
                    String[] cellParts = cellName.Substring(2).Split('_');
                    string cn = cellParts[1]; //YEAR(OrderDate)
                    string ascn = cellParts[0]; //OrderYear
                    sql += " " + cn + " AS " + env.CurrentPath + "_" + ascn + ",";
                    bool isInvisible = Bee.Hive.isInvisible(cellName, realCombName);
                    if (isInvisible == true)
                    {
                        env.GeneratedInvisiblePaths.Add(env.CurrentPath + "_" + cellName);
                    }
                }
                else
                {
                    sql += " [dbo].[" + realCombName + "].[" + cellName + "] AS " + env.CurrentPath + "_" + cellName + ",";
                    bool isInvisible = Bee.Hive.isInvisible(cellName, realCombName);
                    if (isInvisible == true)
                    {
                        env.GeneratedInvisiblePaths.Add(env.CurrentPath + "_" + cellName);
                    }
                }
            }
            sql = sql.Trim();
            if (string.IsNullOrEmpty(sql))//it means that there was no cellName selected
            {   //this causes errors so we choose the first cell of this comb and we shall delete it laiter
                String cellName = Bee.Hive.GetFirstCellName(realCombName);
                sql += " [" + realCombName + "].[" + cellName + "] AS " + env.CurrentPath + "_" + cellName + " ";
                env.GeneratedInvisiblePaths.Add(env.CurrentPath + "_" + cellName);
            }
            return sql;
        }

        public static string FermetCellsWhereScent(JObject whereStructure, String realCombName, ref Environment env)
        {
            string sql = "";
            //go through all the nodes of this whereStructure
            foreach (var node in whereStructure)
            {
                string whereNodeValue =  (string)node.Value;
                //check if this contains a scent
                if (whereNodeValue.Contains(Bee.Scents.Scent))
                {
                    String[] splits = whereNodeValue.Split(' ');
                    String intepretedValue = "";
                    for (int i = 0; i < splits.Length; i++)
                    {
                        String split = splits[i];
                        if (!String.IsNullOrEmpty(split) && !String.IsNullOrEmpty(split = split.Trim()))
                        {
                            if (split.StartsWith(Bee.Scents.Scent))
                            {
                                if (split.StartsWith(Bee.Scents.At) && !split.StartsWith(Bee.Scents.AtHive) && !split.StartsWith(Bee.Scents.AtUser) && !split.StartsWith(Bee.Scents.AtDateTime))
                                { //_@ e.g _@UserName
                                    string tempVal = Bee.Queen.unpack(split, ref env);
                                    String xyz = split.Substring(Bee.Scents.At.Length);
                                    JToken tempToken = env.ContextObject[xyz];
                                    string quotedValue = Bee.Hive.GetQuoted(tempVal, tempToken.Type);
                                    //sql += node.Key + " " + quotedValue;
                                    intepretedValue += " " + quotedValue;
                                }else if (split.StartsWith(Bee.Scents.AtDateTime))
                                { //e.g _@d_now
                                    string tempVal = Bee.Queen.unpack(split, ref env);
                                    //sql += node.Key + " " + quotedValue;
                                    intepretedValue += " '" + tempVal + "'";
                                }
                                else if (split.StartsWith(Bee.Scents.AtHive) && (Bee.Hive.Mood.Equals("dev") || env.IsFlowerCall)) //e.g_@h_security_secretPotion
                                {
                                    String hivePath = split.Substring(Bee.Scents.AtHive.Length).Replace('_','.');
                                    JToken hiveNode = Bee.Engine.Hive[hivePath];
                                    string tempVal = Bee.Hive.GetQuoted((String)hiveNode, hiveNode.Type);
                                    //sql += node.Key + " " + tempVal;
                                    intepretedValue += " " + tempVal;
                                }
                                else if (split.StartsWith(Bee.Scents.AtUser) && (Bee.Hive.Mood.Equals("dev") || env.IsFlowerCall)) //e.g_@u_UserId
                                {
                                    String userPath = split.Substring(Bee.Scents.AtUser.Length).Replace('_', '.');
                                    JToken userValueNode = env.CurrentUser[userPath];
                                    string tempVal = Bee.Hive.GetQuoted(userValueNode.ToString(), userValueNode.Type);
                                    //sql += node.Key + " " + tempVal;
                                    intepretedValue += " " + tempVal;
                                }
                                else if (split.StartsWith(Bee.Scents.JellyImmediate) && !split.StartsWith(Bee.Scents.JellyAfterMath)) //_j
                                {
                                    string tempVal = Bee.Queen.unpack(split, ref env);
                                    //the function has its data type _jstrToken
                                    string quotedValue = Bee.Hive.GetQuoted(tempVal, split.Substring(Bee.Scents.JellyImmediate.Length, 3), null);
                                    //sql += node.Key + " " + quotedValue;
                                    intepretedValue += " " + quotedValue;
                                }
                                else if (split.StartsWith(Bee.Scents.JellyImmediateList) && !split.StartsWith(Bee.Scents.JellyAfterMathList)) //_l
                                {
                                    string tempVal = Bee.Queen.unpack(split, ref env);
                                    //the function has its data type _lstrToken
                                    string quotedValue = Bee.Hive.GetQuoted(tempVal, split.Substring(Bee.Scents.JellyImmediate.Length, 3), null);
                                    //sql += node.Key + " " + quotedValue;
                                    intepretedValue += " " + quotedValue;
                                }
                                else
                                {
                                    intepretedValue += " " + split;
                                }
                                
                            }
                            else
                            {
                                intepretedValue += " " + split;
                            }
                        }

                        //nyd
                        //for now we allow only one value for the where node
                        //so we shall break may be in future versions
                        break;
                    }
                    sql += node.Key + " " + intepretedValue; 
                }
                else
                {
                    string quotedValue = Bee.Hive.GetQuoted(whereNodeValue, node.Value.Type);
                    sql += node.Key + " " + quotedValue; 
                }
                

                              
            }
            sql = sql.Replace(Bee.Scents.ThisNodesRealCombName, " [dbo].[" + realCombName + "].[");

            
            //decorate
            if (!String.IsNullOrEmpty(realCombName))
            {
                string decorationResults = Bee.Workers.Decorate(sql, realCombName);
                sql = decorationResults;
            }
            //the spicing
            string spiceResults = Bee.Workers.Spice(sql);
            sql = spiceResults;


            return sql;
        }

        public static string FermetAttributeScent(KeyValuePair<string, JToken> node, string combName, string asPartial)
        {
            string sql = "";
            string nodeKey = node.Key;
            //an attribute scent means that this node key is actuall a comb (table)
            //the node key could be a plural or actual comb name
            //so the value of this is a string with cell names
            dynamic cellNamesDynamicValue = node.Value;
            string cellNamesValue = cellNamesDynamicValue;
            string[] cellNames = cellNamesValue.Split(' ');
            for (int i = 0; i < cellNames.Length; i++)
            {
                string cellName = cellNames[i];
                cellName = cellName.Trim();
                //make sure we dont take empty spaces as cell names
                if (String.IsNullOrEmpty(cellName))
                {
                    continue;
                }
                
                string asSqlStr = asPartial + "_" + cellName;
                sql = sql + " [dbo].[" + combName + "].[" + cellName + "] AS " + asSqlStr + ",";
            }
            return sql;
        }

        public static string FermetWhereScent(JObject whereJsonObject, ref Trap trap, string combName = "", bool isFlowerCall = false)
        {
            string whereSql = "";
            //go through all the nodes of this nodeObject
            foreach (var whereNode in whereJsonObject)
            {
                string whereNodeKey = whereNode.Key;
                if (whereNode.Value.Type == JTokenType.Object)
                {
                    JObject whereNestedObject = (JObject)whereNode.Value;
                    string nestedWhereSqlResults = Bee.Workers.FermetWhereScent(whereNestedObject, ref trap);
                    whereSql = whereSql + nestedWhereSqlResults;
                }
                else
                {
                    dynamic whereNodeDynamicValue = whereNode.Value;
                    string whereNodeValue = whereNodeDynamicValue;
                    string quotedValue = Bee.Hive.GetQuoted(whereNodeValue, whereNode.Value.Type);
                    //replace if is a external parameter
                    if (trap != null && trap.ExternalParameter != null && quotedValue.Contains("_@"))
                    {
                        string externalQuotedValue = Bee.Queen.prepareQuotedParam(quotedValue, trap.ExternalParameter, true, isFlowerCall);
                        whereSql = whereSql + whereNodeKey + " " + externalQuotedValue;
                    }
                    else
                    {
                        whereSql = whereSql + whereNodeKey + " " + quotedValue;
                    }
                }
            }
            //replace immediate function values
            if (trap != null)
            {
                string imvStr = trap.GetImmediateValuesIn(whereSql);
                whereSql = imvStr;
            }

            //decorate
            if (!String.IsNullOrEmpty(combName))
            {
                string decorationResults = Bee.Workers.Decorate(whereSql, combName);
                whereSql = decorationResults;
            }
            //the spicing
            string spiceResults = Bee.Workers.Spice(whereSql);
            whereSql = spiceResults;

            return whereSql;
        }

        /// <summary>
        /// In a get request, it handles the processing of a root node's child node that is an array 
        /// </summary>
        /// <param name="node">
        ///    SomeLists: [{
        ///        a: "PriceGroupId Name Description"
        ///    }]
        /// </param>
        /// <param name="nodeKey">
        ///   SomeLists
        /// </param>
        /// <param name="combName">
        ///   Dispatch
        /// </param>
        /// <param name="combNodeKey">
        ///   Dispatches
        /// </param>
        /// <returns></returns>
        public static Bee.NodeObject HandleRootNodeCollectionArray(KeyValuePair<string, JToken> node, string nodeKey, string combName, string path)
        {
            //get the comb name of this node
            string nodeCombName = Bee.Hive.GetRealCombName(nodeKey);
            JObject childNode = new JObject();
            string pathToUse = path + "_";
            //get the first node entry
            //because this one represents all the other children
            foreach (JObject childNodeConfig in node.Value)
            {
                childNode = childNodeConfig;
                break;
            }
            pathToUse = pathToUse + "a" + nodeKey;
            Bee.NodeObject no = Bee.Workers.ProcessJObject(childNode, nodeCombName, nodeKey, combName, pathToUse);
            return  no;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childNode">
        ///    {
        ///       a: "PriceGroupId Name Description"
        ///    }
        /// </param>
        /// <param name="childCombName">
        ///   SomeList
        /// </param>
        /// <param name="parentCombName">
        ///   Dispatches
        /// </param>
        /// <param name="path">
        ///   aDispatches_aSomeLists
        /// </param>
        /// <returns></returns>
        public static Bee.NodeObject ProcessJObject(JObject childNode, string childCombName, string childCombKey, String parentCombName,  string path)
        {
            NodeObject no = new NodeObject();
            no.PartialSql = "";
            no.Node = new JObject();
            no.InnerJoin = "";
            //INNER JOIN ParentComb ON childCombName.ParentCombId = ParentComb.ParentCombId
            no.InnerJoin = no.InnerJoin + " INNER JOIN " + parentCombName + " ON ";
            string pkName = Hive.GetPK(parentCombName);
            no.InnerJoin = no.InnerJoin + "  " + childCombName + "." + pkName + " = ";
            no.InnerJoin = no.InnerJoin + "  " + parentCombName + "." + pkName + " ";

            //go through all the attributes of this childNode
            foreach (var childInnerNode in childNode)
            {
                string childInnerNodeKey = childInnerNode.Key;   
                //check if this node key smells like an attribute scent          
                if (childInnerNodeKey.Equals(Bee.Scents.Attribute))
                {
                    string asPartial = path;
                    string tempSqlResults = Bee.Workers.FermetAttributeScent(childInnerNode, childCombName, asPartial);
                    no.PartialSql = no.PartialSql + tempSqlResults;                   
                }
                else if (childInnerNode.Value.Type == JTokenType.Array) //if the node value is an array e.g SomeLists
                {
                    NodeObject newNodeObject = Bee.Workers.HandleRootNodeCollectionArray(childInnerNode, childInnerNodeKey, childCombName, path);
                    no.PartialSql = no.PartialSql +  newNodeObject.PartialSql;
                    no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                    no.Node.Add(childInnerNodeKey, new JArray() as dynamic);
                }
                else if (childInnerNode.Value.Type == JTokenType.Object) //if this node is an object
                {                                    
                    JObject navObj = childInnerNode.Value as JObject;
                    string pathToUse = path + "_";
                    pathToUse = pathToUse + "o" + childInnerNodeKey;
                    string realNavObjTableName = Hive.GetRealCombName(childInnerNodeKey);
                    NodeObject newNodeObject = Bee.Workers.ProcessJObject(navObj, realNavObjTableName, childInnerNodeKey, childCombName, pathToUse);
                    no.PartialSql = no.PartialSql + newNodeObject.PartialSql;
                    no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                    no.Node.Add(childInnerNodeKey, newNodeObject.Node);
                }
            }



            return no;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentNode">
        ///    {
        ///       a: "PriceGroupId Name Description"
        ///    }
        /// </param>
        /// <param name="parentCombName">
        ///   SomeList
        /// </param>
        /// <param name="parentCombKey">
        ///   SomeLists
        /// </param>
        /// <param name="childCombName">
        ///   Dispatch
        /// </param>
        /// <param name="path">
        ///   aDispatches_aSomeLists
        /// </param>
        /// <returns></returns>
        public static Bee.NodeObject ProcessParentJObject(JObject parentNode, string parentCombName, string parentCombKey, String childCombName, string path, ref Bee.Trap trap)
        {
            NodeObject no = new NodeObject();
            no.PartialSql = "";
            no.Node = new JObject();
            no.InnerJoin = "";

            Bee.Workers.PrepareAttributes(ref parentNode, parentCombName, ref trap);


            //INNER JOIN ParentComb ON childCombName.ParentCombId = ParentComb.ParentCombId
            no.InnerJoin = no.InnerJoin + " INNER JOIN [dbo].[" + parentCombName + "] ON ";
            string pkName = Hive.GetPK(parentCombName);
            no.InnerJoin = no.InnerJoin + "  [dbo].[" + childCombName + "].[" + pkName + "] = ";
            no.InnerJoin = no.InnerJoin + "  [dbo].[" + parentCombName + "].[" + pkName + "] ";

            //go through all the attributes of this childNode
            foreach (var parentInnerNode in parentNode)
            {
                string childInnerNodeKey = parentInnerNode.Key;
                //check if this node key smells like an attribute scent          
                if (childInnerNodeKey.Equals(Bee.Scents.Attribute))
                {
                    string asPartial = path;
                    string tempSqlResults = Bee.Workers.FermetAttributeScent(parentInnerNode, parentCombName, asPartial);
                    no.PartialSql = no.PartialSql + tempSqlResults;
                }
                else if (parentInnerNode.Value.Type == JTokenType.Array) //if the node value is an array e.g SomeLists
                {
                    NodeObject newNodeObject = Bee.Workers.HandleRootNodeCollectionArray(parentInnerNode, childInnerNodeKey, childCombName, path);
                    no.PartialSql = no.PartialSql + newNodeObject.PartialSql;
                    no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                    no.Node.Add(childInnerNodeKey, new JArray() as dynamic);
                }
                else if (parentInnerNode.Value.Type == JTokenType.Object) //if this node is an object
                {
                    JObject navObj = parentInnerNode.Value as JObject;
                    string pathToUse = path + "_";
                    pathToUse = pathToUse + "o" + childInnerNodeKey;
                    string realNavObjTableName = Hive.GetRealCombName(childInnerNodeKey);
                    NodeObject newNodeObject = Bee.Workers.ProcessJObject(navObj, realNavObjTableName, childInnerNodeKey, childCombName, pathToUse);
                    no.PartialSql = no.PartialSql + newNodeObject.PartialSql;
                    no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                    no.Node.Add(childInnerNodeKey, newNodeObject.Node);
                }
            }



            return no;
        }

        public static void TurnNectorIntoHoney(string nector, ref JObject honey)
        {
            var conn = Bee.Hive.GetConnection();
            if(conn.State == System.Data.ConnectionState.Closed){
                    conn.Open();
            }
            
            using (SqlCommand sqlCmd = new SqlCommand(nector, conn))
            {
                
                
                
                SqlDataReader reader = sqlCmd.ExecuteReader();

                if (reader == null || reader.HasRows == false)
                {
                    //bRet = false;
                }
                else
                {
                    Bee.Workers.Brew(reader, ref honey);
                }

                //close the reader
                if (reader != null && reader.IsClosed == false)
                {
                    reader.Close();
                }
            }

            
        }

        public static string Blend(string rawWhere, ref Environment env)
        {
            if (String.IsNullOrEmpty(rawWhere) || String.IsNullOrEmpty(rawWhere.Trim()))
            {
                return "";
            }
            rawWhere = rawWhere.Trim();
            //replace immediate jelly values
            foreach (KeyValuePair<string, string> ijv in env.ImmediateJellyValues)
            {
                if (rawWhere.Contains(ijv.Key))
                {
                    string type = ijv.Key.Substring(2, 3); //_jstrToken
                    string qJellyValue = Bee.Hive.GetQuoted(ijv.Value, type, null);
                    rawWhere = rawWhere.Replace(ijv.Key, qJellyValue);
                }
            }

            //Valuables _v At hives _@h_  Ats _@   Immediate Jellies _j
            string []rawWhereParts = rawWhere.Split(' ');
            string newWhere = "";
            for (int i = 0; i < rawWhereParts.Length; i++)
            {
                string part = rawWhereParts[i];
                if (!String.IsNullOrEmpty(part) && !String.IsNullOrEmpty(part = part.Trim()))
                {
                    if (part.StartsWith(Bee.Scents.Valuable))
                    {
                        String path = part.Substring(Bee.Scents.Valuable.Length);
                        String []splits = path.Split('.');
                        String cleanPath = "";
                        for (int p = 0; p < splits.Length - 1; p++)
                        {
                            cleanPath += splits[p].Substring(1) + ".";
                        }
                        cleanPath += splits[splits.Length - 1];
                        path = cleanPath;
                        splits = path.Split('.');

                        //nyd
                        //ability to lookup this value up or down a tree even in lists of children and parents
                        String cellName = splits[splits.Length - 1];
                        dynamic dyn = env.CurrentHoneyRef[cellName];
                        String temp = dyn;
                        String combName = splits[splits.Length - 2];
                        String realCombName = Bee.Hive.GetRealCombName(combName);
                        //oc
                        //String qVal = Bee.Hive.GetQuoted(temp, cellName, realCombName);
                        String qVal = ""; 
                        try
                        {
                            qVal = Bee.Hive.GetQuoted(temp, cellName, realCombName);
                        }catch(Exception ex){
                            string fff = "";
                        }
                        newWhere += " " + qVal;
                    }
                    else
                    {
                        newWhere += " " + part;
                    }
                }
            }
            rawWhere = newWhere;
                
            //put some icing suger
            rawWhere = Bee.Workers.Ice(rawWhere);

            //the spicing givs the honey its aroma and taste
            rawWhere = Bee.Workers.Spice(rawWhere);

            return rawWhere;
        }

        public static SqlConnection RetriveConn(ref Environment env)
        {
            SqlConnection conn = null;
            if (env.conn != null)
            {
                conn = env.conn;
            }
            else
            {
                conn = Bee.Hive.GetConnection();
                env.conn = conn;
            }

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }

        public static List<Object> GetResults(Sql sql, SqlConnection conn)
        {
            List<Object> Results = new List<Object>();
            using (SqlCommand sqlCmd = new SqlCommand(sql.Query, conn))
            {
                SqlDataReader reader = sqlCmd.ExecuteReader();
                Environment.queryNo += 1;
                if (reader == null || reader.HasRows == false)
                {
                }
                else
                {
                    int numberOfColumns = reader.FieldCount;
                    while (reader.Read())
                    {
                        //this defines an entire row
                        List<DataCell> dataCells = new List<DataCell>();
                        for (int columnIndex = 0; columnIndex < numberOfColumns; columnIndex++)
                        {
                            string pathToCellValue = reader.GetName(columnIndex);
                            Object cellValue = reader.GetValue(columnIndex);
                            dataCells.Add(new DataCell() { pathToCellValue = pathToCellValue, cellValue = cellValue });
                        }//end of for loop for columns
                        Results.Add(dataCells);
                    }//end while (reader.Read())  

                }

                //close the reader
                if (reader != null && reader.IsClosed == false)
                {
                    reader.Close();
                }
            }//end using
            return Results;
        }

        public static JToken MakeHoney(Sql sql, ref Environment env, bool isEvaluatingChildSqls, string childPathPassedIn)
        {
            JToken innerHoney = null;
            if (isEvaluatingChildSqls)
            {
                innerHoney = new JArray();
            }

            List<Object> Results = Bee.Workers.GetResults(sql, Bee.Workers.RetriveConn(ref env));
            bool hasEmptyResults = (Results == null || Results.Count() == 0);

            if (hasEmptyResults)
            {
                //we need to add either null or an empty array at the sqls path
                //bool s = true;
                string[] currentPathParts = env.CurrentPath.Split('_');
                JToken pathPoint = env.Honey;
                for (int i = 0; i < currentPathParts.Length; i++)
                {
                    string pth = currentPathParts[i];
                    string path = pth.Substring(1);
                    bool isArray = pth.StartsWith(Bee.Engine.PathListIndicator);
                    JToken nxt = pathPoint[path];
                    if (nxt == null && isArray)
                    {
                        //add an empty array here
                        ((JObject)pathPoint).Add(path, new JArray());
                    }
                    else if (nxt == null && isArray)
                    {
                        //add a null value here
                        ((JObject)pathPoint).Add(path, null);
                    }
                    else
                    {
                        pathPoint = nxt;
                    }
                }
            }

            //this is a single object or row
            bool addItToANewObject = false;
            for (int i = 0; i < Results.Count(); i++)
            {
                bool isActuallyQueringOneObject = false;
                String childrenPathTest = "";
                //new staff
                List<KeyValuePair<String, Sql>> listOfChildSqls = new List<KeyValuePair<string, Sql>>();
                //the properties of this object
                List<DataCell> dataCells = (List<DataCell>)Results[i];
                for (int j = 0; j < dataCells.Count(); j++)
                {
                    DataCell dc = dataCells[j];
                    Object cellValue = dc.cellValue;

                    childrenPathTest = "";
                    

                    //new staff
                    //check if this individual path need a child
                    //first tcheck if its zipped
                    if (dc.pathToCellValue.StartsWith(Bee.Scents.Zee))
                    {
                        bool hi = true;
                    }
                    else
                    {
                        //cut off the tail
                        int ui = dc.pathToCellValue.LastIndexOf('_');
                        string trimmed = dc.pathToCellValue.Substring(0, ui);
                        //check if there is any child whose trimmed path is equal to this
                        //List<KeyValuePair<String, Sql>> newlistOfChildSqls = env.ChildSqls.Where(csql => (
                        //    csql.Key.StartsWith(trimmed) &&
                        //    csql.Key.Substring(trimmed.Length).Split('_').Length == 1 &&
                        //    listOfChildSqls.Where( xx => xx.Key == csql.Key ).Count() == 0
                        //)).ToList();

                        //csql.Key.StartsWith(Bee.Scents.Zee) && csql.Key.Split('~')[1].StartsWith(childrenPathTest) &&
                        //csql.Key.Split('~')[1].Substring(childrenPathTest.Length).Split('_').Length == 1)
                        foreach (var cxql in env.ChildSqls)
                        {
                            //_zNum~oNewsFeed_lComments
                            if (cxql.Key.StartsWith(Bee.Scents.Zee))
                            {
                                String zipHead = cxql.Key.Split('~')[1];
                                if (!zipHead.StartsWith(trimmed))
                                {
                                    continue;
                                }

                                if (trimmed.Length + 1 > zipHead.Length)
                                {
                                    continue;
                                }

                                String remainWith = zipHead.Substring(trimmed.Length + 1);
                                String[] remains = remainWith.Split('_');
                                if (remains.Length != 1)
                                {
                                    continue;
                                }
                                int num = listOfChildSqls.Where(xx => xx.Key == cxql.Key).Count();
                                if (num != 0)
                                {
                                    continue;
                                }
                                listOfChildSqls.Add(new KeyValuePair<string, Sql>(cxql.Key, cxql.Value));
                            }
                            else
                            {

                                if (!cxql.Key.StartsWith(trimmed))
                                {
                                    continue;
                                }

                                if (trimmed.Length + 1 > cxql.Key.Length)
                                {
                                    continue;
                                }

                                String remainWith = cxql.Key.Substring(trimmed.Length + 1);
                                String[] remains = remainWith.Split('_');
                                if (remains.Length != 1)
                                {
                                    continue;
                                }
                                int num = listOfChildSqls.Where(xx => xx.Key == cxql.Key).Count();
                                if (num != 0)
                                {
                                    continue;
                                }
                                listOfChildSqls.Add(new KeyValuePair<string, Sql>(cxql.Key, cxql.Value));
                            }
                        }


                        //if (newlistOfChildSqls.Count() > 0)
                        //{
                        //    listOfChildSqls.Concat(newlistOfChildSqls);
                        //}
                    }

                    
                    env.CurrentHoneyRef = env.Honey;
                    string[] pathParts = dc.pathToCellValue.Split('_');
                  
                    isActuallyQueringOneObject = pathParts[pathParts.Length - 2].StartsWith(Bee.Engine.PathObjectIndicator) && isActuallyQueringOneObject == false;
                    if (isActuallyQueringOneObject == true)
                    {
                        bool whoIsThis = false;
                    }
                    if (isActuallyQueringOneObject && i > 0)
                    {
                        //break;
                    }

                    for (int pi = 0; pi < pathParts.Length ; pi++) 
                    {
                        string pathSegment = pathParts[pi];
                        bool isAList = (pathSegment.StartsWith(Bee.Engine.PathListIndicator)) ? true : false;
                        if(pi + 1 == pathParts.Length){ //i have reached the value segment
                            try
                            {
                                //use this error to know that we need to put this in a new object in side of the first array upwards
                                JToken hasThisToken = ((JObject)env.CurrentHoneyRef)[pathParts.Last()];
                                if (hasThisToken == null)
                                {
                                    if (sql.IsCount)
                                    {
                                        //Role: {
                                        //    vvv is the parent
                                        //    Accesses: [{==>current honey refrence
                                        //        _qCount: {}
                                        //    }]
                                        //}
                                        //oRole_lAccesses_Count ==> oRole
                                        //env.CurrentHoneyRef ==> {}
                                        //env.CurrentHoneyRef.Parent ==> [] ==> Accesses
                                        //env.CurrentHoneyRef.Parent.Parent ==> { Accesses }
                                        //JToken temp = env.CurrentHoneyRef.Parent;
                                        JObject temp = new JObject() { { "Count", new JValue(cellValue) } };
                                        env.CurrentHoneyRef.Parent.Replace(temp);
                                    }
                                    else
                                    {
                                        ((JObject)env.CurrentHoneyRef).Add(pathParts.Last(), new JValue(cellValue));
                                    }
                                }
                                else
                                {
                                    JToken temp = null;
                                    try
                                    {
                                        temp = env.CurrentHoneyRef.Parent;
                                    }catch(Exception ex){
                                        string fff = "";
                                    }

                                    while (temp.Type != JTokenType.Array)
                                    {
                                        temp = temp.Parent;
                                    }
                                    ((JArray)temp).Add(new JObject());
                                    ((JObject)(((JArray)temp).Last())).Add(pathParts.Last(), new JValue(cellValue));
                                    env.CurrentHoneyRef = ((JArray)temp).Last();
                                }

                            }catch(Exception ex){
                                JToken temp = env.CurrentHoneyRef.Parent;
                                try
                                {
                                    while (temp.Type != JTokenType.Array)
                                    {
                                        temp = temp.Parent;
                                    }
                                    ((JArray)temp).Add(new JObject());
                                    ((JObject)(((JArray)temp).Last())).Add(pathParts.Last(), new JValue(cellValue));
                                    env.CurrentHoneyRef = ((JArray)temp).Last();
                                }catch(Exception exx){
                                    bool foo = true;
                                }
                            }
                        }else{
                            string nodeName = pathSegment.Substring(1);
                            JToken node = env.CurrentHoneyRef[nodeName];

                           
                            childrenPathTest += pathSegment + "_";
                            if (node == null && isAList) 
                            {   //this was supposed to be an array but it is not on the ccurrent refrence
                                ((JObject)env.CurrentHoneyRef).Add(nodeName, new JArray());
                                ((JArray)(((JObject)env.CurrentHoneyRef)[nodeName])).Add(new JObject()); //get a refrence to this array and add a new Object
                                env.CurrentHoneyRef = ((JArray)(((JObject)env.CurrentHoneyRef)[nodeName]))[0]; //get a reference to this new object
                            }else if(node == null && isAList ==false) 
                            {   //this was supposed to be an object node
                                ((JObject)env.CurrentHoneyRef).Add(nodeName, new JObject());
                                env.CurrentHoneyRef = ((JObject)env.CurrentHoneyRef)[nodeName];
                            }else if(node != null && isAList){
                                if (((JArray)node).Count() == 0)
                                {   //add a new object
                                    ((JArray)node).Add(new JObject());
                                }
                                //target the last element of the list
                                env.CurrentHoneyRef = ((JArray)node).Last();
                            }else if(node != null && isAList == false){
                                //this is the current refrence
                                env.CurrentHoneyRef = node;
                            }
                        }
                    }//end column for loop
                }//end for loop j


                //check if this objects path has any need for children
                //take into effect the child sql
                //List<KeyValuePair<String, Sql>> listOfChildSqls = env.ChildSqls.Where(csql =>(
                //    csql.Key.StartsWith(childrenPathTest) &&
                //    csql.Key.Substring(childrenPathTest.Length).Split('_').Length == 1 )|| (
                //    csql.Key.StartsWith(Bee.Scents.Zee) && csql.Key.Split('~')[1].StartsWith(childrenPathTest) &&
                //    csql.Key.Split('~')[1].Substring(childrenPathTest.Length).Split('_').Length == 1)
                //).ToList();

               
                
                if (listOfChildSqls != null && listOfChildSqls.Count() > 0)
                {
                    JObject zipBucket = new JObject();
                    foreach (KeyValuePair<String, Sql> childSql in listOfChildSqls)
                    {
                        String path = childSql.Key;
                        if (path.StartsWith(Bee.Scents.Zee))
                        {
                            String[] zeeParts = path.Split('~');
                            String zipItemName = zeeParts[0].Substring(Bee.Scents.Zee.Length);
                            String pathOfExecution = zeeParts[1]; //e.g lMustBuys

                            String[] pathParts = pathOfExecution.Split('_');
                            Sql csql = childSql.Value;


                            //new staff
                            //we need to get the correct current honey reference
                            bool skip = false;
                            JToken honeyPointer = env.Honey;
                            for (int pp = 0; pp < pathParts.Length - 1; pp++)
                            {
                                string pdirty = pathParts[pp];
                                bool isObject = pdirty.StartsWith(Bee.Engine.PathObjectIndicator);
                                string p = pdirty.Substring(1);
                                if (isObject)
                                {
                                    //get a reference to this object
                                    JToken temp = honeyPointer[p];
                                    honeyPointer = temp;
                                }
                                else
                                {
                                    //this is an array so we are interested in the last element in the array
                                    JArray xray = (JArray)(honeyPointer[p]);
                                    if (xray.Count() > 0)
                                    {
                                        JToken temp = xray[xray.Count() - 1];
                                        honeyPointer = temp;
                                    }
                                    else
                                    {
                                        //nyd
                                        //we dont know what to do
                                    }
                                }
                            }
                            JToken prevRefCurrent = env.CurrentHoneyRef;
                            env.CurrentHoneyRef = honeyPointer;
                            //end new staff


                            csql.Prepare(ref env);
                            //check if it has the last part of this childpath
                            string last = pathParts.Last().Substring(1);
                            JToken cp = env.CurrentHoneyRef[last];
                            if (cp == null)
                            {
                                ((JObject)(env.CurrentHoneyRef)).Add(last, new JArray());
                                cp = env.CurrentHoneyRef[last];
                            }
                            JToken formerCurrentHoneyRef = env.CurrentHoneyRef;
                            env.IsEvaluatingCsql = true;
                            Bee.Workers.MakeHoney(csql, ref env, true, path);
                            env.CurrentHoneyRef = formerCurrentHoneyRef;
                            env.IsEvaluatingCsql = false;

                            //the honey has to be kept some where
                            //so of like in a bucket so that that node can be reused
                            //remmber that a zip always is an object
                            JToken jt = zipBucket[last];
                            if (jt == null)
                            {
                                zipBucket.Add(last, new JObject());
                            }
                            //package 
                            //add these results to the bucket
                            JToken extract = env.CurrentHoneyRef[last].DeepClone();
                            //clean this extract
                            Bee.Workers.PackageSpecificHoney(ref extract, ref env);
                            ((JObject)zipBucket[last]).Add(zipItemName, extract);
                            //clear this space
                            ((JObject)env.CurrentHoneyRef).Remove(last);

                            //new staff
                            //env.CurrentHoneyRef = prevRefCurrent;
                            //end new staff
                        }
                        else
                        {
                            String[] pathParts = path.Split('_');
                            Sql csql = childSql.Value;

                            //new staff
                            //we need to get the correct current honey reference
                            bool skip = false;
                            JToken honeyPointer = env.Honey;
                            for (int pp = 0; pp < pathParts.Length - 1; pp++)
                            {
                                string pdirty = pathParts[pp];
                                bool isObject = pdirty.StartsWith(Bee.Engine.PathObjectIndicator);
                                string p = pdirty.Substring(1);
                                if (isObject)
                                {
                                    //get a reference to this object
                                    JToken temp = honeyPointer[p];
                                    honeyPointer = temp;
                                }
                                else
                                {
                                    //this is an array so we are interested in the last element in the array
                                    JArray xray = (JArray)(honeyPointer[p]);
                                    if (xray.Count() > 0)
                                    {
                                        JToken temp = xray[xray.Count() - 1];
                                        honeyPointer = temp;
                                    }
                                    else
                                    {
                                        //nyd
                                        //we dont know what to do
                                    }
                                }
                            }
                            JToken prevRefCurrent = env.CurrentHoneyRef;
                            env.CurrentHoneyRef = honeyPointer;
                            //end new staff

                            csql.Prepare(ref env);
                            //check if it has the last part of this childpath
                            string last = pathParts.Last().Substring(1);
                            JToken cp = env.CurrentHoneyRef[last];
                            if (cp == null)
                            {
                                ((JObject)(env.CurrentHoneyRef)).Add(last, new JArray());
                                cp = env.CurrentHoneyRef[last];
                            }
                            JToken formerCurrentHoneyRef = env.CurrentHoneyRef;
                            //env.CurrentHoneyRef = cp;
                            env.IsEvaluatingCsql = true;
                            Bee.Workers.MakeHoney(csql, ref env, true, path);
                            env.CurrentHoneyRef = formerCurrentHoneyRef;
                            env.IsEvaluatingCsql = false;

                            //new staff
                            env.CurrentHoneyRef = prevRefCurrent;
                            //end new staff

                        }
                    }//end foreach (KeyValuePair<String, Sql> childSql in listOfChildSqls)
                    //package back the zipped staff to the honey
                    foreach (var item in zipBucket)
                    {
                        JToken x = env.CurrentHoneyRef[item.Key];
                        if (x == null)
                        {
                            ((JObject)env.CurrentHoneyRef).Add(item.Key, item.Value);
                        }
                        else
                        {
                            //update
                            ((JObject)env.CurrentHoneyRef)[item.Key].Replace(item.Value);
                        }
                    }
                }


                //this should be the last logic in this loop
                if (isActuallyQueringOneObject == true)
                {
                    //break;
                }
                //
            }//end for loop i

            
            //execiute the aftermaths jellies7
            List<KeyValuePair<string, string>> amfvs = env.AfterMathJellyValues.ToList();
            foreach (KeyValuePair<string, string> amf in amfvs)
            {
                if (env.EatenAfterMathJellyValues.Contains(amf.Key))
                {
                    continue;
                }
                if (hasEmptyResults == true)
                {
                    env.AfterMathJellyValues[amf.Key] = null;
                    env.EatenAfterMathJellyValues.Add(amf.Key);
                }
                else
                {
                    //this has to work with a collection of guys
                    string variableName = amf.Key;
                    string funDef = amf.Value;

                    
                    bool dontSweatIt = Bee.Workers.ProcessAfterMathJelly(amf.Key, funDef, ref env.Honey, amf.Key, "", ref env);
                    if (dontSweatIt == true)
                    {
                        continue;
                    }
                    

                    
                    

                    //nyd
                    //for now this is executing at the first node as the current ref
                    //but the  may in future have a path UserName or attributes may have a path
                    //instead of just UserName ==> User_UserName ==> User.UserName
                    //JToken prevRef = env.CurrentHoneyRef;
                    //string xpatName = "";
                    //foreach (var xpert in env.Honey)
                    //{
                    //    xpatName = xpert.Key;
                    //    break;
                    //}
                    //if (!string.IsNullOrEmpty(xpatName))
                    //{
                    //    JToken tn = env.Honey[xpatName];
                    //    if(tn != null){
                    //        if(tn.Type == JTokenType.Array){
                    //            //JArray tnItems = (JArray)tn;
                    //            //for (int i = 0; i < tnItems.Count(); i++)
                    //            //{
                    //            //    env.CurrentHoneyRef = (JToken)tnItems[i];
                    //            //    string funValue = Bee.Queen.Eat(funDef, ref env);
                    //            //    env.AfterMathJellyValues[amf.Key+i] = funValue;
                    //            //}
                    //            //env.EatenAfterMathJellyValues.Add(amf.Key);
                    //        }else{
                    //            //env.CurrentHoneyRef = (JObject)tn;
                    //            //string funValue = Bee.Queen.Eat(funDef, ref env);
                    //            //env.AfterMathJellyValues[amf.Key] = funValue;
                    //            //env.EatenAfterMathJellyValues.Add(amf.Key);
                    //        }
                    //    } 
                    //}
                    //env.CurrentHoneyRef = prevRef;
                }
            }

            //execute the aftermaths list jellies
            List<KeyValuePair<string, Object>> lamvs = env.AfterMathListJellyValues.ToList();
            foreach (KeyValuePair<string, Object> lam in lamvs)
            {
                if (env.EatenAfterMathListJellyValues.Contains(lam.Key))
                {
                    continue;
                }
                if (hasEmptyResults == true)
                {
                    env.AfterMathListJellyValues[lam.Key] = null;
                    env.EatenAfterMathListJellyValues.Add(lam.Key);
                }
                else
                {
                    string variableName = lam.Key;
                    string funDef = (string)lam.Value;
                    //nyd
                    //for now this is executing at the first node as the current ref
                    //but the  may in future have a path UserName or attributes may have a path
                    //instead of just UserName ==> User_UserName ==> User.UserName
                    JToken prevRef = env.CurrentHoneyRef;
                    string xpatName = "";
                    foreach (var xpert in env.Honey)
                    {
                        xpatName = xpert.Key;
                        break;
                    }
                    if (!string.IsNullOrEmpty(xpatName))
                    {
                        env.CurrentHoneyRef = env.Honey[xpatName];
                    }
                    List<string> funValue = Bee.Queen.EatAlot(funDef, ref env);
                    env.AfterMathListJellyValues[lam.Key] = funValue;
                    env.EatenAfterMathListJellyValues.Add(lam.Key);
                    env.CurrentHoneyRef = prevRef;
                }
            }

            //nyd
            //delete this whole commented block if pacth is woring fine for all use cases in the play.html
            //the dynamic attributes
            //they were added like so: env.FutureDynamicCells.Add(env.CurrentPath + "_" + realCombName  + "_" + trimedPrt);
            //foreach (string item in env.FutureDynamicCells)
            //{
            //    //item e.g oUser_lMustBuys_CPrice
            //    bool dontSweatIt = Bee.Workers.ProcessFutureDynamicCellsOf(ref env.Honey, item, ref env);

            //    if (dontSweatIt == true)
            //    {
            //        continue;
            //    }

            //    string instantCellName = item.Substring(1);
            //    string variableName = Bee.Scents.JellyAfterMath + instantCellName;
            //    dynamic dynVariableValue = env.AfterMathJellyValues[variableName];
            //    string variableValue = dynVariableValue;

            //    if (hasEmptyResults == true)
            //    {
            //        var temp = env.Honey[instantCellName];
            //        if (temp == null)
            //        {
            //            env.Honey.Add(instantCellName, variableValue);
            //        }
            //        continue;
            //    }

            //    //get by path
            //    //value is going inside 
            //    string xpatName = "";
            //    foreach (var xpert in env.Honey)
            //    {
            //        xpatName = xpert.Key;
            //        break;
            //    }
            //    if (string.IsNullOrEmpty(xpatName))
            //    {
            //        var temp = env.Honey[instantCellName];
            //        if (temp == null)
            //        {
            //            env.Honey.Add(instantCellName, variableValue);
            //        }
            //    }
            //    else
            //    {
                    

            //        JToken tn = env.Honey[xpatName];
            //        if (tn != null)
            //        {
            //            if (tn.Type == JTokenType.Array)
            //            {
            //                JArray tnItems = (JArray)tn;
            //                for (int i = 0; i < tnItems.Count(); i++)
            //                {
            //                    variableValue = env.AfterMathJellyValues[variableName + i];
            //                    var temp = ((JObject)tnItems[i])[instantCellName];
            //                    if (temp == null)
            //                    {
            //                        ((JObject)tnItems[i]).Add(instantCellName, variableValue);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                JToken temp = null;
            //                try
            //                {
            //                     temp = ((JObject)tn)[instantCellName];
                                 
            //                     if (temp == null)
            //                     {
            //                         ((JObject)tn).Add(instantCellName, variableValue);
            //                     }
            //                }catch(Exception ex){
            //                    bool wow = true;
            //                }
                            
            //            }
            //        } 
            //    }
            //}

            //check if this is a _qString
            if (sql.IsString == true)
            {
                //need to get the colomn to get values to string together
                String c = sql.StringAs;
                //lBeeChunk_String
                //the parts that has to be strigified has to be a list
                String[] slits = sql.StringAs.Split('_');
                if (slits.Length == 2)
                {
                    string smallPath = slits[0].Substring(1);
                    //get the value field
                    string valueField = slits[1];
                    String ans = "";
                    //get the array
                    JArray recs = (JArray)env.Honey.SelectToken(smallPath);
                    foreach (var item in recs)
                    {
                        JToken v = item[valueField];
                        if (v != null && v.Type != JTokenType.Null)
                        {
                            ans = ans + ((String)v);
                        }
                    }
                    //
                    JObject temp = new JObject() { { "String", new JValue(ans) } };
                    env.Honey[smallPath].Replace(temp);
                }
                else
                {
                    //we need a good strategy
                }
            }

            return innerHoney;
        }

        public static bool ProcessAfterMathJelly(String originalJellyName, String funDef, ref JObject obj, String jellyName, String affix, ref Environment env)
        {
            bool dontSweatIt = false;
            //oUser_lMustBuys_CPrice
            JToken tgt = obj;
            string[] vparts = jellyName.Split('_');
            for (int i = 0; i < vparts.Length; i++)
            {
                string vpart = vparts[i].Substring(1);
                if (i + 1 == vparts.Length)//the value part
                {
                    JToken prevRef = env.CurrentHoneyRef;
                    env.CurrentHoneyRef = tgt;

                    
                    String key = originalJellyName + affix;

                    

                    int li = originalJellyName.LastIndexOf('_');
                    string prevSearchstr = env.searchAfterJellyKey;
                    env.searchAfterJellyKey = originalJellyName.Substring(0,li+1) + "~" + affix;
                    string funValue = Bee.Queen.Eat(funDef, ref env);
                    env.searchAfterJellyKey = prevSearchstr;

                    //check if its required as a future thing
                    //and its not already eaten
                    if (env.FutureDynamicCells.Contains(originalJellyName) && !env.EatenAfterMathJellyValues.Contains(key))
                    {
                        ((JObject)tgt).Add(vparts[i], funValue);//obj.Add(jellyName, funValue);
                        if (!env.AfterMathJellyValues.ContainsKey(key))
                        {
                            env.AfterMathJellyValues.Add(key, funValue);
                        }
                        else
                        {
                            env.AfterMathJellyValues[key] = funValue;
                        }
                        env.EatenAfterMathJellyValues.Add(key);
                    }
                    else if (!env.EatenAfterMathJellyValues.Contains(originalJellyName)) //check if it was not eaten before
                    {
                        env.EatenAfterMathJellyValues.Add(originalJellyName);
                        env.AfterMathJellyValues[originalJellyName] = funValue; //update value
                        if (!env.AfterMathJellyValues.ContainsKey(key)) //the value for this object
                        {
                            env.AfterMathJellyValues.Add(key, funValue);
                        }
                    }
                    else
                    {
                        env.AfterMathJellyValues[originalJellyName] = funValue; //update value
                        if (!env.AfterMathJellyValues.ContainsKey(key)) //the value for this object
                        {
                            env.AfterMathJellyValues.Add(key, funValue);
                        }
                    }
                    env.CurrentHoneyRef = prevRef;
                    
                }
                else
                {
                    JToken xNode = tgt[vpart];
                    if (xNode == null)
                    {
                        //it means honey has not yet acquired this node
                        //so just continue
                        dontSweatIt = true;
                        break;
                    }
                    else
                    {
                        if (xNode.Type == JTokenType.Array)
                        {
                            //each object of this array must continue down this path
                            //as it its way its path it indicates its index an underscore
                            JArray things = (JArray)xNode;
                            string t = ""; //oUser_lMustBuys_CPrice, i == 1, k = 2 , l = 3
                            for (int k = i + 1; k < vparts.Length; k++)
                            {
                                t = t + vparts[k] + "_";
                            }
                            t = t.Trim('_');
                            //this means that i moved forward by one step
                            i = i + 1;
                            for (int j = 0; j < things.Count(); j++)
                            {
                                JObject childObj = (JObject)things[j];
                                bool dst = Bee.Workers.ProcessAfterMathJelly(originalJellyName, funDef, ref childObj, t, affix + "_" + j, ref env);
                                if (dst == true)
                                {  //implies that all the objects in this array will not pass
                                    return dst;
                                }
                            }
                        }
                        else
                        {
                            tgt = xNode;
                        }
                    }
                }
            }
            return dontSweatIt;
        }

        public static bool ProcessFutureDynamicCellsOf(ref JObject obj, String path, ref Environment env)
        {
            string[] pathParts = path.Split('_');
            JToken thisTarget = obj;
            bool dontSweatIt = false;
            for (int i = 0; i < pathParts.Length; i++)
            {
                string pathPart = pathParts[i];
                if (i + 1 == pathParts.Length) //the value part
                {
                    string instantCellName = pathPart.Substring(1);
                    string variableValue = env.AfterMathJellyValues[instantCellName + i];
                    //check if it does not have this value has yet
                    JToken temp = ((JObject)thisTarget)[instantCellName];
                    if (temp == null)
                    {
                        ((JObject)thisTarget).Add(instantCellName, variableValue);
                    }
                }
                else
                {
                    bool isArray = pathPart.StartsWith(Bee.Engine.PathListIndicator);
                    JToken honeyNode = thisTarget[pathPart.Substring(1)];
                    if (honeyNode != null)
                    {

                        //if its an array then every object inthat array must participate
                        if (isArray)
                        {
                            JArray objs = (JArray)honeyNode;
                            //get the rest of the path
                            string t = ""; //oUser_lMustBuys_CPrice, i == 1, k = 2 , l = 3
                            for (int k = i + 1; k < pathParts.Length; k++)
                            {
                                t = t + pathParts[k] + "_";
                            }
                            t = t.Trim('_');
                            for (int j = 0; j < objs.Count(); j++)
                            {
                                JObject objChild = (JObject)objs[j];
                                bool dnt = Bee.Workers.ProcessFutureDynamicCellsOf(ref objChild, t, ref env);
                                //nyd
                                //what should wee do about this bool
                            }
                        }
                        else
                        {
                            thisTarget = honeyNode;
                        }
                    }
                    else
                    {
                        //it means that furtheer traversal of this path does not make scence
                        dontSweatIt = true;
                        break;
                    }
                }
            }
            return dontSweatIt;
        }

        public static List<string> TurnNectorIntoHoney(Bee.Concentrate csql, ref JObject honey, bool isFlowerCall)
        {
            List<string> invisiblePaths = new List<string>();
            if (csql == null)
            {
                return invisiblePaths;
            }

            var conn = Bee.Hive.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            bool resultsAreNull = false;
            using (SqlCommand sqlCmd = new SqlCommand(csql.sql, conn))
            {

                SqlDataReader reader = sqlCmd.ExecuteReader();

                if (reader == null || reader.HasRows == false)
                {
                    //bRet = false;
                    resultsAreNull = true;
                }
                else
                {
                   invisiblePaths = Bee.Workers.Brew(reader, ref honey, csql.childrenCsqls, csql.ConcetrateTrap);
                }

                //close the reader
                if (reader != null && reader.IsClosed == false)
                {
                    reader.Close();
                }
            }

            //nyd
            //execiute the aftermaths
            //what are my aftermaths
            if (csql != null && csql.ConcetrateTrap != null && csql.ConcetrateTrap.afterMathFunctions != null &&
            csql.ConcetrateTrap.afterMathFunctions.Count() > 0 )
            {
                foreach (KeyValuePair<string, string> amf in csql.ConcetrateTrap.afterMathFunctions)
                {
                    if (resultsAreNull == false)
                    {
                        string variableName = amf.Key;
                        string funDef = amf.Value;
                        string funValue = Bee.Queen.Lay(funDef, csql.ConcetrateTrap.ExternalParameter, honey, isFlowerCall);
                        csql.ConcetrateTrap.ExternalParameter.Add(amf.Key, funValue);
                    }
                    else
                    {
                        csql.ConcetrateTrap.ExternalParameter.Add(amf.Key, null);
                    }
                }
            }
            

            return invisiblePaths;
        }

        public static List<string> TurnNectorIntoHoney(Bee.Concentrate csql, ref JObject honey, JObject _vSource)
        {
            List<string> invisiblePaths = new List<string>();
            var conn = Bee.Hive.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            string sql = csql.sql;
            //split the sql 
            string []sqlParts = sql.Split(' ');
            string dilutedSql = ""; 
            for (int i = 0; i < sqlParts.Length; i++)
            {
                string sqlPart = sqlParts[i];
                if (!String.IsNullOrEmpty(sqlPart))
                {
                    string trimedSqlPart = sqlPart.Trim();
                    if (trimedSqlPart.StartsWith("_v"))
                    {
                        string cellName = trimedSqlPart.Substring(2);
                        //nyd
                        //-v deep lookup and linking
                        //for now we only lookup at the root attributes of the _Source
                        dynamic whereNodeDynamicValue = _vSource[cellName];
                        string whereNodeValue = whereNodeDynamicValue;
                        string quotedValue = Bee.Hive.GetQuoted(whereNodeValue, _vSource[cellName].Type);
                        //replace in the sql parts
                        dilutedSql = dilutedSql + " " + quotedValue + " ";
                    }
                    else
                    {
                        dilutedSql = dilutedSql + " " + sqlPart;
                    }
                }else{
                    dilutedSql = dilutedSql + " ";
                }
            }

            using (SqlCommand sqlCmd = new SqlCommand(dilutedSql, conn))
            {

                SqlDataReader reader = sqlCmd.ExecuteReader();

                if (reader == null || reader.HasRows == false)
                {
                    //bRet = false;
                }
                else
                {
                    invisiblePaths = Bee.Workers.Brew(reader, ref honey, csql.childrenCsqls, null);
                }

                //close the reader
                if (reader != null && reader.IsClosed == false)
                {
                    reader.Close();
                }
            }

            return invisiblePaths;
        }

        


        public static void Brew(SqlDataReader reader, ref JObject honey)
        {
            //how many columns does our haave
            int numberOfColumns = reader.FieldCount;
            while (reader.Read())
            {
                bool goToAnewRow = true;
                for (int columnIndex = 0; columnIndex < numberOfColumns; columnIndex++)
                {
                    string pathToCellValue = reader.GetName(columnIndex);
                    Object cellValue = reader.GetValue(columnIndex);
                    string[] pathParts = pathToCellValue.Split('_');
                    //reference the whole honey object
                    JToken currentHoneyNode = honey;
                    //follow the path to this cell value by going through the parts 
                    //of the path
                    for (int pathIndex = 0; pathIndex < pathParts.Length; pathIndex++)
                    {
                        string pathSegment = pathParts[pathIndex];
                        //check if we have reached the last part of the journey
                        //at the last point, its the cellName
                        if (pathIndex == pathParts.Length - 1)
                        {
                            Bee.Workers.Stire(ref currentHoneyNode, ref goToAnewRow, pathSegment, cellValue);
                        }
                        else
                        {
                            Bee.Workers.Toast(ref currentHoneyNode, pathSegment);
                        }
                    }
                }//end for (int columnIndex = 0; ...
            }//end while (reader.Read())
        }//end brew

        public static List<string> Brew(SqlDataReader reader, ref JObject honey, Dictionary<string, Concentrate> childrenCsqls, Trap trap)
        {
            List<string> invisiblePaths = new List<string>();
            //how many columns does our haave
            int numberOfColumns = reader.FieldCount;
            while (reader.Read())
            {
                bool goToAnewRow = true;
                //these will have the comb attributes as well as attributes for the parent comb
                //so after the end of this loop both parents and comb will be filled in
                for (int columnIndex = 0; columnIndex < numberOfColumns; columnIndex++)
                {
                    string pathToCellValue = reader.GetName(columnIndex);
                    if (trap != null && trap.TreatAsInvisible != null && trap.TreatAsInvisible.Count() > 0
                        && trap.TreatAsInvisible.Contains(pathToCellValue))
                    {
                        continue;
                    }
                    Object cellValue = reader.GetValue(columnIndex);
                    string[] pathParts = pathToCellValue.Split('_');
                    //reference the whole honey object
                    JToken currentHoneyNode = honey;
                    //follow the path to this cell value by going through the parts 
                    //of the path
                    for (int pathIndex = 0; pathIndex < pathParts.Length; pathIndex++)
                    {
                        string pathSegment = pathParts[pathIndex];
                        //check if we have reached the last part of the journey
                        //at the last point, its the cellName
                        if (pathIndex == pathParts.Length - 1)
                        {
                            string cmbName =  pathParts[pathParts.Length-2].Substring(1);
                            bool isInvisible = Bee.Hive.isInvisible(pathSegment, cmbName);
                            if (isInvisible == true && !invisiblePaths.Contains(pathToCellValue))
                            {
                                invisiblePaths.Add(pathToCellValue);
                            }
                            Bee.Workers.Stire(ref currentHoneyNode, ref goToAnewRow, pathSegment, cellValue);
                        }
                        else
                        {
                            Bee.Workers.Toast(ref currentHoneyNode, pathSegment);
                        }
                    }
                }//end for (int columnIndex = 0; ...
                //work on its children
                if (childrenCsqls != null && childrenCsqls.Count() > 0)
                {
                    
                    foreach (KeyValuePair<string, Concentrate> childCsql in childrenCsqls)
                    {
                        //travel to this location in honey
                        JToken currentHoneyNode = honey;
                        string[] pathParts = childCsql.Key.Split('_');
                        string pastHistory = "";
                        for (int pathIndex = 0; pathIndex < pathParts.Length; pathIndex++)
                        {
                            string pathSegment = pathParts[pathIndex];
                            //check if we have reached the last part of the journey
                            //at the last point, its the child node key
                            if (pathIndex == pathParts.Length - 1)
                            {
                                string nodeName = pathSegment.Substring(1);
                                //the currentHoneyNode is going to be an array
                                //which means that the last member of this array must get this node 
                                //which is an array node
                                
                                
                                if (currentHoneyNode.Type == JTokenType.Array)
                                {
                                    JArray parents = (JArray)currentHoneyNode;
                                    if (parents.Count() > 0)
                                    {
                                        JObject parent = (JObject)parents[parents.Count() - 1];
                                        JObject childrenHoney = new JObject();
                                        List<string> invisibleChildrenPaths = Bee.Workers.TurnNectorIntoHoney(childCsql.Value, ref childrenHoney, parent);
                                        string cleanedPastHistory = pastHistory.StartsWith("_") ? pastHistory.Substring(1) : pastHistory;
                                        foreach (string pathx in invisibleChildrenPaths)
                                        {
                                            string fullPathX = cleanedPastHistory + "_" + pathx;
                                            if (!invisiblePaths.Contains(fullPathX))
                                            {
                                                invisiblePaths.Add(fullPathX);
                                            }
                                        }
                                        JToken jt = childrenHoney[nodeName];
                                        if (jt == null)
                                        {
                                            parent.Add(nodeName, new JArray());
                                        }
                                        else
                                        {
                                            parent.Add(nodeName, jt);
                                        }
                                    }
                                }
                                else
                                {
                                    JObject parent = (JObject)currentHoneyNode;
                                    JObject childrenHoney = new JObject();
                                    List<string> invisibleChildrenPaths = Bee.Workers.TurnNectorIntoHoney(childCsql.Value, ref childrenHoney, parent);
                                    string cleanedPastHistory = pastHistory.StartsWith("_") ? pastHistory.Substring(1) : pastHistory;
                                    foreach (string pathx in invisibleChildrenPaths)
                                    {
                                        string fullPathX = cleanedPastHistory + "_" + pathx;
                                        if (!invisiblePaths.Contains(fullPathX))
                                        {
                                            invisiblePaths.Add(fullPathX);
                                        }
                                    }
                                    JToken jt = childrenHoney[nodeName];
                                    if (jt == null)
                                    {
                                        parent.Add(nodeName, new JArray());
                                    }
                                    else
                                    {
                                        parent.Add(nodeName, jt);
                                    }
                                }
                            }
                            else
                            {
                                //bool isArray = (pathSegment.StartsWith("a")) ? true : false;
                                string nodeName = pathSegment.Substring(1);
                                currentHoneyNode = currentHoneyNode[nodeName];
                            }
                            pastHistory = pastHistory + "_" + pathSegment;
                        }
                        //if (!csqlChildren.sql.Contains("WHERE"))
                        //{
                        //    csqlChildren.sql = csqlChildren.sql + " WHERE ";
                        //}
                        //csqlChildren.sql = csqlChildren.sql + " " + childCombName + "." + nodeKey + "Id_e _r";
                        ////decorate
                        //if (!String.IsNullOrEmpty(childCombName))
                        //{
                        //    string decorationResults = Bee.Workers.Decorate(csqlChildren.sql, childCombName);
                        //    csqlChildren.sql = decorationResults;
                        //}
                        ////the spicing
                        //csqlChildren.sql = Bee.Workers.Spice(csqlChildren.sql);
                        //Bee.Workers.TurnNectorIntoHoney(childCsql.Value, ref childrenHoney);

                    }

                }
            }//end while (reader.Read())
            return invisiblePaths;
        }//end brew

        public static void Stire(ref JToken honeyNode, ref bool goToAnewRow, string cellName, Object cellValue)
        {
            if (honeyNode.Type == JTokenType.Array)
            {
                //get a refrence to the array
                JArray ja = (JArray)honeyNode;
                if (ja.Count() == 0 || goToAnewRow == true)
                {
                    goToAnewRow = false;
                    //add a new node to this array
                    JObject newNode = new JObject();
                    JValue jv = new JValue(cellValue);
                    newNode.Add(cellName, jv);
                    ja.Add(newNode);
                }
                else
                {
                    //we add a cell to the object at the last index of this array
                    //because we are always editing the object at the last index 
                    //of the array
                    JValue jv = new JValue(cellValue);
                    ((JObject)(ja[ja.Count() - 1])).Add(cellName, jv);
                }
            }
            else //its an object so we put the the cell value
            {
                JValue jv = new JValue(cellValue);
                ((JObject)honeyNode).Add(cellName, jv);
            }
        }

        public static bool HasNode(JToken node, string nodeKey)
        {
            try
            {
                var xtemp = node[nodeKey];
                if (xtemp == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void Toast(ref JToken honeyNode, string pathSegment)
        {
            //check if its array or object
            bool isArray = (pathSegment.StartsWith("a")) ? true : false;
            string nodeName = pathSegment.Substring(1);
            bool nodeFound = Bee.Workers.HasNode(honeyNode, nodeName);
            if (isArray && nodeFound == false)
            {
                //add a new node here that is an array
                ((JObject)honeyNode).Add(nodeName, new JArray());
                //move the refrence to point to this new array
                honeyNode = honeyNode[nodeName];
            }
            else if (isArray && nodeFound == true)
            {
                //make the current refrence to this aray
                honeyNode = honeyNode[nodeName];
            }
            else if (isArray == false && nodeFound == false) //its supposed to be an array here but it was not found
            {
                //but if we find that the curent type is an array
                //we convert it to an object and add a new node to the object
                if (honeyNode.Type != JTokenType.Array)
                {
                    //then we just add a new object into the array
                    ((JObject)honeyNode).Add(nodeName, new JObject());
                    honeyNode = ((JObject)honeyNode)[nodeName];
                }
                else if (honeyNode.Type == JTokenType.Array)
                {
                    //going for the last item inside this array
                    JArray ja = (JArray)honeyNode;
                    if (ja.Count() == 0)
                    {
                        //add a new object
                        JObject njob = new JObject();
                        njob.Add(nodeName, new JObject());
                        ja.Add(njob);
                    }
                    //we are editing the last entry
                    bool subNodeFound = Bee.Workers.HasNode((JObject)(ja[ja.Count() - 1]), nodeName);
                    if (subNodeFound == false)
                    {
                        ((JObject)(ja[ja.Count() - 1])).Add(nodeName, new JObject());
                    }
                    //navigat to this last object in the array
                    honeyNode = (JObject)(ja[ja.Count() - 1])[nodeName];
                }

            }
            else if (isArray == false && nodeFound == true)
            {
                //make the current refrence to this aray
                honeyNode = honeyNode[nodeName];
            }
        }

        /// <summary>
        /// The process of turning a root node into a valid sql statment in a get honey request is called Fermentation
        /// Dispatches: [{
        ///    a: "DispatchId SalesAgentId Date CashAmount Note DispatchState",
        ///    SalesAgent: {
        ///        a : "SalesAgentId FirstName lastName Gender PhoneNumber"
        ///    },
        ///    PriceGroup: {
        ///        a: "PriceGroupId Name Description"
        ///    }
        /// }]
        /// 
        /// <param name="jsonObject">
        /// {
        ///    a: "DispatchId SalesAgentId Date CashAmount Note DispatchState",
        ///    SalesAgent: {
        ///        a : "SalesAgentId FirstName lastName Gender PhoneNumber"
        ///    },
        ///    PriceGroup: {
        ///        a: "PriceGroupId Name Description"
        ///    },
        ///    SomeLists: [{
        ///        a: "PriceGroupId Name Description"
        ///    }]
        /// }
        /// </param>
        /// <param name="combName">Dispatch</param>
        /// <param name="combNodeKey">Dispatches</param>
        /// </summary>
        public static Bee.Concentrate Ferment(JObject jsonObject, string combName, string combNodeKey, string path, JObject externalParameters, bool isFlowerCall)
        {
            
            Bee.Concentrate concentratedSql = new Concentrate(){
                sql = "",
                childrenCsqls = new Dictionary<string, Concentrate>() 
            };

            string sql = "SELECT ";
            string innerJoinSql = "";
            string whereSql = "";

            Trap trap = new Trap();
            trap.ExternalParameter = externalParameters;
            Bee.Workers.PrepareAttributes(ref jsonObject, combName, ref trap);

          
            

            //go through all the nodes of this object           
            foreach (var node in jsonObject)
            {
                //get the node key
                string nodeKey = node.Key;
                //check if this node key smells like an attribute scent
                if (nodeKey.Equals(Bee.Scents.Attribute))
                {
                    path = path + combNodeKey;
                    string tempSqlResults = Bee.Workers.FermetAttributeScent(node, combName, path);
                    string trimedSelect = tempSqlResults.Trim();
                    if (string.IsNullOrEmpty(trimedSelect))
                    {
                        tempSqlResults = "";
                        //it means that there was no cellName selected
                        //this causes errors so we choose the first cell
                        //of this comb and we shall delete it laiter
                        string celName = Bee.Hive.GetFirstCellName(combName);
                        string inviPath = path + "_" + celName;
                        tempSqlResults = tempSqlResults + " [" + combName + "].[" + celName + "] AS " + inviPath + " ";
                        trap.TreatAsInvisible.Add(inviPath);
                    }
                    sql = sql + tempSqlResults;
                }
                else if (nodeKey.StartsWith("_f") && !nodeKey.StartsWith("_f_"))
                {
                    //this function is executed immediately within this context
                    //"_fEncDesc": "encrypt _@desc _mySweetHoney",
                    string variableName = nodeKey.Substring(2);
                    dynamic dynFunctionDef = node.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen lay these eggs
                    string value = Bee.Queen.Lay(eggsDef, externalParameters, null, isFlowerCall);
                    trap.immediateFunctionValues.Add(nodeKey, value);
                }
                else if (nodeKey.StartsWith("_f_"))
                {
                    //this function is executed after the query has returned data and this object 
                    //has been filled in
                    string variableName = nodeKey.Substring(2);
                    dynamic dynFunctionDef = node.Value;
                    string afterMathDef = dynFunctionDef;
                    trap.afterMathFunctions.Add(nodeKey, afterMathDef);
                }
                else if (nodeKey.Equals(Bee.Scents.Where)) //node key smells like a where scent
                {
                    JObject whereJsonObject = (JObject)node.Value;
                    string tempWhereSqlResults = Bee.Workers.FermetWhereScent(whereJsonObject, ref trap, combName);
                    whereSql = whereSql + tempWhereSqlResults;
                }
                else if (nodeKey.StartsWith(Bee.Scents.Zee)) //buzz , zzzz, zzz ...
                {

                }
                else if (node.Value.Type == JTokenType.Array) //if the node value is an array e.g SomeLists
                {

                    //Bee.NodeObject no = Bee.Workers.HandleRootNodeCollectionArray(node, nodeKey, combName, path);
                    //string ccPartialSqlRes = no.PartialSql;
                    //sql = sql + ccPartialSqlRes;

                    path = ((path.Equals("a")) ? path : path + "_a") + nodeKey;
                    String childCombName = Bee.Hive.GetRealCombName(nodeKey);
                    JArray childArray = (JArray)node.Value;
                    Bee.Concentrate csqlChildren = null;
                    foreach (JObject nectorRootNode in childArray)
                    {
                        //check if it has a where
                        bool hasWhere = Bee.Workers.HasNode(nectorRootNode, "_w");
                        if (hasWhere == true)
                        {
                            ((JObject)nectorRootNode["_w"]).Add("_andb_[dbo].[" + childCombName + "].[" + combName + "Id_e", "_v" + combName + "Id ");
                            ((JObject)nectorRootNode["_w"]).Add("_d_", "_s");
                        }
                        else
                        {
                            JObject whereNode = new JObject();
                            whereNode.Add("[dbo].[" + childCombName + "].[" + combName + "Id_e", "_v" + combName + "Id ");
                            //add a where
                            nectorRootNode.Add("_w", whereNode);
                        }
                        csqlChildren = Bee.Workers.Ferment(nectorRootNode, childCombName, nodeKey, "a", externalParameters, isFlowerCall);
                        break;
                    }
                    concentratedSql.childrenCsqls.Add(path, csqlChildren);

                }
                else if (node.Value.Type == JTokenType.Object)
                { //if this node is an object
                    string pathToUse = ((path.Equals("o")) ? path : path + "_o") + nodeKey;
                    string nodeCombName = Bee.Hive.GetRealCombName(nodeKey);
                    JObject parentNodeObject = (JObject)node.Value;
                    Bee.NodeObject no = Bee.Workers.ProcessParentJObject(parentNodeObject, nodeCombName, nodeKey, combName, pathToUse, ref trap);
                    sql = sql + no.PartialSql;
                    innerJoinSql = innerJoinSql + no.InnerJoin;
                }
                else
                {
                    throw new Exception("unrecognised node, no implementation yet");
                }
            }

            sql = sql.Trim(',');

            sql = sql + " FROM [dbo].[" + combName + "] ";
            sql = sql + innerJoinSql;
            whereSql = whereSql.Trim();
            if (!String.IsNullOrEmpty(whereSql))
            {
                sql = sql + " WHERE " + whereSql;
            }
            concentratedSql.sql = sql;

            concentratedSql.ConcetrateTrap = trap;

            return concentratedSql;
        }


        public static void cleanHoney(List<string> pathsToClean, ref JObject honey)
        {
            for (int i = 0; i < pathsToClean.Count; i++)
            {
                JToken currentRef = honey;
                string path = pathsToClean[i];
                string []pathParts = path.Split('_');
                //bool isFormerNodeAnArray = false;
                for (int j = 0; j < pathParts.Length; j++ )
                {
                    if (currentRef == null)
                    {
                        continue;    
                    }
                    string pathPart = pathParts[j];
                    if (j + 1 == pathParts.Length)//at the last part
                    {
                        if (currentRef.Type == JTokenType.Array)
                        {
                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)currentRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                ((JObject)list[k]).Remove(pathPart);
                            }
                        }
                        else
                        {
                            ((JObject)currentRef).Remove(pathPart);
                        }
                    }
                    else
                    {
                        
                        if (currentRef.Type == JTokenType.Array)
                        {
                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)currentRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                JObject listItem = (JObject)list[k];
                                string pathRemainder = "";
                                for (int l = j; l < pathParts.Length; l++)
                                {
                                    pathRemainder =  pathRemainder + "_" + pathParts[l];
                                }
                                pathRemainder = pathRemainder.Substring(1);//remove the leading _
                                List<string> childPathsToClean = new List<string>() { pathRemainder };
                                Bee.Workers.cleanHoney(childPathsToClean, ref listItem);
                            }
                            //by the end of this loop the whole path would have traversed to the end
                            j = pathParts.Length;
                        }
                        else
                        {
                            string nodeName = pathPart.Substring(1);
                            currentRef = currentRef[nodeName];
                        }
                    }
                }
            }
        }

        public static void PackageSpecificHoney(ref JToken sampleHoney, ref Environment env)
        {
            JToken currentRef = null;
            for (int i = 0; i < env.GeneratedInvisiblePaths.Count; i++)
            {
                currentRef = sampleHoney;
                if (currentRef == null)
                {
                    continue;
                }
                string path = env.GeneratedInvisiblePaths[i];
                string[] pathParts = path.Split('_');
                //bool isFormerNodeAnArray = false;
                for (int j = 0; j < pathParts.Length; j++)
                {
                    if (currentRef == null)
                    {
                        continue;
                    }
                    string pathPart = pathParts[j];
                    if (j + 1 == pathParts.Length)//at the last part
                    {
                        if (currentRef.Type == JTokenType.Array)
                        {

                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)currentRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                ((JObject)list[k]).Remove(pathPart);
                            }
                        }
                        else
                        {
                            ((JObject)currentRef).Remove(pathPart);
                        }
                    }
                    else
                    {

                        if (currentRef.Type == JTokenType.Array)
                        {
                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)currentRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                JObject listItem = (JObject)list[k];
                                string pathRemainder = "";
                                for (int l = j; l < pathParts.Length; l++)
                                {
                                    pathRemainder = pathRemainder + "_" + pathParts[l];
                                }
                                pathRemainder = pathRemainder.Substring(1);//remove the leading _
                                List<string> childPathsToClean = new List<string>() { pathRemainder };
                                Bee.Workers.cleanHoney(childPathsToClean, ref listItem);  
                            }
                            //by the end of this loop the whole path would have traversed to the end
                            j = pathParts.Length;
                        }
                        else
                        {
                            string nodeName = pathPart.Substring(1);
                            var t = currentRef[nodeName];
                            if (t != null)
                            {
                                currentRef = currentRef[nodeName];
                            }
                            else
                            {
                                //the reset of this path parts are use less
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void Package(ref Environment env)
        {
            for (int i = 0; i < env.GeneratedInvisiblePaths.Count; i++)
            {
                env.CurrentHoneyRef = env.Honey;
                if (env.CurrentHoneyRef == null)
                {
                    continue;
                }
                string path = env.GeneratedInvisiblePaths[i];
                string[] pathParts = path.Split('_');
                //bool isFormerNodeAnArray = false;
                for (int j = 0; j < pathParts.Length; j++)
                {
                    if (env.CurrentHoneyRef == null)
                    {
                        continue;
                    }
                    string pathPart = pathParts[j];
                    if (j + 1 == pathParts.Length)//at the last part
                    {
                        if (env.CurrentHoneyRef.Type == JTokenType.Array)
                        {
                            
                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)env.CurrentHoneyRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                ((JObject)list[k]).Remove(pathPart);
                            }
                        }
                        else
                        {
                            ((JObject)env.CurrentHoneyRef).Remove(pathPart);
                        }
                    }
                    else
                    {

                        if (env.CurrentHoneyRef.Type == JTokenType.Array)
                        {
                            //all the objects inside this array will need to be cleaned
                            JArray list = (JArray)env.CurrentHoneyRef;
                            for (int k = 0; k < list.Count(); k++)
                            {
                                JObject listItem = (JObject)list[k];
                                string pathRemainder = "";
                                for (int l = j; l < pathParts.Length; l++)
                                {
                                    pathRemainder = pathRemainder + "_" + pathParts[l];
                                }
                                pathRemainder = pathRemainder.Substring(1);//remove the leading _
                                List<string> childPathsToClean = new List<string>() { pathRemainder };
                                Bee.Workers.cleanHoney(childPathsToClean, ref listItem);
                            }
                            //by the end of this loop the whole path would have traversed to the end
                            j = pathParts.Length;
                        }
                        else
                        {
                            string nodeName = pathPart.Substring(1);
                            var t = env.CurrentHoneyRef[nodeName];
                            if (t != null)
                            {
                                env.CurrentHoneyRef = env.CurrentHoneyRef[nodeName];
                            }
                            else
                            {
                                //the reset of this path parts are use less
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="nectorStructure"></param>
        /// <param name="realCombName"></param>
        /// <returns>A list of cells that will be added laiter, these begin with +</returns>
        public static void PrepareCells(ref JObject nectorStructure, string realCombName, string thisCombsNameInTheNode, ref Environment env)
        {
            if (env.IsPosting == true)
            {
                return;
            }

            //check if it has an attribute node
            dynamic temp = nectorStructure[Bee.Scents.Attribute]; 
            if (temp == null)
            {
                //get all attributes of this comb
                string cellNames = Bee.Hive.GetAttributes(realCombName);
                //add an attribute node
                //get reference to first object
                JObject newRef = new JObject();
                newRef.Add(Bee.Scents.Attribute, cellNames);
                foreach (var refTemp in nectorStructure)
                {
                    newRef.Add(refTemp.Key, refTemp.Value);
                }
                nectorStructure = newRef;
            }
            else
            {
                string attrValue= (string)nectorStructure[Bee.Scents.Attribute];
                attrValue = attrValue.Trim();
                //if the attribute value contains some special editing
                if (attrValue.Contains(Bee.Scents.Everything) || attrValue.Contains(Bee.Scents.Minus) || attrValue.Contains(Bee.Scents.Plus))
                {
                    //will not come back with thos that have a minus(-)
                    string cellNames = Bee.Hive.GetAttributes(realCombName, attrValue);
                    //jsonObject.Remove(Bee.Scents.Attribute);
                    //jsonObject.Add(Bee.Scents.Attribute, cellNames);
                    nectorStructure[Bee.Scents.Attribute] = cellNames;
                    if (attrValue.Contains(Bee.Scents.Plus))
                    {
                        //some thinhg that will come in the future
                        string[] attParts = attrValue.Split(' ');
                        for (int i = 0; i < attParts.Length; i++)
                        {
                            string trimedPrt = attParts[i].Trim();
                            if (String.IsNullOrEmpty(trimedPrt))
                            {
                                continue;
                            }
                            if (trimedPrt.StartsWith(Bee.Scents.Plus) && trimedPrt.Contains(Bee.Scents.As))
                            { //+OrderYear->Year_OrderDate   ..... YEAR(orderdate) AS orderyear
                                string[] parts = trimedPrt.Replace(Bee.Scents.As, " ").Split(' '); //+OrderYear Year_OrderDate
                                String []funcParts = parts[1].Split('_');
                                string parmz = ""; //" [dbo].[Orders].[OrderDate] "
                                for (int z = 1; z < funcParts.Length; z++)
                                {
                                    parmz = parmz + "[dbo].[" + realCombName.Trim() + "].[" + funcParts[z].Trim() + "],";
                                }
                                parmz = parmz.Trim(',');
                                String rez = "_q" + parts[0].Replace("+","") + "_" + funcParts[0].ToUpper() + "(" + parmz + ")";
                                //_qOrderYear_YEAR(OrderDate)
                                nectorStructure[Bee.Scents.Attribute] = ((String)(nectorStructure[Bee.Scents.Attribute])) + " " + rez;
                            }
                            else if (trimedPrt.StartsWith(Bee.Scents.Plus))
                            {
                                //env.FutureDynamicCells.Add(trimedPrt);//env.CurrentPath + "_" + realCombName  + "_" + fu
                                string fp = env.CurrentPath + thisCombsNameInTheNode + "_" + trimedPrt.Substring(1);
                                env.FutureDynamicCells.Add(fp);
                            }
                        }
                    }
                }
                else
                {
                    nectorStructure[Bee.Scents.Attribute] = attrValue;
                }
            }
        }


        public static void PrepareAttributes(ref JObject jsonObject, string combName, ref Bee.Trap trap)
        {
            //check if it has an attribute node
            bool hasAttribute = Bee.Workers.HasNode(jsonObject, Bee.Scents.Attribute);
            if (hasAttribute == false)
            {
                //get all attributes of this comb
                string cellNames = Bee.Hive.GetAttributes(combName);
                //add an attribute a 
                //get reference to first object
                JObject newRef = new JObject();
                newRef.Add(Bee.Scents.Attribute, cellNames);
                foreach (var refTemp in jsonObject)
                {
                    newRef.Add(refTemp.Key, refTemp.Value);
                }
                jsonObject = newRef;
            }
            else
            {
                dynamic attrDynamicValue = jsonObject[Bee.Scents.Attribute];
                string attrValue = attrDynamicValue;
                attrValue = attrValue.Trim();
                //if the attribute value contains some special editing
                if (attrValue.Contains("*") || attrValue.Contains("-") || attrValue.Contains("+"))
                {
                    string cellNames = Bee.Hive.GetAttributes(combName, attrValue);
                    //jsonObject.Remove(Bee.Scents.Attribute);
                    //jsonObject.Add(Bee.Scents.Attribute, cellNames);
                    jsonObject[Bee.Scents.Attribute] = cellNames;
                    if (attrValue.Contains("+"))
                    {
                        //some thinhg that will come in the future
                        string []attParts = attrValue.Split(' ');
                        for (int i = 0; i < attParts.Length; i++)
                        {
                            string trimedPrt = attParts[i].Trim();
                            if (String.IsNullOrEmpty(trimedPrt))
                            {
                                continue;
                            }
                            if (trimedPrt.StartsWith("+"))
                            {
                                if (trap == null)
                                {
                                    trap = new Trap();
                                }
                                trap.Attributes.Add(trimedPrt);
                            }
                        }
                    }
                }
                else
                {
                    jsonObject[Bee.Scents.Attribute] = attrValue;
                }
            }
        }

        /// <summary>
        /// The active one
        /// </summary>
        /// <param name="nectorStructure"></param>
        /// <param name="realCombName"></param>
        /// <param name="combName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static Bee.Sql Ferment(JObject nectorStructure, string realCombName, string combName, ref Environment env)
        {
            Bee.Sql sql = new Bee.Sql();
            sql.realCombName = realCombName;
            //prepare the cells that this structre is looking for
            Bee.Workers.PrepareCells(ref nectorStructure, realCombName, combName, ref env);


         
            env.CurrentPath += combName;

            if (!String.IsNullOrEmpty(env.CurrentChildRealCombName))
            {
                //INNER JOIN ParentComb ON childCombName.ParentCombId = ParentComb.ParentCombId
                sql.InnerJoin = " INNER JOIN [dbo].[" + realCombName + "] ON ";
                string pkName = Bee.Hive.GetPK(realCombName);
                sql.InnerJoin += "  [dbo].[" + env.CurrentChildRealCombName + "].[" + pkName + "] = ";
                sql.InnerJoin += "  [dbo].[" + realCombName + "].[" + pkName + "] ";
                env.CurrentChildRealCombName = "";
            }

            if (!String.IsNullOrEmpty(env.CurrentParentRealCombName))
            {   
                
                string pkName =  Bee.Hive.GetPK(env.CurrentParentRealCombName);
                string valuablePath = Bee.Scents.Valuable + env.CurrentPath.Substring(0,env.CurrentPath.Length - combName.Length - 1).Replace('_', '.');
                valuablePath += (valuablePath.EndsWith(".") ? "" : ".") + pkName;
                //check if this child structure  has a where node
                JObject where = (JObject)nectorStructure[Bee.Scents.Where];
                if (where != null)
                {
                    ((JObject)nectorStructure[Bee.Scents.Where]).Add("_andb_[dbo].[" + realCombName + "].[" + pkName + "_e", valuablePath + " ");
                    ((JObject)nectorStructure[Bee.Scents.Where]).Add("_d_", Bee.Scents.Space);
                }
                else
                {
                    JObject whereNode = new JObject();
                    whereNode.Add("[dbo].[" + realCombName + "].[" + pkName + "_e", valuablePath + " ");
                    //add a where
                    nectorStructure.Add(Bee.Scents.Where, whereNode);
                }
                env.CurrentParentRealCombName = "";
            }

            //go through each node because the loop gives us a better paradigm 
            foreach (var node in nectorStructure)
            {
                if (node.Key.Equals("_errors"))
                {
                    continue;
                }

                //attribute/cells select sql
                sql.Select += (node.Key.Equals(Bee.Scents.Attribute)) ? FermetCellsAttributeScent((string)node.Value, realCombName, ref env) : "";

                //immmediate jelly _j
                if (node.Key.StartsWith(Bee.Scents.JellyImmediate) && !node.Key.StartsWith(Bee.Scents.JellyAfterMath))
                {
                    //because of zeeping we need to check for existence so that we just do an update
                    if (!env.ImmediateJellyValues.ContainsKey(node.Key))
                    {
                        env.ImmediateJellyValues.Add(node.Key, Bee.Queen.Eat((String)node.Value, ref env));
                    }
                    else
                    {
                        env.ImmediateJellyValues[node.Key] = Bee.Queen.Eat((String)node.Value, ref env);
                    }
                }
                else if (node.Key.StartsWith(Bee.Scents.JellyAfterMath)) //AfterMath jelly _j_
                {   //this jelly is made after the query has returned data and this object has been filled in
                    string xxx=env.CurrentPath + "_" + node.Key.Substring(Bee.Scents.JellyAfterMath.Length);
                    env.AfterMathJellyValues.Add(xxx, (string)node.Value);
                }
                if (node.Key.StartsWith(Bee.Scents.JellyImmediateList) && !node.Key.StartsWith(Bee.Scents.JellyAfterMathList))
                {
                    env.ImmediateListJellyValues.Add(node.Key, Bee.Queen.EatAlot((String)node.Value, ref env));
                }
                else if (node.Key.StartsWith(Bee.Scents.JellyAfterMathList)) //AfterMath jelly _l_
                {   
                    env.AfterMathListJellyValues.Add(node.Key, (string)node.Value);
                }else if (node.Key.StartsWith(Bee.Scents.Flower)) //visit server flowers
                {
                    env.IsFlowerCall = true;
                    Bee.Workers.VisitFlower(node.Key.Substring(2), (JObject)node.Value, ref env);
                    env.IsFlowerCall = false;
                }
                else if (node.Key.StartsWith(Bee.Scents.Pot) && !node.Key.StartsWith(Bee.Scents.QuisoPage)) //_p put value into the pot
                {
                    env.Pots.Add(node.Key, (String)node.Value);
                }
                else if (node.Key.StartsWith(Bee.Scents.Quiso) && !node.Key.StartsWith(Bee.Scents.QuisoPage))
                {
                    if (node.Key.Equals(Bee.Scents.QuisoCount))
                    {
                        sql.IsCount = true;
                        sql.CountAs = env.CurrentPath + "_" + "Count";
                    }

                    if (node.Key.Equals(Bee.Scents.QuisoString))
                    {
                        sql.IsString = true;
                        sql.StringAs = env.CurrentPath + "_" + ((String)node.Value).Trim();
                    }
                    //JToken prevRef = env.CurrentHoneyRef;
                    //env.CurrentHoneyRef = env.Honey;
                    //Bee.Workers.ProcessQuiso(node.Key, (JObject)node.Value, ref env);
                    //env.CurrentHoneyRef = prevRef;
                }
                else if (node.Key.Equals(Bee.Scents.Where)) //where
                {
                    sql.Where += Bee.Workers.FermetCellsWhereScent((JObject)node.Value, realCombName, ref env);
                }
                else if (node.Key.Equals(Bee.Scents.GroupBy)) //_g group by clause
                { //we can only have one group by clause per root object nodes
                    //_g:"Month(EntryDate) MustById"
                    string groupString = ((String)node.Value).Trim().Replace(" ", ", ");
                    sql.GroupBy = groupString;
                }
                else if (node.Key.Equals(Bee.Scents.Ascend)) //_asc_:"Name Size",
                {
                    //string orderString = ((String)node.Value).Trim().Replace(" "," ASC ") + " ASC ,";
                    //sql.OrderBy += orderString;

                    //new staff
                    string os = (String)node.Value;
                    string[] osparts = os.Trim().Split(' ');
                    if (osparts.Length == 1)
                    {
                        sql.OrderBy += osparts[0] + " ASC ,";
                    }
                    else
                    {
                        for (int i = 0; i < osparts.Length; i++)
                        {
                            sql.OrderBy += osparts[i] + " ASC ,";
                        }
                    }
                }
                else if (node.Key.Equals(Bee.Scents.Descend)) //_dsc_:"Name Size",
                {
                    //string orderString = ((String)node.Value).Trim().Replace(" ", " DESC ") + " DESC ,";
                    //sql.OrderBy += orderString;

                    //new staff
                    string os = (String)node.Value;
                    string[] osparts = os.Trim().Split(' ');
                    if (osparts.Length == 1)
                    {
                        sql.OrderBy += osparts[0] + " DESC ,";
                    }
                    else
                    {
                        for (int i = 0; i < osparts.Length; i++)
                        {
                            sql.OrderBy += osparts[i] + " DESC ,";
                        }
                    }

                }
                else if (node.Key.Equals(Bee.Scents.QuisoPage)) //_pg_:"1 20"
                {
                    string[] pageInfo = ((String)node.Value).Split(' ');
                    string pageInfoFirst = pageInfo[0];
                    if(pageInfo.Length > 1){
                        pageInfoFirst = pageInfoFirst.Trim();
                    }
                    string pg = (String.IsNullOrEmpty(pageInfoFirst)) ? "0" : pageInfoFirst;
                    string size = (pageInfo.Length > 1 && pageInfo[1].Trim().Length >= 1)?pageInfo[1].Trim(): "1";
                    sql.page = pg;
                    sql.pageSize = size;
                }
                else if (node.Key.StartsWith(Bee.Scents.Zee)) //buzz , zzzz, zzz ...
                {
                    //all zipings will always be an object
                    string zNodeName = node.Key.Substring(Bee.Scents.Zee.Length);
                    string zRealCombName = Bee.Hive.GetRealCombName(zNodeName);
                    //go through all the different zippings
                    JObject zip = (JObject)node.Value;
                    foreach (var zipItem in zip)
                    {
                        string zipItemName = zipItem.Key;
                        //Its value can only be an array beacuse it doesnt make scence to
                        //zip parents
                        //zipItem.Value must be an array
                        JArray array = (JArray)zipItem.Value;
                        JObject childrenStructure = (JObject)((array.Count() == 0) ? JValue.Parse("{'" + Bee.Scents.Attribute + "':'" + Bee.Scents.Everything + "'}") : array[0]);
                        env.CurrentParentRealCombName = realCombName;
                        String previousPath = env.CurrentPath;
                        env.CurrentPath += "_" + Bee.Engine.PathListIndicator;
                        String childRealCombName = zRealCombName;
                        //check if you have read parmissions here
                        bool can = Bee.Hive.CanRead(childRealCombName, ref env);
                        if (can)
                        {
                            Sql childSql = Ferment(childrenStructure, childRealCombName, zNodeName, ref env);
                            env.ChildSqls.Add(Bee.Scents.Zee + zipItemName + "~" + env.CurrentPath, childSql);
                            env.CurrentPath = previousPath;
                            
                        }
                        else
                        {
                            throw new Exception("Drone Security: You have no rights to read " + zNodeName);
                        }
                    }
                }
                else if (node.Value.Type == JTokenType.Object && !node.Key.StartsWith(Bee.Scents.Scent)) //parents
                {
                    String previousPath = env.CurrentPath;
                    env.CurrentPath += "_" + Bee.Engine.PathObjectIndicator;
                    env.CurrentChildRealCombName = realCombName;


                    String parentRealCombName = Bee.Hive.GetRealCombName(node.Key);
                    //check if you have read parmissions here
                    bool can = Bee.Hive.CanRead(parentRealCombName, ref env);
                    if (can)
                    {
                        Sql parentSql = Ferment((JObject)node.Value, parentRealCombName, node.Key, ref env);
                        env.CurrentPath = previousPath;
                        sql.Select += parentSql.Select;
                        sql.InnerJoin += parentSql.InnerJoin;

                        //new way
                        sql.childSqls.Concat(parentSql.childSqls);
                    }
                    else
                    {
                        throw new Exception("Drone Security: You have no rights to read " + node.Key);
                    }
                }
                else if (node.Value.Type == JTokenType.Array && !node.Key.StartsWith(Bee.Scents.Scent)) //children
                {
                    JArray array = (JArray)node.Value;
                    JObject childrenStructure = (JObject)((array.Count() == 0) ? JValue.Parse("{'" + Bee.Scents.Attribute + "':'" + Bee.Scents.Everything + "'}") : array[0]);
                    env.CurrentParentRealCombName = realCombName;
                    String previousPath = env.CurrentPath;
                    env.CurrentPath += "_" + Bee.Engine.PathListIndicator;

                    String childRealCombName = Bee.Hive.GetRealCombName(node.Key);
                    //check if you have read parmissions here
                    bool can = Bee.Hive.CanRead(childRealCombName, ref env);
                    if (can)
                    {
                        Sql childSql = Ferment(childrenStructure, childRealCombName, node.Key, ref env);
                        env.ChildSqls.Add(env.CurrentPath, childSql);
                        env.CurrentPath = previousPath;

                        
                    }
                    else
                    {
                        throw new Exception("Drone Security: You have no rights to read " + node.Key);
                    }
                }

                

            }
            
            //functions immediate sql e.g _c, _j, 

            //nyd
            //dont forget to decorate, spice etc .... the sql

            //functions aftermath sql


            return sql;
        }

        public static void FlowerGetHelper(String flowerName, JObject fnector, ref Environment env)
        {
            //check if you have read parmissions here
            bool can = Bee.Hive.CanRead(Bee.Scents.Flower + flowerName, ref env);
            if (can == false)
            {
                throw new Exception("Drone Security: You have no rights to visit or eat nector from flower: " + flowerName);
            }

            JObject flowerHoney = Bee.Workers.ProcessGet(fnector, env.IsFlowerCall, env);
            if (flowerHoney == null)
            {
                env.Honey.Add(flowerName, null);
            }
            else
            {
                dynamic x = env.Honey[flowerName];
                if (x == null)
                {
                    env.Honey.Add(flowerName, new JObject());
                }
                bool hasAtleastOneNode = false;
                foreach (var flowerHoneyNode in flowerHoney)
                {

                    if (flowerHoneyNode.Value.Type == JTokenType.Object)
                    {
                        var temp = (JObject)flowerHoneyNode.Value;
                        foreach (var innerNode in temp)
                        {
                            ((JObject)env.Honey[flowerName]).Add(innerNode.Key, innerNode.Value);
                            hasAtleastOneNode = true;
                        }
                    }
                    else
                    {
                        ((JObject)env.Honey[flowerName]).Add(flowerHoneyNode.Key, flowerHoneyNode.Value);
                        hasAtleastOneNode = true;
                    }
                }
                if (hasAtleastOneNode == false)
                {
                    env.Honey[flowerName] = null;
                }
            }
        }

        public static void FlowerPostHelper(String flowerName, JObject fnector, ref Environment env)
        {
            //check if you have Create parmissions here
            bool can = Bee.Hive.CanCreate(Bee.Scents.Flower + flowerName, ref env);
            if (can == false)
            {
                throw new Exception("Drone Security: You have no rights to create nector from flower: " + flowerName);
            }


            JObject flowerHoney = Bee.Workers.ProcessPost(fnector, env);
            if (flowerHoney == null)
            {
                env.Honey.Add(flowerName, null);
            }
            else
            {
                dynamic x = env.Honey[flowerName];
                if (x == null)
                {
                    env.Honey.Add(flowerName, flowerHoney);
                }
                else
                {
                    ((JObject)(env.Honey[flowerName])).Replace(flowerHoney);
                }
                //bool hasAtleastOneNode = false;
                //foreach (var flowerHoneyNode in flowerHoney)
                //{

                //    if (flowerHoneyNode.Value.Type == JTokenType.Object)
                //    {
                //        var temp = (JObject)flowerHoneyNode.Value;
                //        foreach (var innerNode in temp)
                //        {
                //            ((JObject)env.Honey[flowerName]).Add(innerNode.Key, innerNode.Value);
                //            hasAtleastOneNode = true;
                //        }
                //    }
                //    else
                //    {
                //        ((JObject)env.Honey[flowerName]).Add(flowerHoneyNode.Key, flowerHoneyNode.Value);
                //        hasAtleastOneNode = true;
                //    }
                //}
                //if (hasAtleastOneNode == false)
                //{
                //    env.Honey[flowerName] = null;
                //}
            }
        }

        public static void FlowerUpdateHelper(String flowerName, JObject fnector, ref Environment env)
        {
            //check if you have Create parmissions here
            bool can = Bee.Hive.CanUpdate(Bee.Scents.Flower + flowerName, ref env);
            if (can == false)
            {
                throw new Exception("Drone Security: You have no rights to modify nector from flower: " + flowerName);
            }


            JObject flowerHoney = Bee.Workers.ProcessUpdate(fnector, ref env);
            if (flowerHoney == null)
            {
                env.Honey.Add(flowerName, null);
            }
            else
            {
                dynamic x = env.Honey[flowerName];
                if (x == null)
                {
                    env.Honey.Add(flowerName, new JObject());
                }
                bool hasAtleastOneNode = false;
                foreach (var flowerHoneyNode in flowerHoney)
                {

                    if (flowerHoneyNode.Value.Type == JTokenType.Object)
                    {
                        var temp = (JObject)flowerHoneyNode.Value;
                        foreach (var innerNode in temp)
                        {
                            ((JObject)env.Honey[flowerName]).Add(innerNode.Key, innerNode.Value);
                            hasAtleastOneNode = true;
                        }
                    }
                    else
                    {
                        ((JObject)env.Honey[flowerName]).Add(flowerHoneyNode.Key, flowerHoneyNode.Value);
                        hasAtleastOneNode = true;
                    }
                }
                if (hasAtleastOneNode == false)
                {
                    env.Honey[flowerName] = null;
                }
            }
        }

        public static void FlowerDeleteHelper(String flowerName, JObject fnector, ref Environment env)
        {
            //check if you have Delete parmissions here
            bool can = Bee.Hive.CanDelete(Bee.Scents.Flower + flowerName, ref env);
            if (can == false)
            {
                throw new Exception("Drone Security: You have no rights to clear nector from flower: " + flowerName);
            }


            JObject flowerHoney = Bee.Workers.ProcessDelete(fnector, ref env);
            if (flowerHoney == null)
            {
                env.Honey.Add(flowerName, null);
            }
            else
            {
                dynamic x = env.Honey[flowerName];
                if (x == null)
                {
                    env.Honey.Add(flowerName, new JObject());
                }
                bool hasAtleastOneNode = false;
                foreach (var flowerHoneyNode in flowerHoney)
                {

                    if (flowerHoneyNode.Value.Type == JTokenType.Object)
                    {
                        var temp = (JObject)flowerHoneyNode.Value;
                        foreach (var innerNode in temp)
                        {
                            ((JObject)env.Honey[flowerName]).Add(innerNode.Key, innerNode.Value);
                            hasAtleastOneNode = true;
                        }
                    }
                    else
                    {
                        ((JObject)env.Honey[flowerName]).Add(flowerHoneyNode.Key, flowerHoneyNode.Value);
                        hasAtleastOneNode = true;
                    }
                }
                if (hasAtleastOneNode == false)
                {
                    env.Honey[flowerName] = null;
                }
            }
        }

        public static void VisitFlower(String flowerName, JObject PollenGrains, ref Environment env)
        {
            var tempHoldContext = env.ContextObject;
            env.ContextObject = PollenGrains;
            //we deep clone here because we dont want to edit the hive file
            JObject flowers = (JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName];
            JObject fla = (JObject)flowers[flowerName];
            String flaStr = fla.ToString();
            JObject flower = (JObject)JValue.Parse(flaStr); //(JObject)(fla.DeepClone());
            JObject fnector = (JObject)flower[Bee.Engine.RawNectorName];

            String way = (String)flower[Bee.Engine.ActionName];
            //extend  the nector with users inclusion sent
            bool formerIsPosting = env.IsPosting;
            env.IsPosting = (way.Equals(Bee.Engine.GetActionName) ) ? false : true ;
            Bee.Queen.include(ref fnector, PollenGrains, ref env);           
            if (way.Equals(Bee.Engine.GetActionName))
            {
                Bee.Workers.FlowerGetHelper(flowerName, fnector, ref env);
            }else if (way.Equals(Bee.Engine.PostActionName))
            {
                Bee.Workers.FlowerPostHelper(flowerName, fnector, ref env);
            }
            else if (way.Equals(Bee.Engine.UpdateActionName))
            {
                Bee.Workers.FlowerUpdateHelper(flowerName, fnector, ref env);
            }
            else if (way.Equals(Bee.Engine.DeleteActionName))
            {
                Bee.Workers.FlowerDeleteHelper(flowerName, fnector, ref env);
            }
            env.IsPosting = formerIsPosting;
            env.ContextObject = tempHoldContext; //put back the original context
        }

        public static void ProcessQuiso(String quisoKey, JObject objectsToBeCounted, ref Environment env)
        {
            //we are interested in node key., which is the comb name and a where only
            if (quisoKey.Equals(Bee.Scents.QuisoCount))
            {
                JToken countNode = env.CurrentHoneyRef["count"];
                if (countNode == null)
                {
                    ((JObject)env.CurrentHoneyRef).Add("Count", new JObject());
                }

                foreach (var objectToBeCounted in objectsToBeCounted)
                {
                    string realCn = Bee.Hive.GetRealCombName(objectToBeCounted.Key);
                    string pk = Bee.Hive.GetPK(realCn);
                    Sql countSql = new Sql();
                    countSql.realCombName = realCn;
                    countSql.Select = " COUNT([dbo].[" + realCn + "].[" + pk + "]) AS list_of_" + objectToBeCounted.Key + " ";
                    //check if it has a where
                    JArray arr = (JArray)objectToBeCounted.Value;
                    if (arr.Count() > 0)
                    {
                        JToken w = arr[0][Bee.Scents.Where];
                        if (w != null)
                        {
                            countSql.Where = Bee.Workers.FermetCellsWhereScent((JObject)w, realCn, ref env);
                        }
                    }
                    countSql.Prepare(ref env);
                    List<Object> Results = Bee.Workers.GetResults(countSql, Bee.Workers.RetriveConn(ref env));
                    if (Results == null || Results.Count() == 0)
                    {
                        ((JObject)(env.CurrentHoneyRef["Count"])).Add(objectToBeCounted.Key, 0);
                    }
                    else
                    {
                        for (int i = 0; i < Results.Count(); i++)
                        {
                            //the properties of this object
                            List<DataCell> dataCells = (List<DataCell>)Results[i];
                            for (int j = 0; j < dataCells.Count(); j++)
                            {
                                DataCell dc = dataCells[j];
                                Object cellValue = dc.cellValue;
                                String colName = dc.pathToCellValue;
                                ((JObject)(env.CurrentHoneyRef["Count"])).Add(objectToBeCounted.Key, Convert.ToInt32(cellValue));
                            }
                        }
                    }
                }
            }
        }


        public static JObject ProcessGet(JObject nector, bool isFlowerCall, Environment envForContext)
        {
            List<string> theInvisibleCellNamePaths = new List<string>();

            Environment environment = new Environment();
            environment.IsFlowerCall = isFlowerCall;
            if (envForContext != null)
            {
                environment.IsSelf = envForContext.IsSelf;
                environment.CurrentUser = envForContext.CurrentUser;
                environment.ContextObject = envForContext.ContextObject;
                //environment = envForContext;
            }


            foreach (var nectorNode in nector)
            {
                if (nectorNode.Key.StartsWith(Bee.Scents.Scent) &&
                   !nectorNode.Key.StartsWith(Bee.Scents.Flower) &&
                   !nectorNode.Key.StartsWith(Bee.Scents.Quiso) &&
                   !nectorNode.Key.StartsWith(Bee.Scents.Zee))
                {
                    continue;
                }

                if(nectorNode.Key.StartsWith(Bee.Scents.Flower)){
                    environment.IsFlowerCall = true;
                    Bee.Workers.VisitFlower(nectorNode.Key.Substring(Bee.Scents.Flower.Length), (JObject)nectorNode.Value, ref environment);
                    environment.IsFlowerCall = false;
                }
                else if (nectorNode.Key.StartsWith(Bee.Scents.Quiso))
                {
                    JToken prevRef = environment.CurrentHoneyRef;
                    environment.CurrentHoneyRef = environment.Honey;
                    Bee.Workers.ProcessQuiso(nectorNode.Key, (JObject)nectorNode.Value, ref environment);
                    environment.CurrentHoneyRef = prevRef;
                }
                else if (nectorNode.Key.StartsWith(Bee.Scents.Zee)) //buzz , zzzz, zzz ...
                {
                    //all zipings will always be an object
                    String pathIndicator = "";
                    string zCombName = nectorNode.Key.Substring(Bee.Scents.Zee.Length);
                    string realCombName = Bee.Hive.GetRealCombName(zCombName);
                    //go through all the different zippings
                    JObject zip = (JObject)nectorNode.Value;
                    foreach (var zipItem in zip)
                    {
                        string zipItemName = zipItem.Key;
                        //Its value can be an array or an object
                        JObject combStructure = null;
                        if (zipItem.Value.Type == JTokenType.Array)
                        {
                            pathIndicator = Bee.Engine.PathListIndicator;
                            //check if its empty in which case we insert a full structure definition of { "_a" : "*" }
                            JArray array = (JArray)zipItem.Value;
                            combStructure = (JObject)((array.Count() == 0) ? JValue.Parse("{'" + Bee.Scents.Attribute + "':'" + Bee.Scents.Everything + "'}") : array[0]);
                        }
                        else
                        {
                            pathIndicator = Bee.Engine.PathObjectIndicator;
                            combStructure = (JObject)zipItem.Value;
                        }

                        environment.CurrentPath = pathIndicator;
                        string nichPath = pathIndicator + zipItemName;
                        //create a ninch environment for this node in the nector
                        environment.Ninches.Add(nichPath, new object());

                        //check if you have read parmissions here
                        bool can = Bee.Hive.CanRead(realCombName, ref environment);
                        if (can)
                        {
                            Sql nodeSql = Bee.Workers.Ferment(combStructure, realCombName, zCombName, ref environment);
                            nodeSql.Select = nodeSql.Select.Trim(',');
                            nodeSql.path = pathIndicator;
                            environment.RootSqls.Add(Bee.Scents.Zee + zipItemName + "~" + environment.CurrentPath, nodeSql);
                            if (pathIndicator.Equals(Bee.Engine.PathObjectIndicator))
                            {
                                nodeSql.isRootSql = true;
                                nodeSql.rootSqlIsObject = true;
                            }
                        }
                        else
                        {
                            throw new Exception("Drone Security: You have no rights to read " + zCombName);
                        }
                    }//end foreach (var zipItem in zip)

                }
                else{ //this is a combname (cn)
                    String pathIndicator = "";
                    string combName = nectorNode.Key;
                    //Its value can be an array or an object
                    JObject combStructure = null;
                    if (nectorNode.Value.Type == JTokenType.Array)
                    {
                        pathIndicator = Bee.Engine.PathListIndicator;
                        //check if its empty
                        //in which case we insert a full structure definition of { "_a" : "*" }
                        JArray array = (JArray)nectorNode.Value;
                        combStructure = (JObject)((array.Count() == 0) ? JValue.Parse("{'" + Bee.Scents.Attribute + "':'" + Bee.Scents.Everything + "'}"): array[0]);                       
                    }
                    else
                    {
                        pathIndicator = Bee.Engine.PathObjectIndicator;
                        combStructure = (JObject)nectorNode.Value;
                        
                    }
                    environment.CurrentPath = pathIndicator;
                    string nichPath = pathIndicator + combName;
                    //create a ninch environment for this node in the nector
                    environment.Ninches.Add(nichPath, new object());
                    string realCombName = Bee.Hive.GetRealCombName(combName);

                    //check if you have read parmissions here
                    bool can = Bee.Hive.CanRead(realCombName, ref environment);
                    if (can)
                    {
                        Sql nodeSql = Bee.Workers.Ferment(combStructure, realCombName, combName, ref environment);
                        nodeSql.Select = nodeSql.Select.Trim(',');
                        nodeSql.path = pathIndicator;
                        environment.RootSqls.Add(environment.CurrentPath, nodeSql);
                        if (pathIndicator.Equals(Bee.Engine.PathObjectIndicator))
                        {
                            nodeSql.isRootSql = true;
                            nodeSql.rootSqlIsObject = true;
                        }
                        else if (pathIndicator.Equals(Bee.Engine.PathListIndicator))
                        {
                            nodeSql.isRootSql = true;
                            nodeSql.rootSqlIsObject = false;
                        }
                    }
                    else
                    {
                        throw new Exception("Drone Security: You have no rights to read " + combName);
                    }

                    
                }
            }


            JObject zipBucket = new JObject();
            //turn our sql into honey
            foreach (KeyValuePair<String,Sql> rootSql in environment.RootSqls)
	        {
                String path = rootSql.Key;
                if (path.StartsWith(Bee.Scents.Zee))
                {
                    String[] zeeParts = path.Split('~');
                    String zipItemName = zeeParts[0].Substring(Bee.Scents.Zee.Length);
                    String pathOfExecution = zeeParts[1]; //e.g lMustBuys
                    Sql sql = rootSql.Value;
                    sql.Prepare(ref environment);
                    var x = Bee.Workers.MakeHoney(sql, ref environment, false, "");
                    //the honey has to be kept some where
                    //so of like in a bucket so that that node can be reused
                    //remmber that a zip always is an object
                    String rootPath = pathOfExecution.Substring(1);
                    JToken jt = zipBucket[rootPath];
                    if (jt == null)
                    {
                        zipBucket.Add(rootPath,new JObject());
                    }
                    //package 
                    //add these results to the bucket
                    JToken extract  = environment.Honey[rootPath].DeepClone();
                    //clean this extract
                    Bee.Workers.PackageSpecificHoney(ref extract, ref environment);
                    ((JObject)zipBucket[rootPath]).Add(zipItemName, extract);
                    //clear this space
                    environment.Honey.Remove(rootPath);
                }
                else
                {
                    Sql sql = rootSql.Value;
                    sql.Prepare(ref environment);

                    var x = Bee.Workers.MakeHoney(sql, ref environment, false, "");
                }
            }
            //get all the items in the bucket and put them into the honey
            foreach (var item in zipBucket)
            {
                JToken x = environment.Honey[item.Key];
                if (x == null)
                {
                    environment.Honey.Add(item.Key,item.Value);
                }
                else
                {
                    //update
                    environment.Honey[item.Key].Replace(item.Value);
                }
            }
            
            
            Bee.Workers.Package(ref environment);
            
            return environment.Honey;
        }


        public static JObject ProcessPost(JObject nectorJsonObject, Environment env)
        {
            var conn = Hive.GetConnection();
            Dictionary<string, int> hooks = new Dictionary<string, int>();
            JObject honey = new JObject();

            
            
            String sql = "";
            //Going through the root nodes
            foreach (var rootNode in nectorJsonObject)
            {
                string rootNodeKey = rootNode.Key;
                if (rootNodeKey.Equals(Bee.Scents.Errors))
                {
                    continue;
                }
                string realTableName = Hive.GetRealCombName(rootNodeKey);

                if (env != null)
                {
                    //check if you have read parmissions here
                    bool can = Bee.Hive.CanCreate(realTableName, ref env);
                    if (can == false)
                    {
                        throw new Exception("DRONE SECURITY: you cannot create honey at " + realTableName);
                    }
                }

                if (rootNode.Key.StartsWith(Bee.Scents.JellyImmediate) && !rootNode.Key.StartsWith(Bee.Scents.JellyAfterMath))
                {
                    env.ImmediateJellyValues.Add(rootNode.Key, Bee.Queen.Eat((String)rootNode.Value, ref env));
                }
                else if (rootNode.Key.StartsWith(Bee.Scents.JellyAfterMath)) //AfterMath jelly _j_
                {   //this jelly is made after the query has returned data and this object has been filled in
                    env.AfterMathJellyValues.Add(rootNode.Key, (string)rootNode.Value);
                }
                else if (rootNode.Key.StartsWith(Bee.Scents.JellyImmediateList) && !rootNode.Key.StartsWith(Bee.Scents.JellyAfterMathList))
                {
                    env.ImmediateListJellyValues.Add(rootNode.Key, Bee.Queen.EatAlot((String)rootNode.Value, ref env));
                }
                else if (rootNode.Key.StartsWith(Bee.Scents.JellyAfterMathList)) //AfterMath jelly _l_
                {
                    env.AfterMathListJellyValues.Add(rootNode.Key, (string)rootNode.Value);
                }
                else if (rootNode.Key.StartsWith(Bee.Scents.Pot)) //put value into the pot
                {
                    env.Pots.Add(rootNode.Key, (String)rootNode.Value);
                }
                else if (rootNode.Value.Type == JTokenType.Array) //this is an array
                {
                    honey.Add(rootNodeKey, new JArray());
                    String prevPath = env.CurrentPath;
                    env.CurrentPath = Bee.Engine.PathListIndicator + rootNode.Key + "_";
                    //go through all the objects in the array
                    foreach (JObject nodeObject in rootNode.Value)
                    {
                        JObject insertedObject = PostObject(nodeObject, realTableName, ref hooks, ref env);
                        ((JArray)honey[rootNodeKey]).Add(insertedObject);
                    }
                    env.CurrentPath = prevPath;
                }
                else
                {
                    String prevPath = env.CurrentPath;
                    env.CurrentPath = Bee.Engine.PathObjectIndicator + rootNode.Key + "_";
                    JObject nodeObject = (JObject)rootNode.Value;
                    JObject insertedObject = PostObject(nodeObject, realTableName, ref hooks, ref env);
                    honey.Add(rootNodeKey,insertedObject);
                    env.CurrentPath = prevPath;
                }
            }

            env.CurrentHoneyRef = honey;
            //execiute the aftermaths jellies7
            List<KeyValuePair<string, string>> amfvs = env.AfterMathJellyValues.ToList();
            foreach (KeyValuePair<string, string> amf in amfvs)
            {
                if (env.EatenAfterMathJellyValues.Contains(amf.Key))
                {
                    continue;
                }
                

                //this has to work with a collection of guys
                string variableName = amf.Key;
                string funDef = amf.Value;
                //nyd
                //for now this is executing at the first node as the current ref
                //but the  may in future have a path UserName or attributes may have a path
                //instead of just UserName ==> User_UserName ==> User.UserName
                JToken prevRef = env.CurrentHoneyRef;
                string xpatName = "";
                foreach (var xpert in honey)
                {
                    xpatName = xpert.Key;
                    break;
                }
                if (!string.IsNullOrEmpty(xpatName))
                {
                    JToken tn = honey[xpatName];
                    if (tn != null)
                    {
                        if (tn.Type == JTokenType.Array)
                        {
                            JArray tnItems = (JArray)tn;
                            for (int i = 0; i < tnItems.Count(); i++)
                            {
                                env.CurrentHoneyRef = (JToken)tnItems[i];
                                JObject formerHoney = env.Honey;
                                env.Honey = honey;
                                string funValue = Bee.Queen.Eat(funDef, ref env);
                                env.AfterMathJellyValues[amf.Key + i] = funValue;
                                env.Honey = formerHoney;
                            }
                            env.EatenAfterMathJellyValues.Add(amf.Key);
                        }
                        else
                        {
                            env.CurrentHoneyRef = (JObject)tn;
                            JObject formerHoney = env.Honey;
                            env.Honey = honey;
                            string funValue = Bee.Queen.Eat(funDef, ref env);
                            env.AfterMathJellyValues[amf.Key] = funValue;
                            env.EatenAfterMathJellyValues.Add(amf.Key);
                            env.Honey = formerHoney;
                        }
                    }
                }
                env.CurrentHoneyRef = prevRef;
                
            }

            //execute the aftermaths list jellies
            List<KeyValuePair<string, Object>> lamvs = env.AfterMathListJellyValues.ToList();
            foreach (KeyValuePair<string, Object> lam in lamvs)
            {
                if (env.EatenAfterMathListJellyValues.Contains(lam.Key))
                {
                    continue;
                }
                
                string variableName = lam.Key;
                string funDef = (string)lam.Value;
                //nyd
                //for now this is executing at the first node as the current ref
                //but the  may in future have a path UserName or attributes may have a path
                //instead of just UserName ==> User_UserName ==> User.UserName
                JToken prevRef = env.CurrentHoneyRef;
                string xpatName = "";
                foreach (var xpert in honey)
                {
                    xpatName = xpert.Key;
                    break;
                }
                if (!string.IsNullOrEmpty(xpatName))
                {
                    env.CurrentHoneyRef = honey[xpatName];
                }
                List<string> funValue = Bee.Queen.EatAlot(funDef, ref env);
                env.AfterMathListJellyValues[lam.Key] = funValue;
                env.EatenAfterMathListJellyValues.Add(lam.Key);
                env.CurrentHoneyRef = prevRef;
                
            }


            Bee.Workers.cleanHoney(env.GeneratedInvisiblePaths, ref honey);
            return honey;
        }

        public static JObject PostObject(JObject nodeObject, string realTableName, ref Dictionary<string, int> hooks, ref Environment env)
        {
            var conn = Hive.GetConnection();
            String nodeObjSql = "INSERT INTO [dbo].[" + realTableName + "] ( ";
            String valuesSqlPartial = "";
            string _r = "";
            string _ro_ = "";
            int lastInsertId = 0;
            Dictionary<string, JArray> excecuteLaiter = new Dictionary<string, JArray>();
            Trap trap = new Trap();
            trap.ExternalParameter = new JObject();


            JObject insertedObject = new JObject();
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in nodeObject)
            {
                string columnName = nodeObjectAttribute.Key;
                if (columnName.Equals(Bee.Scents.Obtain))
                {
                    //get fks
                    JObject fks = (JObject)nodeObjectAttribute.Value;
                    //go through the forein keys
                    foreach (var fksNode in fks)
                    {
                        string fkColumnName = fksNode.Key;
                        dynamic fkColumnRepStr = fksNode.Value;
                        string fkColumnRep = fkColumnRepStr;
                        //look up the value of this
                        int fkValue = hooks[fkColumnRep];
                        nodeObjSql = nodeObjSql + " [" + fkColumnName + "] ,";
                        //get the type of column
                        valuesSqlPartial = valuesSqlPartial + " " + fkValue + " ,";
                        insertedObject.Add(fkColumnName, fkValue);

                        bool isHidden = Bee.Hive.isInvisible(fkColumnName, realTableName);
                        if (isHidden)
                        {
                            String path = env.CurrentPath + fkColumnName;
                            env.GeneratedInvisiblePaths.Add(path);
                        }
                    }
                } else if (columnName.Equals(Bee.Scents.ObtainObject)) //_oo_: { myFk: 'User_UserId'}
                {
                    //get fks
                    JObject fks = (JObject)nodeObjectAttribute.Value;
                    //go through the forein keys
                    foreach (var fksNode in fks)
                    {
                        string fkColumnName = fksNode.Key;
                        dynamic fkColumnRepStr = fksNode.Value;
                        string fkColumnRep = fkColumnRepStr;
                        //look up the value of this
                        string []pathx = fkColumnRep.Split('_');
                        string tokenPath = fkColumnRep.Substring(pathx[0].Length + 1).Replace('_','.'); //User_UserId
                        JToken fkValue = env.RetainedObjects[pathx[0]].SelectToken(tokenPath);
                        nodeObjSql = nodeObjSql + " [" + fkColumnName + "] ,";
                       
                        //get the type of column
                        string qfkValue = Bee.Hive.GetQuoted(fkValue.ToString(), fkValue.Type);
                        valuesSqlPartial = valuesSqlPartial + " " + qfkValue + " ,";
                        insertedObject.Add(fkColumnName, fkValue);

                        bool isHidden = Bee.Hive.isInvisible(fkColumnName, realTableName);
                        if (isHidden)
                        {
                            String path = env.CurrentPath + fkColumnName;
                            env.GeneratedInvisiblePaths.Add(path);
                        }
                    }
                }
                else if (nodeObjectAttribute.Key.StartsWith(Bee.Scents.Pot)) //put value into the pot
                {
                    env.Pots.Add(nodeObjectAttribute.Key, (String)nodeObjectAttribute.Value);
                }
                else if (columnName.Equals(Bee.Scents.Retain))
                {
                    //retain primary key value when this object has been inserted
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    hooks.Add(value, 0);
                    _r = value;
                }
                else if (columnName.Equals(Bee.Scents.RetainObject))
                {
                    //retain primary key value and the rest of the object  when this object has been inserted
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    env.RetainedObjects.Add(value, null);
                    _ro_ = value;
                }
                else if (columnName.StartsWith(Bee.Scents.JellyImmediate) && !columnName.StartsWith(Bee.Scents.JellyAfterMath))
                {
                    //this function is executed immediately within this context
                    //"_fEncDesc": "encrypt _@desc _mySweetHoney",
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen lay these eggs
                    string value = Bee.Queen.Lay(eggsDef, trap.ExternalParameter);
                    trap.immediateFunctionValues.Add(columnName, value);
                }
                else if (columnName.StartsWith("_l") && !columnName.StartsWith("_l_")) //a list
                {
                    //this function is executed immediately within this context
                    //it returns a list of items
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen lay these eggs
                    List<Object> value = Bee.Queen.LayMany(eggsDef, trap.ExternalParameter);
                    trap.immediateListFunctionValues.Add(columnName, value);
                }
                else if (columnName.StartsWith(Bee.Scents.Attribute))
                {
                    //this function is executed immediately within this context
                    //and is part of the values to be submitted
                    //its also an immediate function value
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen eat this jelly
                    string value = Bee.Queen.Eat(eggsDef, ref env);
                    trap.immediateFunctionValues.Add(columnName, value);

                    //we assume its a normal attribute
                    nodeObjSql = nodeObjSql + " [" + variableName + "] ,";
                    string qv = Hive.GetQuoted(value, variableName, realTableName);
                    valuesSqlPartial = valuesSqlPartial + " " + qv + " ,";
                    //every thing here is also an external paramter
                    trap.ExternalParameter.Add(columnName, nodeObjectAttribute.Value);
                    insertedObject.Add(variableName, value);

                    bool isHidden = Bee.Hive.isInvisible(variableName, realTableName);
                    if (isHidden)
                    {
                        String path = env.CurrentPath + variableName;
                        env.GeneratedInvisiblePaths.Add(path);
                    }
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //these will be executed laiter because their foreign key 
                    //may not yet be available at this time
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    excecuteLaiter.Add(columnName, arr);
                    insertedObject.Add(columnName, new JArray());
                }
                else
                {
                    //we assume its a normal attribute
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    nodeObjSql = nodeObjSql + " [" + columnName + "] ,";

                    string qv = Hive.GetQuoted(value, columnName, realTableName);
                    valuesSqlPartial = valuesSqlPartial + " " + qv + " ,";
                    //every thing here is also an external paramter
                    trap.ExternalParameter.Add(columnName, nodeObjectAttribute.Value);
                    insertedObject.Add(columnName, value);

                    bool isHidden = Bee.Hive.isInvisible(columnName, realTableName);
                    if (isHidden)
                    {
                        String path = env.CurrentPath + columnName;
                        env.GeneratedInvisiblePaths.Add(path);
                    }
                }
            }//end for loop
            nodeObjSql = nodeObjSql.Trim(',');
            valuesSqlPartial = valuesSqlPartial.Trim(',');
            nodeObjSql = nodeObjSql + " ) VALUES ( " + valuesSqlPartial + " ) ";

            if (!String.IsNullOrEmpty(_r) || !String.IsNullOrEmpty(_ro_))
            {
                nodeObjSql = nodeObjSql + "; SELECT SCOPE_IDENTITY();";
            }

            using (SqlCommand sqlCmd = new SqlCommand(nodeObjSql, conn))
            {
                if (!String.IsNullOrEmpty(_r) || !String.IsNullOrEmpty(_ro_))
                {
                    string res = sqlCmd.ExecuteScalar().ToString();
                    lastInsertId = Convert.ToInt32(res);
                }
                else
                {
                    sqlCmd.ExecuteNonQuery();
                }
            }

            if (!String.IsNullOrEmpty(_r) || !String.IsNullOrEmpty(_ro_))
            {
                if(!String.IsNullOrEmpty(_r)){
                    hooks[_r] = lastInsertId;
                }
                string pk = Bee.Hive.GetPK(realTableName);
                insertedObject.Add(pk, lastInsertId);
                if (!String.IsNullOrEmpty(_ro_))
                {
                    env.RetainedObjects[_ro_] = (JObject)insertedObject.DeepClone();
                }
            }

            //execute laiter
            foreach (KeyValuePair<string, JArray> arrayItem in excecuteLaiter)
            {
                string colName = arrayItem.Key;
                JArray arr = arrayItem.Value;
                string localRealTableName = Hive.GetRealCombName(colName);
                foreach (JObject localNodeObject in arr)
                {
                    JObject childObj = PostObject(localNodeObject, localRealTableName, ref hooks, ref env);
                    ((JArray)insertedObject[colName]).Add(childObj);
                }
            }

            return insertedObject;
        }


        public static JObject ProcessUpdate(JObject nector, ref Environment env)
        {
            var conn = Hive.GetConnection();


            JObject honey = new JObject();
            String sql = "";
            //Going through the root nodes
            foreach (var rootNode in nector)
            {
                string rootNodeKey = rootNode.Key;
                if (rootNodeKey.Equals(Bee.Scents.Errors))
                {
                    continue;
                }
                string realTableName = Hive.GetRealCombName(rootNodeKey);
                bool can = Bee.Hive.CanUpdate(realTableName, ref env);
                if (can == false)
                {
                    throw new Exception("Drone Security: You have no rights to modify " + rootNodeKey);
                }

                if (rootNode.Value.Type == JTokenType.Array) //this is an array
                {
                    //go through all the objects in the array
                    foreach (JObject nodeObject in rootNode.Value)
                    {
                        UpdateObject(nodeObject, realTableName, ref env);
                    }
                }
                else
                {
                    JObject nodeObject = (JObject)rootNode.Value;
                    UpdateObject(nodeObject, realTableName, ref env);
                }
            }

            return honey;
        }

        public static void UpdateObject(JObject nodeObject, string realTableName, ref Environment env)
        {
            var conn = Hive.GetConnection();
            Trap trap = new Trap();
            trap.ExternalParameter = new JObject();

            String nodeObjSql = "UPDATE [dbo].[" + realTableName + "] SET ";
            String whereSql = "";
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in nodeObject)
            {
                string columnName = nodeObjectAttribute.Key;
                if (columnName.Equals(Bee.Scents.Where))
                {
                    //get where clause
                    JObject whereObject = (JObject)nodeObjectAttribute.Value;
                    string whereRes = Bee.Workers.FermetCellsWhereScent(whereObject, realTableName, ref env);
                    whereSql = whereSql + whereRes;
                }
                else if (nodeObjectAttribute.Key.StartsWith(Bee.Scents.Pot)) //put value into the pot
                {
                    env.Pots.Add(nodeObjectAttribute.Key, (String)nodeObjectAttribute.Value);
                }
                else if (columnName.StartsWith(Bee.Scents.JellyImmediate) && !columnName.StartsWith(Bee.Scents.JellyAfterMath))
                {
                    //this function is executed immediately within this context
                    //"_fEncDesc": "encrypt _@desc _mySweetHoney",
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen lay these eggs
                    string value = Bee.Queen.Eat(eggsDef, ref env);
                    env.ImmediateJellyValues.Add(columnName, value);
                    trap.immediateFunctionValues.Add(columnName, value);
                }
                else if (columnName.StartsWith(Bee.Scents.JellyAfterMath)) //AfterMath jelly _j_
                {   //this jelly is made after the query has returned data and this object has been filled in
                    env.AfterMathJellyValues.Add(columnName, (string)nodeObjectAttribute.Value);
                }else if (columnName.StartsWith(Bee.Scents.JellyImmediateList) && !columnName.StartsWith(Bee.Scents.JellyAfterMathList))
                {
                    env.ImmediateListJellyValues.Add(columnName, Bee.Queen.EatAlot((String)nodeObjectAttribute.Value, ref env));
                }
                else if (columnName.StartsWith(Bee.Scents.JellyAfterMathList)) //AfterMath jelly _l_
                {
                    env.AfterMathListJellyValues.Add(columnName, (string)nodeObjectAttribute.Value);
                }
                else if (columnName.StartsWith("_l") && !columnName.StartsWith("_l_")) //a list
                {
                    //this function is executed immediately within this context
                    //it returns a list of items
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen lay these eggs
                    List<Object> value = Bee.Queen.LayMany(eggsDef, trap.ExternalParameter);
                    trap.immediateListFunctionValues.Add(columnName, value);
                    env.AfterMathListJellyValues.Add(columnName, value);
                }
                else if (columnName.StartsWith(Bee.Scents.Attribute))
                {
                    //this function is executed immediately within this context
                    //and is part of the values to be submitted
                    //its also an immediate function value
                    string variableName = columnName.Substring(2);
                    dynamic dynFunctionDef = nodeObjectAttribute.Value;
                    string eggsDef = dynFunctionDef;
                    //let the queen eat this jelly
                    string value = Bee.Queen.Eat(eggsDef, ref env);
                    trap.immediateFunctionValues.Add(columnName, value);

                    //we assume its a normal attribute
                    string qv = Hive.GetQuoted(value, variableName, realTableName);
                    nodeObjSql = nodeObjSql + " " + variableName + " = " + qv + " ,";
                    //every thing here is also an external paramter
                    trap.ExternalParameter.Add(columnName, nodeObjectAttribute.Value);                    
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //for update operations the nested objets are executed imediately
                    string localRealTableName = Hive.GetRealCombName(columnName);
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    foreach (JObject localNodeObject in arr)
                    {
                        UpdateObject(localNodeObject, localRealTableName, ref env);
                    }
                }
                else
                {
                    //we assume its a normal attribute
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    string qv = Hive.GetQuoted(value, columnName, realTableName);
                    nodeObjSql = nodeObjSql + " " + columnName + " = " + qv + " ,";
                }
            }//end for loop
            nodeObjSql = nodeObjSql.Trim(',');
            whereSql = whereSql.Trim();
            if (!string.IsNullOrEmpty(whereSql))
            {
                nodeObjSql = nodeObjSql + " WHERE " + whereSql;
            }

            using (SqlCommand sqlCmd = new SqlCommand(nodeObjSql, conn))
            {
                sqlCmd.ExecuteNonQuery();
            }

            //execiute the aftermaths jellies7
            List<KeyValuePair<string, string>> amfvs = env.AfterMathJellyValues.ToList();
            foreach (KeyValuePair<string, string> amf in amfvs)
            {
                if (env.EatenAfterMathJellyValues.Contains(amf.Key))
                {
                    continue;
                }
                
                //this has to work with a collection of guys
                string variableName = amf.Key;
                string funDef = amf.Value;
                //nyd
                //for now this is executing at the first node as the current ref
                //but the  may in future have a path UserName or attributes may have a path
                //instead of just UserName ==> User_UserName ==> User.UserName
                JToken prevRef = env.CurrentHoneyRef;
                string xpatName = "";
                foreach (var xpert in env.Honey)
                {
                    xpatName = xpert.Key;
                    break;
                }
                if (!string.IsNullOrEmpty(xpatName))
                {
                    JToken tn = env.Honey[xpatName];
                    if (tn != null)
                    {
                        if (tn.Type == JTokenType.Array)
                        {
                            JArray tnItems = (JArray)tn;
                            for (int i = 0; i < tnItems.Count(); i++)
                            {
                                env.CurrentHoneyRef = (JToken)tnItems[i];
                                string funValue = Bee.Queen.Eat(funDef, ref env);
                                env.AfterMathJellyValues[amf.Key + i] = funValue;
                            }
                            env.EatenAfterMathJellyValues.Add(amf.Key);
                        }
                        else
                        {
                            env.CurrentHoneyRef = (JObject)tn;
                            string funValue = Bee.Queen.Eat(funDef, ref env);
                            env.AfterMathJellyValues[amf.Key] = funValue;
                            env.EatenAfterMathJellyValues.Add(amf.Key);
                        }
                    }
                }
                env.CurrentHoneyRef = prevRef;
                
            }

            //execute the aftermaths list jellies
            List<KeyValuePair<string, Object>> lamvs = env.AfterMathListJellyValues.ToList();
            foreach (KeyValuePair<string, Object> lam in lamvs)
            {
                if (env.EatenAfterMathListJellyValues.Contains(lam.Key))
                {
                    continue;
                }
                
                string variableName = lam.Key;
                string funDef = (string)lam.Value;
                //nyd
                //for now this is executing at the first node as the current ref
                //but the  may in future have a path UserName or attributes may have a path
                //instead of just UserName ==> User_UserName ==> User.UserName
                JToken prevRef = env.CurrentHoneyRef;
                string xpatName = "";
                foreach (var xpert in env.Honey)
                {
                    xpatName = xpert.Key;
                    break;
                }
                if (!string.IsNullOrEmpty(xpatName))
                {
                    env.CurrentHoneyRef = env.Honey[xpatName];
                }
                List<string> funValue = Bee.Queen.EatAlot(funDef, ref env);
                env.AfterMathListJellyValues[lam.Key] = funValue;
                env.EatenAfterMathListJellyValues.Add(lam.Key);
                env.CurrentHoneyRef = prevRef;
            }

        }


        public static JObject ProcessDelete(JObject nector, ref Environment env)
        {
            var conn = Hive.GetConnection();

            JObject honey = new JObject();
            String sql = "";
            //Going through the root nodes
            foreach (var rootNode in nector)
            {
                string rootNodeKey = rootNode.Key;
                string realTableName = Hive.GetRealCombName(rootNodeKey);
                bool can = Bee.Hive.CanDelete(realTableName, ref env);
                if (can == false)
                {
                    throw new Exception("Drone Security: You have no rights to delete " + rootNodeKey);
                }
                if (rootNode.Value.Type == JTokenType.Array) //this is an array
                {
                    //go through all the objects in the array
                    foreach (JObject nodeObject in rootNode.Value)
                    {
                        DeleteObject(nodeObject, realTableName, ref env);
                    }
                }
                else
                {
                    JObject nodeObject = (JObject)rootNode.Value;
                    DeleteObject(nodeObject, realTableName, ref env);
                }
            }

            return honey;
        }

        public static void DeleteObject(JObject nodeObject, string realTableName, ref Environment env)
        {
            var conn = Hive.GetConnection();
            String nodeObjSql = "DELETE FROM " + realTableName + "  ";
            String whereSql = "";
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in nodeObject)
            {
                string columnName = nodeObjectAttribute.Key;
                if (columnName.Equals(Bee.Scents.Where))
                {
                    //get where clause
                    JObject whereObject = (JObject)nodeObjectAttribute.Value;
                    string whereRes = Bee.Workers.FermetCellsWhereScent(whereObject, realTableName, ref env);
                    whereSql = whereSql + whereRes;
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //for update operations the nested objets are executed imediately
                    string localRealTableName = Hive.GetRealCombName(columnName);
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    foreach (JObject localNodeObject in arr)
                    {
                        DeleteObject(localNodeObject, localRealTableName, ref env);
                    }
                }
                else
                {
                    //we dont support normal attribute here
                }
            }//end for loop
            nodeObjSql = nodeObjSql.Trim(',');
            whereSql = whereSql.Trim();
            if (!string.IsNullOrEmpty(whereSql))
            {
                nodeObjSql = nodeObjSql + " WHERE " + whereSql;
            }

            using (SqlCommand sqlCmd = new SqlCommand(nodeObjSql, conn))
            {
                sqlCmd.ExecuteNonQuery();
            }

        }









    }

}