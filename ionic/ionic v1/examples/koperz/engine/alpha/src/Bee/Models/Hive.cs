using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace Bee
{
    public class Hive
    {
        public static dynamic hive;
        public static JObject hiveJObject;
        public static bool Exists = false;
        public static string masterConnectionString = ""; //string connectionString = "Server=(local)\\netsdk;uid=sa;pwd=;database=master";
        public static Dictionary<string,string> HoneyTypes;

        public static JObject SecurityCombs = null;
        public static JObject ChunkCombs = null;
        public static bool hasCopiedSecurityCombs = false;
        public static bool hasCopiedChunkCombs = false;
        public static JObject NativeFlowers = null;
        public static string Mood = "pro";
        public static bool isSeedding = false;



        
            


        public static void MakeMasterConnectionString()
        {
            String location = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationLocationNodeName];
            String[] locationParts = location.Split(';');
            String server = locationParts.FirstOrDefault<string>(l => l.Contains("Source"));
            String uid = locationParts.FirstOrDefault<string>(l => l.Contains("ID"));
            String pwd = locationParts.FirstOrDefault<string>(l => l.Contains("Password"));
            string connString = server + ";" + uid + ";" + pwd;  //"server=201.10.22.4;uid=*;pwd=*;database=master";
            Bee.Hive.masterConnectionString = connString;
        }


        public static void CheckSecurityCombs()
        {
            if (Bee.Hive.SecurityCombs == null)
            {
                string sc = @"{
                    ""Role"" : ""pkRoleId strName strDescription"",
                    ""Access"" : ""pkAccessId intRoleId strCombName intCanDo"",
                    ""UserInRole"": ""pkUserInRoleId intRoleId intUserId""
                }";
                Bee.Hive.SecurityCombs = JObject.Parse(sc);
            }
        }

        public static void CheckChunkCombs()
        {
            if (Bee.Hive.ChunkCombs == null)
            {
                string bc = @"{
                    ""BeeChunk"" : ""pkBeeChunkId intUserId str_Chunk intPage str_FileKey""
                }";
                Bee.Hive.ChunkCombs = JObject.Parse(bc);
            }
        }

        public static void CheckHoneyTypes()
        {
            if (Bee.Hive.HoneyTypes == null)
            {
                HoneyTypes = new Dictionary<string, string>();
                
                Bee.Hive.HoneyTypes.Add("inv_pk", "[int] IDENTITY(1,1) NOT NULL");
                Bee.Hive.HoneyTypes.Add("pk", "[int] IDENTITY(1,1) NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_int_", "[int] NULL");
                Bee.Hive.HoneyTypes.Add("inv_int", "[int] NOT NULL");
                Bee.Hive.HoneyTypes.Add("int_", "[int] NULL");
                Bee.Hive.HoneyTypes.Add("int", "[int] NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_dbl_", "[float] NULL");
                Bee.Hive.HoneyTypes.Add("inv_dbl", "[float] NOT NULL");
                Bee.Hive.HoneyTypes.Add("dbl_", "[float] NULL");
                Bee.Hive.HoneyTypes.Add("dbl", "[float] NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_str_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("inv_str", "[nvarchar](max) NOT NULL");
                Bee.Hive.HoneyTypes.Add("str_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("str", "[nvarchar](max) NOT NULL");

                //nca
                //img for image
                Bee.Hive.HoneyTypes.Add("inv_img_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("inv_img", "[nvarchar](max) NOT NULL");
                Bee.Hive.HoneyTypes.Add("img_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("img", "[nvarchar](max) NOT NULL");
                //fle for file
                Bee.Hive.HoneyTypes.Add("inv_fle_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("inv_fle", "[nvarchar](max) NOT NULL");
                Bee.Hive.HoneyTypes.Add("fle_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("fle", "[nvarchar](max) NOT NULL");
                //doc for document
                Bee.Hive.HoneyTypes.Add("inv_doc_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("inv_doc", "[nvarchar](max) NOT NULL");
                Bee.Hive.HoneyTypes.Add("doc_", "[nvarchar](max) NULL");
                Bee.Hive.HoneyTypes.Add("doc", "[nvarchar](max) NOT NULL");
                //end nca


                Bee.Hive.HoneyTypes.Add("inv_dte_", "[datetime] NULL");
                Bee.Hive.HoneyTypes.Add("inv_dte", "[datetime] NOT NULL");
                Bee.Hive.HoneyTypes.Add("dte_", "[datetime] NULL");
                Bee.Hive.HoneyTypes.Add("dte", "[datetime] NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_enm_", "[int]  NULL");
                Bee.Hive.HoneyTypes.Add("inv_enm", "[int] NOT NULL");
                Bee.Hive.HoneyTypes.Add("enm_", "[int]  NULL");
                Bee.Hive.HoneyTypes.Add("enm", "[int] NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_bol_", "[bit] NULL");
                Bee.Hive.HoneyTypes.Add("inv_bol", "[bit] NOT NULL");
                Bee.Hive.HoneyTypes.Add("bol_", "[bit] NULL");
                Bee.Hive.HoneyTypes.Add("bol", "[bit] NOT NULL");

                Bee.Hive.HoneyTypes.Add("inv_fk", "[int] NOT NULL");
                Bee.Hive.HoneyTypes.Add("fk", "[int] NOT NULL");
                
            }
        }

        public static void CheckHiveMood()
        {
            var mood = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationMoodNodeName];
            if (mood != null)
            {
                Bee.Hive.Mood = (mood != null && ((((string)mood).Equals("dev")) || ((string)mood).Equals("pro"))) ? (string)mood : Bee.Hive.Mood;
            }
        }

        public static bool Can(int perm, string realCombName, ref Environment env)
        {
            //check if its the env
            if (env.IsSelf)
            {
                return true;
            }

            //check if we havw a security flag in the hive
            var securityNodeDefinition = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
            if (securityNodeDefinition == null)
            {
                return true; //we are not interested in security
            }

            //check for exclusions
            //cbh
            JToken exclusionString = Bee.Engine.Hive.SelectToken("security.exclude." + realCombName);
            if (exclusionString == null || exclusionString.Type == JTokenType.Null)
            {
                //continue
            }
            else
            {
                //what is excluded about this table
                if (exclusionString.Type == JTokenType.String)
                {
                    //get all the parts of this string
                    string[] pts = ((String)exclusionString).Trim().Split(' ');
                    if (pts.Contains(perm.ToString()))
                    {
                        return true;
                    }
                }
                else
                {
                    //nyd
                    //what happens when the exclusion is an object
                }
            }

            if (env.CurrentUser == null)
            {
                return false;
            }

            JToken uirsT = env.CurrentUser["UserInRoles"];
            if (uirsT == null)
            {
                return false;
            }
            JArray uirs = (JArray)uirsT;
            foreach (JToken uir in uirs)
            {
                JToken rT = (JObject)uir["Role"];
                if (rT == null)
                {
                    continue;
                }

                JToken acsT = rT["Accesses"];
                JArray acs = (JArray)acsT;
                if (acs == null)
                {
                    continue;
                }

                foreach (JToken ac in acs)
                {
                    string cn = (string)ac["CombName"];
                    int cd = (int)ac["CanDo"];

                    if (cn.Equals("*") && cd == -1)//all combs and all permissions of CRUD
                    {
                        return true;
                    }

                    if (cn.Equals("*") && cd == perm) //all combs and permission 
                    {
                        return true;
                    }

                    if (cn.Equals(realCombName) && cd == perm) //this comb and permission
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool CanCreate(String realCombName, ref Environment env)
        {
            bool cn = Bee.Hive.Can(1, realCombName, ref env);
            return cn;
        }

        public static bool CanRead(String realCombName, ref Environment env)
        {
            bool cn = Bee.Hive.Can(2, realCombName, ref env);
            return cn;
        }

        public static bool CanUpdate(String realCombName, ref Environment env)
        {
            bool cn = Bee.Hive.Can(3, realCombName, ref env);
            return cn;
        }

        public static bool CanDelete(String realCombName, ref Environment env)
        {
            bool cn = Bee.Hive.Can(4, realCombName, ref env);
            return cn;
        }


        public static void PlantNativeFlowers()
        {
            if (Bee.Hive.NativeFlowers == null)
            {
                //nca
                //all the native flowers work with this security node
                //without it errors are thrown around like carzy
                JToken t = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
                if (t == null || t.Type == JTokenType.Null)
                {
                    return;
                }
                //end nca

                string userComb = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserCombNodeName];
                string usernameCell = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserNameCellNodeName];
                string passwordCell = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityPasswordCellNodeName];
                string secretCode = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecuritySecretPotionNodeName];
                string primaryKeyCell = Bee.Hive.GetPK(userComb);


                string loginFlower = @"{
                                        '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.GetActionName + @"',
                                        '" + Bee.Engine.RawNectorName + @"' : {
                                            '" + userComb + @"': {
                                                '" + Bee.Scents.Attribute + @"' : '" + usernameCell + @" " + passwordCell + @" +Token ',
                                                '_jstrEncPassword': 'encrypt _@password _" + secretCode + @" ',
                                                '" + Bee.Scents.Where + @"':{
                                                    '_$" + usernameCell + @"_e' : '_@username',
                                                    '_and__$" + passwordCell + @"_e' : '_jstrEncPassword',
                                                    '_and__$Status_e' : 'ok'
                                                },
                                                '_j_joined' : 'join " + usernameCell + @" _:  _@password  ',
                                                '_j_Token' : 'encrypt _j_joined _" + secretCode + @" ' 
                                            }
                                        }
                                    }";
                var x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.LoginNativeFlowerName];
                if (x == null)
                {
                    JObject loginFlowerJObject = JObject.Parse(loginFlower);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.LoginNativeFlowerName, loginFlowerJObject);
                }

                string getUserOfToken = @"{
                                            '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.GetActionName + @"',
                                            '" + Bee.Engine.RawNectorName + @"' : {
                                                '" + userComb + @"': {
                                                    '" + Bee.Scents.Attribute + @"' : '*',
                                                    '_jstrJoined' : 'dencrypt _@token _" + secretCode + @" ',
                                                    '_lstrSplits' : 'split _jstrJoined _: ',
                                                    '_jstrEncPassword': 'encrypt _lstrSplits[1] _" + secretCode + @" ',
                                                    '" + Bee.Scents.Where + @"':{
                                                        '_$" + usernameCell + @"_e' : '_lstrSplits[0]',
                                                        '_and__$" + passwordCell + @"_e' : '_jstrEncPassword'
                                                    },
                                                    'UserInRoles' : [{ 
                                                        'Role': { 
                                                            'Accesses':[{}]
                                                        }
                                                    }],
                                                }
                                            }
                                        }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.GetUserOfTokenNativeFlowerName];
                if (x == null)
                {
                    JObject getUserOfTokenJObject = JObject.Parse(getUserOfToken);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.GetUserOfTokenNativeFlowerName, getUserOfTokenJObject);
                }

                //User": "pkUserId strName strEmail strPhoneNumber inv_strUserName inv_strPassword strStatus strCode dteCodeDate dteCodeExpiryDate",
                string statusCell = (String)Bee.Engine.Hive.SelectToken("security.statusCell");
                string codeCell = (String)Bee.Engine.Hive.SelectToken("security.codeCell");
                string codeDateCell = (String)Bee.Engine.Hive.SelectToken("security.codeDateCell");
                string codeDateExpiryCell = (String)Bee.Engine.Hive.SelectToken("security.codeExpiryCell");
                string phoneNumberCell = (String)Bee.Engine.Hive.SelectToken("security.phoneNumberCell");
                string emailCell = (String)Bee.Engine.Hive.SelectToken("security.emailCell");
                //get the verification node details
                //"verify":{
                //    "method":"sms&email",
                //    "codeLength" : 4,
                //    "codeType" : "digits",
                //    "emailTemplate": "",
                //    "smsTemplate": "",
                //    "expiry": "30m"
                //},
                string verifMethod = (String)Bee.Engine.Hive.SelectToken("security.verify.method");
                string smsTemplate = (String)Bee.Engine.Hive.SelectToken("security.verify.smsTemplate");
                string emailTemplate = (String)Bee.Engine.Hive.SelectToken("security.verify.emailTemplate");
                string verificationExpiry = (String)Bee.Engine.Hive.SelectToken("security.verify.expiry");
                string veriEmailSubject = (String)Bee.Engine.Hive.SelectToken("security.verify.subject");
                string verificationCode = (String)Bee.Engine.Hive.SelectToken("security.verify.code");
                bool doSms = false;
                bool doemail = false;
                if (verifMethod == null)
                {
                    doSms = false;
                    doemail = false;
                }else if (verifMethod.Equals("sms&email") || verifMethod.Equals("email&sms"))
                {
                    doSms = true;
                    doemail = true;
                }
                else if (verifMethod.Equals("sms"))
                {
                    doSms = true;
                    doemail = false;
                }
                else if (verifMethod.Equals("email"))
                {
                    doSms = false;
                    doemail = true;
                }

                string register = @"{
                                    '" + Bee.Engine.ActionName + @"' : '"+ Bee.Engine.PostActionName +@"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        '" + userComb + @"': {
                                            '_a" + usernameCell + @"' : '_@username',
                                            '_a" + passwordCell + @"' : 'encrypt _@password _@h_security_secretPotion',
                                            '" + statusCell + @"' : '"+((doSms == false && doemail == false)?"ok":"notverified")+@"',
                                            '_a" + codeCell + @"' : ' " + ((doSms == false && doemail == false)?"getrandomecode _5" : "getcode _" + verificationCode ) + @" ',
                                            '_a" + codeDateCell + @"' : '_@d_now',
                                            '_a" + codeDateExpiryCell + @"' : '_@d_now_add_" + ((doSms == false && doemail == false) ? "30m" : verificationExpiry) + @"',
                                            '_ro_': 'User'
                                        },
                                        'UserInRoles': [{
                                            '_aRoleId' : '_@h_security_defaultRole',
			                                '_aUserId' : '_oo_User_UserId',
                                            '_ro_': 'UserInRoles1'
		                                }] " + ((doSms) ? 
                                        @",'_j_strVerCode': 'sendsms _" + phoneNumberCell + @" _" + smsTemplate + @"' " : "") + ((doemail) ?
                                        @",'_pstrSubject' : '"+veriEmailSubject+@"',
                                           '_j_strVerEmail': 'sendemail " + emailCell + @" _pstrSubject _" + emailTemplate + @"' " : "") + @"
                                    }
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.RegisterNativeFlowerName];
                if (x == null)
                {
                    JObject registerJObject = JObject.Parse(register);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.RegisterNativeFlowerName, registerJObject);
                }

                string updatePasword = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.UpdateActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        '" + userComb + @"': {
                                            '_a" + passwordCell + @"' : 'encrypt _@newpassword _@h_security_secretPotion',
                                            '_jstrEncOldPassword' : 'encrypt _@oldpassword _@h_security_secretPotion',
                                            '" + Bee.Scents.Where + @"': {
                                                '_$" + passwordCell + @"_e' : '_jstrEncOldPassword',
                                                '_and__$" + statusCell + @"_e' : 'ok',
                                                '_and__$" + primaryKeyCell + @"_e' : '_@u_" + primaryKeyCell + @"'
                                            }
                                        }
                                    } 
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.ChangePasswordNativeFlowerName];
                if (x == null)
                {
                    JObject updateJObject = JObject.Parse(updatePasword);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.ChangePasswordNativeFlowerName, updateJObject);
                }


                string recoveryMethod = (String)Bee.Engine.Hive.SelectToken("security.recover.method");
                string smsRecoverTemplate = (String)Bee.Engine.Hive.SelectToken("security.recover.smsTemplate");
                string emailRecoverTemplate = (String)Bee.Engine.Hive.SelectToken("security.recover.emailTemplate");
                string recoverExpiry = (String)Bee.Engine.Hive.SelectToken("security.recover.expiry");
                string recoverEmailSubject = (String)Bee.Engine.Hive.SelectToken("security.recover.subject");
                string recoverCode = (String)Bee.Engine.Hive.SelectToken("security.recover.code");
                bool useSms = false;
                bool useEmail = false;
                string attStr ="";
                string whereStr ="";
                if (recoveryMethod == null)
                {
                    useSms = false;
                    useEmail = false;
                }else if (recoveryMethod.Equals("sms&email"))
                {
                    useSms = true;
                    useEmail = true;
                    attStr =  emailCell + " " +  phoneNumberCell;
                    whereStr = "'_$" + emailCell + "_e' : '_@email', ";
                    whereStr += "'_and__$" + phoneNumberCell + "_e' : '_@phonenumber' ";
                }
                else if (recoveryMethod.Equals("sms"))
                {
                    useSms = true;
                    useEmail = false;
                    attStr = phoneNumberCell;
                    whereStr = "'_$" + phoneNumberCell + "_e' : '_@phonenumber' ";
                }
                else if (recoveryMethod.Equals("email"))
                {
                    useSms = false;
                    useEmail = true;
                    attStr = emailCell;
                    whereStr = "'_$" + emailCell + "_e' : '_@email'";
                }
                string recoverPasword = @"{
                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.UpdateActionName + @"',
                    '" + Bee.Engine.RawNectorName + @"' : {
                        '" + userComb + @"': { 
                            '_a" + codeCell + @"' : 'getcode _" + recoverCode + @" ',
                            '_a" + codeDateCell + @"' : '_@d_now',
                            '_a" + codeDateExpiryCell + @"' : '_@d_now_add_" + recoverExpiry + @"',
                            '" + Bee.Scents.Where + @"': {
                                " + whereStr + @",
                                '_and__$"+ statusCell+@"_e' : 'ok'
                            } " + ((useEmail) ?
                            @",'_pstrSubject' : '" + recoverEmailSubject + @"',
                               '_j_strVerEmail': 'sendemail _@email _pstrSubject _" + emailRecoverTemplate + @"' " : "") + ((useSms) ?
                            @",'_j_strVerCode': 'sendsms _@phonenumber  _" + smsRecoverTemplate + @"' " : "") + @"
                        }
                    } 
                }";
                //'" + Bee.Scents.Attribute + @"':' " + attStr + @" +',
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.ForgotPasswordNativeFlowerName];
                if (x == null)
                {
                    JObject recoverJObject = JObject.Parse(recoverPasword);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.ForgotPasswordNativeFlowerName, recoverJObject);
                }



                string createNewPasword = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.UpdateActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        '" + userComb + @"': {
                                            '_a" + passwordCell + @"' : 'encrypt _@newpassword _@h_security_secretPotion',
                                            '_a" + codeCell + @"' : 'getrandomecode _10',
                                            '" + Bee.Scents.Where + @"': {
                                                '_$" + codeCell + @"_e' : '_@code', 
                                                '_and__$" + statusCell + @"_e' : 'ok', 
                                                '_and__$" + codeDateExpiryCell + @"_gtoe' : '_@d_now'" + ((useEmail) ?
                                                @",'_and__$" + emailCell + @"_e' : '_@email' " : "") + ((useSms) ?
                                                @",'_and__$" + phoneNumberCell + @"_e' : '_@phonenumber' " : "") + @" 
                                            }
                                        }
                                    } 
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.NewPasswordNativeFlowerName];
                if (x == null)
                {
                    JObject newPasswordJObject = JObject.Parse(createNewPasword);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.NewPasswordNativeFlowerName, newPasswordJObject);
                }


                string activateAccount = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.UpdateActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        '" + userComb + @"': {
                                            '" + statusCell + @"' : 'ok',
                                            '_a" + codeCell + @"' : 'getrandomecode _10',
                                            '" + Bee.Scents.Where + @"': {
                                                '_$" + codeCell + @"_e' : '_@code', 
                                                '_and__$" + codeDateExpiryCell + @"_gtoe' : '_@d_now',
                                                '_and__$Status_e' : 'notverified'" + ((doemail) ?
                                                @",'_and__$" + emailCell + @"_e' : '_@email' " : "") + ((doSms) ?
                                                @",'_and__$" + phoneNumberCell + @"_e' : '_@phonenumber' " : "") + @" 
                                            }
                                        }
                                    } 
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.AccivateAccountNativeFlowerName];
                if (x == null)
                {
                    JObject activateAccountJObject = JObject.Parse(activateAccount);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.AccivateAccountNativeFlowerName, activateAccountJObject);
                }




                //clear user chunks
                string clearChunks = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.DeleteActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        'BeeChunk': {
                                            '" + Bee.Scents.Where + @"': {
                                                '_$UserId_e' : '_@u_" + primaryKeyCell + @"'
                                            }
                                        }
                                    } 
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.ClearChunksNativeFlowerName];
                if (x == null)
                {
                    JObject clearChunksJObject = JObject.Parse(clearChunks);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.ClearChunksNativeFlowerName, clearChunksJObject);
                }
                
                //add users chunks
                string addChunk = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.PostActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        'BeeChunk': {
                                            '_aUserId' : '_@u_" + primaryKeyCell+ @"',
                                            '_aChunk' : '_@chunk',
                                            '_aPage' : '_@page',
                                            '_aFileKey' : '_@filekey'
                                        }
                                    }
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.AddChunksNativeFlowerName];
                if (x == null)
                {
                    JObject addChunkJObject = JObject.Parse(addChunk);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.AddChunksNativeFlowerName, addChunkJObject);
                }


                //add a file from chunks
                string makeFile = @"{
                                    '" + Bee.Engine.ActionName + @"' : '" + Bee.Engine.GetActionName + @"',
                                    '" + Bee.Engine.RawNectorName + @"' : {
                                        'BeeChunk': [{
                                            '_qString':'Chunk',
                                            '_w':{'_$UserId_e' : '_@u_" + primaryKeyCell + @"'}
                                        }]
                                    }
                                }";
                //plant this native flower
                x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName])[Bee.Engine.MakeFileNativeFlowerName];
                if (x == null)
                {
                    JObject makeFileJObject = JObject.Parse(makeFile);
                    ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationFlowersNodeName]).Add(Bee.Engine.MakeFileNativeFlowerName, makeFileJObject);
                }

            }
        }


        

        public static void ReadHiveDefinition()
        {
            if (Bee.Engine.Hive == null)
            {
                String hiveFileName = "Hive.json";
                string filesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
                String hiveFilePath = filesPath + Path.GetFileName(hiveFileName);
                //check if file exists
                if (File.Exists(hiveFilePath))
                {
                    string hiveDefinition = "";
                    using (StreamReader sr = new StreamReader(hiveFilePath))
                    {
                        hiveDefinition = sr.ReadToEnd();
                    }
                    Bee.Engine.Hive = (JObject)JValue.Parse(hiveDefinition);
                }
                else
                {
                    throw new Exception("Hive.json file not found at the root");
                }
                Bee.Hive.MakeMasterConnectionString();
            }
        }

        public static string GetDbName()
        {
            String location = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationLocationNodeName];
            String[] locationParts = location.Split(';');
            String dbName = locationParts.FirstOrDefault<string>(l => l.Contains("Catalog"));
            String[] nameParts = dbName.Split('=');
            String realDbName = nameParts[1];
            return realDbName;
        }

        public static void Save(String body, String sizePath, String uniqueFileName, String host, string key, string dbName, MemoryStream outStream, ref Environment env)
        {
            String fileNameToSave = sizePath + Path.GetFileName(uniqueFileName);
            // Do something with the stream.
            using (var fileStream = new FileStream(sizePath + uniqueFileName, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                outStream.Position = 0;
                outStream.CopyTo(fileStream);
                string url = "http://" + host + "/" + dbName + "/" +  body + "/" +  uniqueFileName;
                //for the sake of unloading in case of exceptions
                env.Uploads.Add(new Upload()
                {
                    FileParameterName = key,
                    UploadedFileName = uniqueFileName,
                    FullSavedFileName = fileNameToSave,
                    ToBeSaved = url
                });
                //an immediate jelly
                if (!env.ImmediateJellyValues.ContainsKey(Bee.Scents.JellyImmediate+key))
                {
                    env.ImmediateJellyValues.Add(Bee.Scents.JellyImmediate + key, uniqueFileName);
                }
            }
        }

        public static void Save(String body, String filesPath, String uniqueFileName, String host, string key, string dbName, HttpPostedFile postedFile, ref Environment env)
        {
            String fileNameToSave = filesPath + Path.GetFileName(uniqueFileName);
            postedFile.SaveAs(fileNameToSave);
            string url = "http://" + host + "/" + dbName + "/" + body + "/" + uniqueFileName;
            env.Uploads.Add(new Upload()
            {
                FileParameterName = key,
                UploadedFileName = uniqueFileName,
                FullSavedFileName = fileNameToSave,
                ToBeSaved = url
            });
            //an immediate jelly
            if (!env.ImmediateJellyValues.ContainsKey(Bee.Scents.JellyImmediate + key))
            {
                env.ImmediateJellyValues.Add(Bee.Scents.JellyImmediate + key, uniqueFileName);
            }
        }

        

        public static void CreateHive()
        {
            //check if db doesnot exist and then create it
            if (Bee.Hive.Exists == false)
            {
                String realDbName = Bee.Hive.GetDbName();
                bool doesDbExist = ChechHive(realDbName);
                if (doesDbExist == false)
                {
                    //create the database
                    Bee.Hive.CreateHive(realDbName);
                    //create tables
                    Bee.Hive.CreateCells((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName]);
                    //create a chunks table for those that use bee.upload()
                    Bee.Hive.CreateChunkCombs();
                    //boot strap security tables
                    Bee.Hive.CreateDroneSecurity();
                    //seed the hive with initial honey
                    Bee.Hive.Seed();
                }

                Bee.Hive.Exists = true;
            }
        }

        public static void InsertSecurityCombs()
        {
            //copy securitycombs to hive
            var hasDroneSecurity = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
            if (hasDroneSecurity != null)
            {
                foreach (var securityCombNode in Bee.Hive.SecurityCombs)
                {
                    var x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[securityCombNode.Key];
                    if (x == null)
                    {
                        ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName]).Add(securityCombNode.Key, securityCombNode.Value);
                    }
                }
            }
        }

        public static void InsertChunkCombs()
        {
            if (Bee.Hive.hasCopiedChunkCombs == false)
            {
                //copy chunk combs to hive
                foreach (var chunkCombNode in Bee.Hive.ChunkCombs)
                {
                    var x = ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[chunkCombNode.Key];
                    if (x == null)
                    {
                        ((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName]).Add(chunkCombNode.Key, chunkCombNode.Value);
                    }
                }
                Bee.Hive.hasCopiedChunkCombs = true;
            }
        }

        public static SqlConnection GetConnection()
        {



            Bee.Hive.CheckSecurityCombs();

            Bee.Hive.CheckChunkCombs();

            Bee.Hive.CheckHoneyTypes();

            Bee.Hive.ReadHiveDefinition();

            Bee.Hive.CheckHiveMood();

            //check if it init is set to false
            bool init = true;
            JToken jInitToken = Bee.Engine.Hive["init"];
            if (jInitToken == null || jInitToken.Type == JTokenType.Null)
            {
                init = true;
            }
            else if (Convert.ToBoolean(jInitToken) == false)
            {
                init = false;
            }else{
                init = true;
            }

            if (init == true)
            {
                Bee.Hive.CreateHive();
            }

            Bee.Hive.CheckHiveMood();

            //native flowers
            Bee.Hive.PlantNativeFlowers();

            if (init == true)
            {
                Bee.Hive.InsertSecurityCombs();
            }

            if (init == true)
            {
                Bee.Hive.InsertChunkCombs();
            }

            String connectionString = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationLocationNodeName];
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static void CreateHive(string dbName)
        {
            dbName = dbName.Trim();
            string cmdText = "CREATE DATABASE [" + dbName + "] ;";
            using (SqlConnection sqlConnection = new SqlConnection(Bee.Hive.masterConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCmd = new SqlCommand(cmdText, sqlConnection))
                {
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }

        public static string GetQuoted(string value, string target, string realCombName)
        {
            if (value.StartsWith(Bee.Scents.Scent))
            {
                return (value.StartsWith(Bee.Scents.Space)) ? " " : value;
            }

            if (String.IsNullOrEmpty(realCombName))
            {
                //it means that the target was a hive datatype e.g in evaluating _jstrToken ==> GetQuoted(val, 'str', null)
                return (Bee.CellTypes.Quotables.Contains(target)) ? "'" + value + "'" : value;
            }

            
            String []cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
            for (int i = 0; i < cellDefinitions.Length; i++)
            {
                string cellDefinition = cellDefinitions[i].Trim();
                if (!String.IsNullOrEmpty(cellDefinition))
                {
                    Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                    if (cell.Name.Equals(target))
                    { 
                        return (Bee.CellTypes.Quotables.Contains(cell.Type))? "'" + value + "'" : value;
                    }
                }
               
            }

            return value;
        }

        public static string GetQuoted(string value, JTokenType type)
        {
            if (value.StartsWith(Bee.Scents.Scent))
            {
                return (value.StartsWith(Bee.Scents.Space)) ? " " : value;
            }

            JTokenType[] quotables = { JTokenType.String, JTokenType.Date };
            return (quotables.Contains(type))? "'" + value + "'" : value;
        }




        public static string GetPK(string combName)
        {
            string realCombName = GetRealCombName(combName);
            String []cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
            for (int i = 0; i < cellDefinitions.Length; i++)
            {
                string cellDefinition = cellDefinitions[i].Trim();
                if (!String.IsNullOrEmpty(cellDefinition))
                {
                    Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                    if (cell.IsPrimaryKey)
                    {
                        return cell.Name;
                    }
                }
               
            }
            return "";
        }

        public static string GetFirstCellName(string combName)
        {
            string realCombName = GetRealCombName(combName);
            String[] cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
            for (int i = 0; i < cellDefinitions.Length; i++)
            {
                string cellDefinition = cellDefinitions[i].Trim();
                if (!String.IsNullOrEmpty(cellDefinition))
                {
                    Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                    return cell.Name;
                }

            }
            return "";
        }

        //public static void GetSomething(String combName, Delegate k)
        //{
        //    string realCombName = GetRealCombName(combName);
        //    String[] cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
        //    for (int i = 0; i < cellDefinitions.Length; i++)
        //    {
        //        string cellDefinition = cellDefinitions[i].Trim();
        //        if (!String.IsNullOrEmpty(cellDefinition))
        //        {
        //            Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
        //            k(cell);
        //        }
        //    }
        //}


        public static JObject GetFlower(string flowerName)
        {
            JObject honey = new JObject();
            JObject flowers = Bee.Hive.hive["flowers"];
            JObject flower = (JObject)flowers[flowerName];
            return flower;
        }

        public static string GetAttributes(string combName, string attributeFilter = "")
        {
            attributeFilter = attributeFilter.Trim();
            string attr = "";
            string realCombName = GetRealCombName(combName);
            String[] cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
            //attributeFilter = " " + attributeFilter;
            for (int i = 0; i < cellDefinitions.Length; i++)
            {
                string cellDefinition = cellDefinitions[i].Trim();
                if (!String.IsNullOrEmpty(cellDefinition))
                {
                    Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                    if (string.IsNullOrEmpty(attributeFilter) || attributeFilter.Equals(Bee.Scents.Everything))
                    {
                        attr = attr + " " + cell.Name;
                    }
                    else if (attributeFilter.Contains(Bee.Scents.Everything) && (
                        attributeFilter.Contains(Bee.Scents.Minus + cell.Name + " ") ||
                        attributeFilter.EndsWith(Bee.Scents.Minus + cell.Name ) ))
                    {
                       continue;
                    }
                    else if (!attributeFilter.Contains(Bee.Scents.Everything) && (attributeFilter.Contains(" " + Bee.Scents.Minus) || attributeFilter.StartsWith(Bee.Scents.Minus)))
                    {
                        bool isAllMinus = true;
                        //check if all attributes begin with -
                        //this is the same as * -aa -aa
                        string[] attParts = attributeFilter.Split(' ');
                        for (int atIndex = 0; atIndex < attParts.Length; atIndex++)
                        {
                            string attPart = attParts[atIndex];
                            attPart = attPart.Trim();
                            if (!String.IsNullOrEmpty(attPart) && !attPart.StartsWith(Bee.Scents.Minus))
                            {
                                isAllMinus = false;
                                break;
                            }
                        }
                        if (isAllMinus && (attributeFilter.Contains(Bee.Scents.Minus + cell.Name + " ") || attributeFilter.EndsWith(Bee.Scents.Minus + cell.Name)) )
                        {
                            continue;
                        }
                        else
                        {
                            attr = attr + " " + cell.Name;
                        }
                    }
                    else if (attributeFilter.Contains(Bee.Scents.Everything) && attributeFilter.Contains(Bee.Scents.Plus))
                    {
                        attr = attr + " " + cell.Name;
                    }
                    else if (attributeFilter.Contains(cell.Name))
                    {
                        string[] attParts = attributeFilter.Split(' ');
                        for (int atIndex = 0; atIndex < attParts.Length; atIndex++)
                        {
                            string attPart = attParts[atIndex];
                            attPart = attPart.Trim();
                            if (attPart.Equals(cell.Name))
                            {
                                attr = attr + " " + cell.Name;
                            }
                        }
                    }
                }
            }
            
            return attr;
        }

        public static string GetRealCombName(string combName)
        {
            string theOtherName = "";
            var p = PluralizationService.CreateService(new CultureInfo("en-US"));
            theOtherName =  (p.IsPlural(combName))? p.Singularize(combName) : p.Pluralize(combName);
            string realName = (Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName][combName] != null) ? combName :
                              ((Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName][theOtherName] != null) ? theOtherName : "");
            return realName;
        }

        public static void CreateCells(JObject combNodes)
        {
            String location = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationLocationNodeName];
            using (SqlConnection sqlConnection = new SqlConnection(location))
            {
                sqlConnection.Open();
                foreach (var combNode in combNodes)
                {
                    string sql = "CREATE TABLE [dbo].[" + combNode.Key +"] ( ";
                    string[] cellDefinitions = ((String)combNode.Value).Trim().Split(' ');
                    string pk = "";
                    for (int i = 0; i < cellDefinitions.Length; i++)
			        {
			            string cellDefinition = cellDefinitions[i].Trim();
                        if (!String.IsNullOrEmpty(cellDefinition))
                        {
                            Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                            sql += " [" + cell.Name + "] " + cell.TypeValue + ",";
                            pk = cell.IsPrimaryKey ? cell.Name : pk;
                        }
			        }
                    if (!string.IsNullOrEmpty(pk))
                    {
                        sql +=  @" CONSTRAINT ["+ pk + @"] PRIMARY KEY CLUSTERED ( [" + pk + @"] ASC ) WITH ( 
		                    PAD_INDEX = OFF, 
		                    STATISTICS_NORECOMPUTE = OFF, 
		                    IGNORE_DUP_KEY = OFF, 
		                    ALLOW_ROW_LOCKS = ON, 
		                    ALLOW_PAGE_LOCKS = ON
                        ) ON [PRIMARY] ";
                    }
                    else
                    {
                        sql = sql.Trim(',');
                    }
                    sql += " )";
                    sql += (!string.IsNullOrEmpty(pk)) ? " ON [PRIMARY] " : "";
                    sql += ";";
                    SqlCommand sqlCmd = new SqlCommand(sql, sqlConnection);
                    sqlCmd.ExecuteNonQuery();
                }              
                
            }

        }

        public static void ExtractType(string str, out string name, out string type)
        {
            string t = "";
            string n = "";
            foreach (KeyValuePair<string, string> hay in Bee.Hive.HoneyTypes)
            {
                string start = hay.Key;
                if (str.StartsWith(start))
                {
                    string invkey = "inv_";
                    t = (start.StartsWith(invkey))?start.Substring(invkey.Length):start;
                    n = str.Substring(start.Length);
                    break;
                }
            }
            type = t;
            name = n;
        }

        public static void ExtractColumn(string str, out string name, out string type, out bool isPk){
            string t = "";
            string n = "";
            isPk = false;
            foreach (KeyValuePair<string,string> hay in Bee.Hive.HoneyTypes)
	        {
                string start = hay.Key;
		        if(str.StartsWith(start)){ 
                    t = hay.Value;
                    n = str.Substring(start.Length);
                    if (start.Equals("pk") || start.Equals("inv_pk"))
                    {
                        isPk = true;
                    }
                    break;
                }
	        }
            type = t;
            name = n;
        }

        public static bool isInvisible(string cellName, string combName)
        {
            string realCombName = GetRealCombName(combName);
            String[] cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[realCombName]).Trim().Split(' ');
            for (int i = 0; i < cellDefinitions.Length; i++)
            {
                string cellDefinition = cellDefinitions[i].Trim();
                if (!String.IsNullOrEmpty(cellDefinition))
                {
                    Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                    if (cell.Name.Equals(cellName) && cell.Type.StartsWith(Bee.Coatings.Invisible))
                    { //cellDefinition.Contains(wouldbe)
                        return true;
                    }
                }
            }

            return false;
        }

        public static string getSecretPortion()
        {
            try
            {
                JObject x = (JObject)Bee.Engine.Hive["security"];
                if (x == null)
                {
                    return "";
                }
                JObject spNode = (JObject)x["secretPortion"];
                if (spNode == null)
                {
                    return "";
                }
                dynamic dynPort = spNode;
                string sp = dynPort;
                return sp;
            }
            catch (Exception ex)
            {
                return "";
            }
        }


        public static bool ChechHive(string dbName)
        {
            string cmdText = "SELECT * FROM master.dbo.sysdatabases WHERE name=\'" + dbName + "\'";
            bool bRet = false;
            using (SqlConnection sqlConnection = new SqlConnection(Bee.Hive.masterConnectionString))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCmd = new SqlCommand(cmdText, sqlConnection))
                {
                    SqlDataReader reader = sqlCmd.ExecuteReader();

                    if (reader == null || reader.HasRows == false)
                    {
                        bRet = false;
                    }
                    else
                    {
                        bRet = true;
                    }

                }
            }

            return bRet;
        }

        public static void CreateDroneSecurity()
        {
            var securityNodeDefinition = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
            if (securityNodeDefinition != null)
            {
                Bee.Hive.CreateCells(Bee.Hive.SecurityCombs);              
            }
        }


        public static void CreateChunkCombs()
        {
            String location = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationLocationNodeName];
            using (SqlConnection sqlConnection = new SqlConnection(location))
            {
                sqlConnection.Open();
                
                string sql = "CREATE TABLE [dbo].[BeeChunk] ( ";
                sql = sql + "[BeeChunkId] [int] IDENTITY(1,1) NOT NULL ,";
                sql = sql + "[UserId] [int] NOT NULL ,";
                sql = sql + "[Chunk] [nvarchar](max) NULL ,";
                sql = sql + "[Page] [int] NOT NULL ,";
                sql = sql + "[FileKey] [nvarchar](max) NULL  ,";
                sql = sql + @" CONSTRAINT [BeeChunkId] PRIMARY KEY CLUSTERED ( [BeeChunkId] ASC ) WITH ( 
		                    PAD_INDEX = OFF, 
		                    STATISTICS_NORECOMPUTE = OFF, 
		                    IGNORE_DUP_KEY = OFF, 
		                    ALLOW_ROW_LOCKS = ON, 
		                    ALLOW_PAGE_LOCKS = ON
                        ) ON [PRIMARY] ";
                sql += " ) ON [PRIMARY];";
                    
                SqlCommand sqlCmd = new SqlCommand(sql, sqlConnection);
                sqlCmd.ExecuteNonQuery();
                
            }
        }
        
        public static void Seed()
        {
            Bee.Hive.isSeedding = true;
            JObject seeds = (JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSeedsNodeName];
            if (seeds != null)
            {
                Environment seedingEnvironment = new Environment();
                seedingEnvironment.IsSelf = true;
                seedingEnvironment.IsPosting = true;              
                Bee.Workers.ProcessPost(seeds, seedingEnvironment);
            }
            Bee.Hive.isSeedding = false;
        }


    }
}