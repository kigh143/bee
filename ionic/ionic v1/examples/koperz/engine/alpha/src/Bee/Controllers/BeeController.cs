using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Bee.Models;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;
using ImageProcessor;
using System.Web.Http.Cors;

namespace Bee.Controllers
{
    [EnableCors("*", "*", "*")]
    public class BeeController : ApiController
    {

        public object Post()
        {
            JObject honey = new JObject();
            JObject nector = new JObject();
            List<string> errors = new List<string>();
            String way = "";
            String auth = "";
            bool isNectorFine = false;
            bool has_errosNode = false;
            Environment environment = new Environment();

            

            try
            {
                var x = Request.RequestUri;
                //does he want to get, post, update or delete data
                way = System.Web.HttpContext.Current.Request.Form.GetValues(Bee.Engine.ActionName)[0];
            }
            catch (Exception ex)
            {
                errors.Add("Unknown public api, please use get, post, update or delete");
            }

            try
            {
                auth = System.Web.HttpContext.Current.Request.Form.GetValues(Bee.Engine.AuthName)[0];
                auth = (!String.IsNullOrEmpty(auth))? auth.Trim() : "";
            }
            catch (Exception ex)
            {
                errors.Add("could not samon woker bees for drone security");
            }

            bool intendsToDisplayErrors = false;
            try
            {
                String encodeQuery = System.Web.HttpContext.Current.Request.Form.GetValues(Bee.Engine.RawNectorName)[0];
                byte[] queryBytes = Convert.FromBase64String(encodeQuery);
                string nectorJsonString = Encoding.ASCII.GetString(queryBytes);
                intendsToDisplayErrors = nectorJsonString.Contains(Bee.Scents.Errors);
                nector = JObject.Parse(nectorJsonString);
                isNectorFine = true;
            }
            catch (Exception ex)
            {
                errors.Add("Your nector has issues: " + ex.Message);
                isNectorFine = false;
            }

            //check if you need to display errors
            //or if you already have errors in your requests
            var errorsNode = nector.SelectToken(Bee.Scents.Errors);
            if (errorsNode != null || intendsToDisplayErrors == true)
            {
                has_errosNode = true;
                honey.Add(Bee.Scents.Errors, new JArray());
            }
            else
            {
                honey.Add(Bee.Scents.Errors, new JArray());
            }
            if (errors.Count() > 0)
            {
                foreach (var item in errors)
                {
                    ((JArray)honey["_errors"]).Add(item);
                }
                return honey;
            }

            try
            {
                //boot a hive
                Bee.Hive.GetConnection();
                var t = nector[Bee.Scents.Flower + Bee.Engine.LoginNativeFlowerName];
                if (t != null && String.IsNullOrEmpty(auth))
                {
                    //execute only this part
                    String username = (String)t["username"];
                    String password = (String)t["password"];
                    JObject UserNec = new JObject();
                    UserNec.Add("username", username);
                    UserNec.Add("password", password);

                    var nct = @"{" +Bee.Scents.Flower + Bee.Engine.LoginNativeFlowerName + @": {
                            'username': '"+username+@"',
                            'password': '"+password+@"'
                        },
                        _errors: []
                    }";
                    Environment loginEnv = new Environment();
                    loginEnv.Nector = (JObject)JValue.Parse(nct);
                    loginEnv.Auth = auth;
                    loginEnv.IsFlowerCall = true;
                    loginEnv.IsSelf = true;
                    Bee.Workers.VisitFlower(Bee.Engine.LoginNativeFlowerName, UserNec, ref loginEnv);
                    honey  = loginEnv.Honey;
                    loginEnv.IsFlowerCall = false;
                    loginEnv.IsSelf = false;
                    return honey;
                }

                //is registration open
                //oc JToken jtx = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityIsRegistrationOpenNodeName];
                //nca
                JToken tempJTX = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
                JToken jtx = null;
                if (tempJTX == null || tempJTX.Type == JTokenType.Null)
                {
                    jtx = null;
                }
                else
                {
                    jtx = tempJTX[Bee.Engine.RawHiveDefiniationSecurityIsRegistrationOpenNodeName];
                }
                //end nca
                //oc bool isRegistrationOpen = (Boolean)(jtx);
                //nca
                bool isRegistrationOpen = (jtx == null || jtx.Type == JTokenType.Null)? false : (Boolean)(jtx);
                //end nca 
                var tt = nector[Bee.Scents.Flower + Bee.Engine.RegisterNativeFlowerName];
                var act = nector[Bee.Scents.Flower + Bee.Engine.AccivateAccountNativeFlowerName];
                var fgt = nector[Bee.Scents.Flower + Bee.Engine.ForgotPasswordNativeFlowerName];
                var npt = nector[Bee.Scents.Flower + Bee.Engine.NewPasswordNativeFlowerName];
                var addChunk = nector[Bee.Scents.Flower + Bee.Engine.AddChunksNativeFlowerName];
                var clearChunk = nector[Bee.Scents.Flower + Bee.Engine.ClearChunksNativeFlowerName];
                var makefile = nector[Bee.Scents.Flower + Bee.Engine.MakeFileNativeFlowerName];
                if (tt != null && String.IsNullOrEmpty(auth) && isRegistrationOpen == true)
                {
                    //execute only this part
                    String username = (String)tt["username"];
                    String password = (String)tt["password"];
                    JObject UserNec = new JObject();
                    UserNec.Add("username", username);
                    UserNec.Add("password", password);

                    bool hasExtension = false;
                    String ext = "_: [{";
                    JToken foo = tt["_"];
                    if (foo != null)
                    {
                        JToken extArra = ((JArray)foo)[0];
                        //extend this but with only those things that go to the users comb
                        string usernameCell = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserNameCellNodeName];
                        string passwordCell = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityPasswordCellNodeName];
                        string userComb = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserCombNodeName];
                        String[] cellDefinitions = ((String)((JObject)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationCombNodeName])[userComb]).Trim().Split(' ');
                        for (int i = 0; i < cellDefinitions.Length; i++)
                        {
                            string cellDefinition = cellDefinitions[i].Trim();
                            if (!String.IsNullOrEmpty(cellDefinition))
                            {
                                Bee.Cell cell = Bee.CellTypes.Resolve(cellDefinition);
                                JToken jt = extArra[cell.Name];
                                if (jt != null && !cell.Name.Equals(usernameCell) && !cell.Name.Equals(passwordCell))
                                {

                                    string v = (String)extArra[cell.Name];
                                    string vq = Bee.Hive.GetQuoted(v, jt.Type);
                                    ext += "'" + cell.Name + "': " + vq + ",";
                                    hasExtension = true;
                                }else if(jt == null){
                                    JToken activeAttribute = extArra[Bee.Scents.Attribute+cell.Name];
                                    if (activeAttribute != null)
                                    {
                                        string v = (String)extArra[Bee.Scents.Attribute + cell.Name];
                                        ext += "'" + Bee.Scents.Attribute + cell.Name + "': '" + v + "',";
                                        hasExtension = true;
                                    }
                                }
                            }

                        }
                        ext = ext.Trim(',');
                        ext += "}]";
                    }

                    var nct = @"{" + Bee.Scents.Flower + Bee.Engine.RegisterNativeFlowerName + @": {
                            'username': '" + username + @"',
                            'password': '" + password + @"'
                        }," + ((hasExtension)? ext + "," : "" ) + @"
                        _errors: []
                    }";
                    UserNec = (JObject)JValue.Parse(nct);
                    Environment regEnv = new Environment();
                    Upload(ref regEnv);
                    regEnv.Nector = (JObject)JValue.Parse(nct);
                    regEnv.Auth = auth;
                    regEnv.IsFlowerCall = true;
                    regEnv.IsSelf = true;

                    Bee.Workers.VisitFlower(Bee.Engine.RegisterNativeFlowerName, UserNec, ref regEnv);
                    
                    regEnv.IsSelf = false;
                    regEnv.IsFlowerCall = false;


                    //login
                    //execute only this part
                    UserNec = new JObject();
                    UserNec.Add("username", username);
                    UserNec.Add("password", password);

                    nct = @"{" + Bee.Scents.Flower + Bee.Engine.LoginNativeFlowerName + @": {
                            'username': '" + username + @"',
                            'password': '" + password + @"'
                        },
                        _errors: []
                    }";
                    Environment loginEnv = new Environment();
                    loginEnv.Nector = (JObject)JValue.Parse(nct);
                    loginEnv.Auth = auth;
                    loginEnv.IsFlowerCall = true;
                    loginEnv.IsSelf = true;
                    Bee.Workers.VisitFlower(Bee.Engine.LoginNativeFlowerName, UserNec, ref loginEnv);
                    loginEnv.IsFlowerCall = false;
                    loginEnv.IsSelf = false;

                    //swap
                    regEnv.Honey[Bee.Engine.RegisterNativeFlowerName] = loginEnv.Honey[Bee.Engine.LoginNativeFlowerName];
                    honey = regEnv.Honey;

                    return honey;
                }
                else if (act != null && String.IsNullOrEmpty(auth) && isRegistrationOpen == true)
                {
                    //execute only this part
                    String code = (String)act["code"];
                    String email = (String)act["email"];
                    String phonenumber = (String)act["phonenumber"];
                    
                    
                    //check the type of verification
                    string verifMethod = (String)Bee.Engine.Hive.SelectToken("security.verify.method");
                    bool doSms = false;
                    bool doemail = false;
                    JObject UserNec = new JObject();
                    UserNec.Add("code", code);
                    if (verifMethod.Equals("sms&email") || verifMethod.Equals("email&sms"))
                    {
                        doSms = true;
                        doemail = true;
                        UserNec.Add("email", email);
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (verifMethod.Equals("sms"))
                    {
                        doSms = true;
                        doemail = false;
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (verifMethod.Equals("email"))
                    {
                        doSms = false;
                        doemail = true;
                        UserNec.Add("email", email);
                    }
                    UserNec.Add("_errors", new JArray());
                    Environment actEnv = new Environment();
                    actEnv.Nector = UserNec;
                    actEnv.Auth = auth;
                    actEnv.IsFlowerCall = true;
                    actEnv.IsSelf = true;
                    Bee.Workers.VisitFlower(Bee.Engine.AccivateAccountNativeFlowerName, UserNec, ref actEnv);
                    actEnv.IsSelf = false;
                    actEnv.IsFlowerCall = false;
                    return actEnv.Honey;
                }
                else if (fgt != null && String.IsNullOrEmpty(auth) && isRegistrationOpen == true)
                {
                    //execute only this part
                    String email = (String)fgt["email"];
                    String phonenumber = (String)fgt["phonenumber"];
                    //check the type of recovery
                    string recoveryMethod = (String)Bee.Engine.Hive.SelectToken("security.recover.method");
                    JObject UserNec = new JObject();
                    bool useSms = false;
                    bool useEmail = false;
                    if (recoveryMethod.Equals("sms&email"))
                    {
                        useSms = true;
                        useEmail = true;
                        UserNec.Add("email", email);
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (recoveryMethod.Equals("sms"))
                    {
                        useSms = true;
                        useEmail = false;
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (recoveryMethod.Equals("email"))
                    {
                        useSms = false;
                        useEmail = true;
                        UserNec.Add("email", email);
                    }
                    UserNec.Add("_errors", new JArray());
                    Environment recEnv = new Environment();
                    recEnv.Nector = UserNec;
                    recEnv.Auth = auth;
                    recEnv.IsFlowerCall = true;
                    recEnv.IsSelf = true;
                    Bee.Workers.VisitFlower(Bee.Engine.ForgotPasswordNativeFlowerName, UserNec, ref recEnv);
                    recEnv.IsSelf = false;
                    recEnv.IsFlowerCall = false;
                    return recEnv.Honey;
                }
                else if (npt != null && String.IsNullOrEmpty(auth) && isRegistrationOpen == true)
                {
                    //execute only this part
                    String code = (String)npt["code"];
                    String email = (String)npt["email"];
                    String phonenumber = (String)npt["phonenumber"];
                    String newpassword = (String)npt["newpassword"];
                    //check the type of recovery
                    string recoveryMethod = (String)Bee.Engine.Hive.SelectToken("security.recover.method");
                    JObject UserNec = new JObject();
                    UserNec.Add("code", code);
                    UserNec.Add("newpassword", newpassword);
                    bool useSms = false;
                    bool useEmail = false;
                    if (recoveryMethod.Equals("sms&email"))
                    {
                        useSms = true;
                        useEmail = true;
                        UserNec.Add("email", email);
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (recoveryMethod.Equals("sms"))
                    {
                        useSms = true;
                        useEmail = false;
                        UserNec.Add("phonenumber", phonenumber);
                    }
                    else if (recoveryMethod.Equals("email"))
                    {
                        useSms = false;
                        useEmail = true;
                        UserNec.Add("email", email);
                    }
                    UserNec.Add("_errors", new JArray());
                    Environment recEnv = new Environment();
                    recEnv.Nector = UserNec;
                    recEnv.Auth = auth;
                    recEnv.IsFlowerCall = true;
                    recEnv.IsSelf = true;
                    Bee.Workers.VisitFlower(Bee.Engine.NewPasswordNativeFlowerName, UserNec, ref recEnv);
                    recEnv.IsSelf = false;
                    recEnv.IsFlowerCall = false;
                    return recEnv.Honey;
                }
                else
                {

                    var securityNodeDefinition = Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName];
                    if (securityNodeDefinition != null)
                    {
                        string nct = @"{ " + Bee.Scents.Flower + Bee.Engine.GetUserOfTokenNativeFlowerName + @": {
                                'token': '" + auth + @"'
                            },
                            _errors: []
                        }";
                        Environment authEnv = new Environment();
                        authEnv.Nector = (JObject)JValue.Parse(nct);
                        if (!String.IsNullOrEmpty(auth))
                        {
                            authEnv.Auth = auth;
                            authEnv.IsFlowerCall = true;
                            authEnv.IsSelf = true;
                            JObject UserNec = new JObject();
                            UserNec.Add("token", authEnv.Auth);
                            Bee.Workers.VisitFlower(Bee.Engine.GetUserOfTokenNativeFlowerName, UserNec, ref authEnv);
                            authEnv.IsSelf = false;
                        }
                        var u = authEnv.Honey["GetTokenUser"];
                        if (u != null && u.Type != JTokenType.Null)
                        {
                            environment.CurrentUser = (JObject)u;
                        }
                        else
                        {
                            //check that what this guy is needing to access is in the 
                            //exclusion zone
                            JToken exclusionObject = Bee.Engine.Hive.SelectToken("security.exclude");
                            if (exclusionObject == null || exclusionObject.Type == JTokenType.Null)
                            {
                                errors.Add("DRONE SECURITY: U dont have an account at this hive");
                            }
                            else
                            {
                                //what is excluded about this table
                                //all the root node parts of the nector must be excluded
                                foreach (var rn in nector)
                                {
                                    if(rn.Key.Equals(Bee.Scents.Errors) ||
                                       rn.Key.StartsWith(Bee.Scents.JellyImmediate) ||
                                       rn.Key.StartsWith(Bee.Scents.JellyImmediateList) ||
                                       rn.Key.StartsWith(Bee.Scents.JellyAfterMath) ||
                                       rn.Key.StartsWith(Bee.Scents.JellyAfterMathList)) 
                                    {
                                        continue;
                                    }
                                    string rcn = "";
                                    //what if itsa zip
                                    if (rn.Key.StartsWith(Bee.Scents.Zee))
                                    {
                                        string temp = rn.Key.Substring(Bee.Scents.Zee.Length);
                                        rcn = Bee.Hive.GetRealCombName(rn.Key);
                                    }
                                    else
                                    {
                                        rcn = (rn.Key.StartsWith(Bee.Scents.Flower)) ? rn.Key : Bee.Hive.GetRealCombName(rn.Key);
                                    }
                                    JToken jt = exclusionObject[rcn];
                                    if (jt == null || jt.Type == JTokenType.Null)
                                    {
                                        errors.Add("DRONE SECURITY: U dont have an account at this hive");
                                        break;
                                    }
                                }
                                if (errors.Count() == 0)
                                {
                                    //we are creating for this guy a sample user

                                }
                                else
                                {
                                }
                            }
                            
                        }
                    }//end if user has a security node

                    //start add user chunk
                    if (addChunk != null && addChunk.Type != JTokenType.Null)
                    {
                        //check for the current user
                        if (environment.CurrentUser == null || environment.CurrentUser.Type == JTokenType.Null)
                        {
                            //adding a chunk must be of a system user
                            errors.Add("DRONE SECURITY: U dont have permissions to do this");
                        }
                        else
                        {
                            Environment chunkEnv = new Environment();
                            chunkEnv.Nector = (JObject)addChunk;
                            chunkEnv.Auth = auth;
                            chunkEnv.IsFlowerCall = true;
                            chunkEnv.IsSelf = true;
                            chunkEnv.CurrentUser = environment.CurrentUser;

                            Bee.Workers.VisitFlower(Bee.Engine.AddChunksNativeFlowerName, (JObject)addChunk, ref chunkEnv);

                            chunkEnv.IsSelf = false;
                            chunkEnv.IsFlowerCall = false;

                            //swap
                            return chunkEnv.Honey;
                        }
                    }
                    //end add user chunk

                    //start clear user chunk
                    if (clearChunk != null && clearChunk.Type != JTokenType.Null)
                    {
                        //check for the current user
                        if (environment.CurrentUser == null || environment.CurrentUser.Type == JTokenType.Null)
                        {
                            //clearing a chunk must be of a system user
                            errors.Add("DRONE SECURITY: U dont have permissions to do this");
                        }
                        else
                        {
                            Environment chunkEnv = new Environment();
                            chunkEnv.Nector = (JObject)clearChunk;
                            chunkEnv.Auth = auth;
                            chunkEnv.IsFlowerCall = true;
                            chunkEnv.IsSelf = true;
                            chunkEnv.CurrentUser = environment.CurrentUser;

                            Bee.Workers.VisitFlower(Bee.Engine.ClearChunksNativeFlowerName, (JObject)clearChunk, ref chunkEnv);

                            chunkEnv.IsSelf = false;
                            chunkEnv.IsFlowerCall = false;
                             
                            //swap
                            return chunkEnv.Honey;
                        }
                    }
                    //end clear user chunk

                    //make file from chunk
                    if (makefile != null && makefile.Type != JTokenType.Null)
                    {
                        //check for the current user
                        if (environment.CurrentUser == null || environment.CurrentUser.Type == JTokenType.Null)
                        {
                            //clearing a chunk must be of a system user
                            errors.Add("DRONE SECURITY: U dont have permissions to do this");
                        }
                        else
                        {
                            Environment makeFileEnv = new Environment();
                            makeFileEnv.Nector = (JObject)makefile;
                            makeFileEnv.Auth = auth;
                            makeFileEnv.IsFlowerCall = true;
                            makeFileEnv.IsSelf = true;
                            makeFileEnv.CurrentUser = environment.CurrentUser;

                            Bee.Workers.VisitFlower(Bee.Engine.MakeFileNativeFlowerName, (JObject)makefile, ref makeFileEnv);

                            makeFileEnv.IsSelf = false;
                            makeFileEnv.IsFlowerCall = false;

                            string startsWith = "data:image/jpeg;base64,";
                            string postNector = (String)makeFileEnv.Honey.SelectToken("MakeFile.String");
                            JObject uploadNector = (JObject)JValue.Parse(postNector);
                            String fileKey = (String)makefile.SelectToken("fileKey");
                            JToken methodToken = makefile.SelectToken("method");
                            string methodToUse = "post";
                            if (methodToken == null || methodToken.Type == JTokenType.Null)
                            {
                                methodToUse = "post";
                            }
                            else
                            {
                                methodToUse = (String)methodToken;
                            }
                            String []possibleKeys = fileKey.Split(' ');
                            String imageData = "";
                            environment.Auth = auth;
                            environment.Nector = uploadNector;
                            foreach (var node in uploadNector)
                            {
                                if (node.Key.StartsWith(Bee.Scents.Scent))
                                {
                                    continue;
                                }

                                //since when posting data we do not nest
                                //then this must be on the root
                                //check if the upload key exists  here
                                if (node.Value.Type == JTokenType.Array)
                                {
                                    //inside the array is the configuration
                                    JObject config = (JObject)(((JArray)node.Value)[0]);
                                    for (int i = 0; i < possibleKeys.Count(); i++)
                                    {
                                        string possibleKey = Bee.Scents.Attribute + possibleKeys[i];
                                        JToken tempx = config[possibleKey];
                                        if (tempx != null && tempx.Type != JTokenType.Null)
                                        {
                                            //we have a key that has an upload
                                            imageData = (String)tempx;
                                            if (imageData.StartsWith(startsWith))
                                            {
                                                imageData = imageData.Substring(startsWith.Length);
                                            }
                                            //turn the string into a file
                                            Upload(ref  environment, fileKey, imageData);
                                            //update this entry with a jelly call
                                            config[possibleKey] = Bee.Scents.JellyImmediate + possibleKey.Substring(2);
                                        }
                                    }
                                }
                                else
                                {
                                    JObject config = (JObject)(node.Value);
                                    for (int i = 0; i < possibleKeys.Count(); i++)
                                    {
                                        string possibleKey = Bee.Scents.Attribute + possibleKeys[i];
                                        JToken tempx = config[possibleKey];
                                        if (tempx != null && tempx.Type != JTokenType.Null)
                                        {
                                            //we have a key that has an upload
                                            imageData = (String)tempx;
                                            if (String.IsNullOrEmpty(imageData))
                                            {
                                                //check if we have a default value
                                                string rcn  = Bee.Hive.GetRealCombName(node.Key);
                                                String kyx = possibleKey.Substring(2);
                                                String pthy = "store."+kyx+".defaults."+ rcn;
                                                JToken defVal = Bee.Engine.Hive.SelectToken(pthy);
                                                if (defVal == null || defVal.Type == JTokenType.Null)
                                                {
                                                    config[possibleKey] = null;
                                                    config.Remove(possibleKey);
                                                }
                                                else
                                                {
                                                    //register the value into the immediates
                                                    string defValue = (String)defVal;
                                                    string kiiy = Bee.Scents.JellyImmediate + kyx;
                                                    environment.ImmediateJellyValues.Add(kiiy,defValue);
                                                    config[possibleKey] = Bee.Scents.JellyImmediate + kyx;
                                                }
                                            }
                                            else
                                            {
                                                if (imageData.StartsWith(startsWith))
                                                {
                                                    imageData = imageData.Substring(startsWith.Length);
                                                }
                                                //turn the string into a file
                                                Upload(ref  environment, fileKey, imageData);
                                                //update this entry with a jelly call
                                                config[possibleKey] = Bee.Scents.JellyImmediate + possibleKey.Substring(2);
                                            }
                                        }
                                    }
                                }
                            }
                            environment.Nector = uploadNector;
                            //post the rest of the data
                            if (methodToUse.Equals("post"))
                            {
                                honey = Bee.Workers.ProcessPost(uploadNector, environment);
                            }
                            else if (methodToUse.Equals("update"))
                            {
                                honey = Bee.Workers.ProcessUpdate(uploadNector, ref environment);
                            }
                            //clear chunks
                            string userComb = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserCombNodeName];
                            string primaryKeyCell = Bee.Hive.GetPK(userComb);
                            Environment chunkEnvDelete = new Environment();
                            string deleteNect = @"{
                                                        'BeeChunk': {
                                                            '" + Bee.Scents.Where + @"': {
                                                                '_$UserId_e' : '_@u_" + primaryKeyCell + @"'
                                                            }
                                                        }
                                                    }";
                            JObject deleteNectObject = (JObject)JValue.Parse(deleteNect);
                            chunkEnvDelete.Nector = deleteNectObject;
                            chunkEnvDelete.Auth = auth;
                            chunkEnvDelete.IsFlowerCall = true;
                            chunkEnvDelete.IsSelf = true;
                            chunkEnvDelete.CurrentUser = environment.CurrentUser;
                            honey = Bee.Workers.ProcessDelete(deleteNectObject, ref chunkEnvDelete);
                            chunkEnvDelete.IsSelf = false;
                            chunkEnvDelete.IsFlowerCall = false;
                            //swap
                            return environment.CurrentHoneyRef;
                        }
                    }
                    //end make file from chunk
                    
                }
            }catch(Exception ex){
                errors.Add("We faced challenges when trying to authorise you, it seems you dont have an account at this hive");
                errors.Add(ex.StackTrace);
            }
            
            if (errors.Count() == 0)
            {
                try
                {

                    environment.Auth = auth;
                    environment.Nector = nector;
                    Upload(ref environment);
                    if (way.Equals(Bee.Engine.GetActionName))
                    {
                        Environment.queryNo = 0;
                        honey = ProcessGet(ref environment);
                    }
                    else if (way.Equals(Bee.Engine.PostActionName))
                    {
                        //honey = ProcessPost(ref environment);
                        honey = Bee.Workers.ProcessPost(nector, environment);
                    }
                    else if (way.Equals(Bee.Engine.UpdateActionName))
                    {
                        //honey = ProcessUpdate(ref environment);
                        honey = Bee.Workers.ProcessUpdate(nector, ref environment);
                    }
                    else if (way.Equals(Bee.Engine.DeleteActionName))
                    {
                        //honey = ProcessDelete(ref environment);
                        honey = Bee.Workers.ProcessDelete(nector, ref environment);
                    }


                }
                catch (Exception ex)
                {
                    Unload(ref environment);
                    errors.Add("!!!zzz sorry, worker bees got some issues : " + ex.Message);
                    if (Bee.Hive.Mood == "dev")
                    {
                        errors.Add(ex.StackTrace);
                    }
                    if (has_errosNode == true)
                    {
                        foreach (string error in errors)
                        {
                            ((JArray)honey["_errors"]).Add(error);
                        }
                    }
                }
            }
            else if (isNectorFine == true)
            {
                //check if we have an _errors staff in the honey
                if (has_errosNode == true)
                {
                    foreach (string error in errors)
                    {
                        ((JArray)honey["_errors"]).Add(error);
                    }
                }
                else
                {
                    var xn = honey.SelectToken(Bee.Scents.Errors);
                    if (xn == null || xn.Type == JTokenType.Null)
                    {
                        honey.Add("_errors", new JArray());
                    }
                    foreach (string error in errors)
                    {
                        ((JArray)honey["_errors"]).Add(error);
                    }
                }
            }
            else
            {
                if (has_errosNode == true)
                {
                    foreach (string error in errors)
                    {
                        ((JArray)honey["_errors"]).Add(error);
                    }
                }
                else
                {
                    var xn = honey.SelectToken(Bee.Scents.Errors);
                    if (xn == null || xn.Type == JTokenType.Null)
                    {
                        honey.Add("_errors", new JArray());
                    }
                    foreach (string error in errors)
                    {
                        ((JArray)honey["_errors"]).Add(error);
                    }
                }
            }

            return honey;
        }

        public string Get()
        {
            return "zzzzzz !!! zzzzzz zz...";
        }

        private JObject ProcessGet(ref Environment env)
        {
            //boot a hive
            Bee.Hive.GetConnection();
            JObject honey = new JObject();
            honey = Bee.Workers.ProcessGet(env.Nector, false, env);          
            return honey;
        }

        private JObject ProcessGetOriginal(JObject nectorJsonObject)
        {
            JObject honey = new JObject();
            String sql = "";

            var conn = Hive.GetConnection();

            foreach (var nectorRootItem in nectorJsonObject)
            {
                string key = nectorRootItem.Key;
                string realTableName = Hive.GetRealCombName(key);
                sql = "SELECT ";
                string innerJoinSql = "";
                string whereSql = "";
                //is this a list or a single object
                if (nectorRootItem.Value.Type == JTokenType.Array) //this is an array
                {
                    //go through the json objects in the array
                    //there should only be one configuration object in the array
                    //the array syntax is just to indicate that we mean to return a list 
                    //of these items
                    foreach (JObject tableConfig in nectorRootItem.Value)
                    {
                        //go through all the attributes of this configuration
                        foreach (var tableConfigItem in tableConfig)
                        {
                            string tablekey = tableConfigItem.Key;
                            dynamic tablekeyValue = tableConfigItem.Value;
                            if (tablekey.Equals("a"))
                            {
                                string attributsStr = tablekeyValue;
                                string[] cols = attributsStr.Split(' ');
                                foreach (string col in cols)
                                {
                                    string asStr =  "a" + key + "_" + col;
                                    sql = sql + " " + realTableName + "." + col + " AS " + asStr + ",";

                                }
                                //tablekeyValue
                            }
                            else if(tablekey.Equals("_w"))
                            {
                                JObject whObj = (JObject)tableConfigItem.Value;
                                string ts = ProcessWhere(whObj, realTableName);
                                whereSql = whereSql + ts;
                            }
                            else
                            {
                                string realNavObjTableName = Hive.GetRealCombName(tablekey);
                                JObject navObj = new JObject();
                                string pathToUse = "a" + key + "_";
                                if (tableConfigItem.Value.Type == JTokenType.Array) //a collection of these
                                {
                                    //get the attributes of a single object of this collection
                                    foreach (JObject childTableConfig in tableConfigItem.Value)
                                    {
                                        navObj = childTableConfig;
                                    }
                                    pathToUse =  pathToUse + "a" + tablekey;
                                    NodeObject no = ProcessJObject(navObj, realNavObjTableName, realTableName, pathToUse);
                                    sql = sql + no.PartialSql;
                                    //honey.Add(tablekey, new JArray() as dynamic);
                                }
                                else //a parent table
                                {
                                    //inner join
                                    navObj = tableConfigItem.Value as JObject;
                                    pathToUse = pathToUse + "o" + tablekey;
                                    NodeObject no = ProcessJObject(navObj, realNavObjTableName, realTableName, pathToUse);
                                    sql = sql + no.PartialSql;
                                    innerJoinSql = innerJoinSql + no.InnerJoin;
                                    //honey.Add(tablekey, no.Node);

                                        
                                }
                            }
                        }
                    }
                    sql = sql.Trim(',');
                    sql = sql + " FROM " + realTableName;
                    sql = sql + innerJoinSql;
                    whereSql = whereSql.Trim();
                    if (!String.IsNullOrEmpty(whereSql))
                    {
                        sql = sql + " WHERE " + whereSql;
                    }
                }
                else //this is an object
                {
                }


                //execute query
                if(conn.State == System.Data.ConnectionState.Closed){
                    conn.Open();
                }
                using (SqlCommand sqlCmd = new SqlCommand(sql, conn))
                {
                    SqlDataReader reader = sqlCmd.ExecuteReader();

                    if (reader == null || reader.HasRows == false)
                    {
                        //bRet = false;
                    }
                    else
                    {
                            
                        while (reader.Read())
                        {
                            bool newRow = true;
                            int cols = reader.FieldCount;
                            for (int i = 0; i < cols; i++)
                            {
                                string fieldname = reader.GetName(i);
                                Object value = reader.GetValue(i);
                                string[] fieldParts = fieldname.Split('_');
                                JToken current = honey;
                                for (int j = 0; j < fieldParts.Length ; j++)
                                {
                                    string fieldPart = fieldParts[j];
                                    if (j == fieldParts.Length - 1) //at the last point, its the column name
                                    {
                                        if (current.Type == JTokenType.Array)
                                        {
                                            JArray ja = (JArray)current;
                                            int lenght = ja.Count();
                                            if (lenght == 0 || newRow == true)
                                            {
                                                newRow = false;
                                                //add a new object
                                                JObject njob = new JObject();
                                                JValue jv = new JValue(value);
                                                njob.Add(fieldPart, jv);
                                                ja.Add(njob);
                                            }
                                            else
                                            {
                                                JValue jv = new JValue(value);
                                                //we are editing the last entry
                                                ((JObject)(ja[lenght - 1])).Add(fieldPart, jv);
                                            }
                                        }
                                        else //its an object so we put the the attribute value
                                        {
                                            JObject temp = ((JObject)current);
                                            Type t = value.GetType();
                                            JValue jv = new JValue(value);
                                            temp.Add(fieldPart, jv);
                                        }
                                    }
                                    else
                                    {
                                        //check if its array or object
                                        bool isArray = (fieldPart.StartsWith("a")) ? true : false;
                                        string nodeName = fieldPart.Substring(1);

                                        bool nodeFound = false;
                                        try
                                        {
                                            var xtemp = current[nodeName];
                                            if (xtemp == null)
                                            {
                                                nodeFound = false;
                                            }
                                            else
                                            {
                                                nodeFound = true;
                                            }
                                        }catch(Exception ex){
                                            nodeFound = false;
                                        }

                                        if (isArray && nodeFound == false)
                                        {
                                            ((JObject)current).Add(nodeName, new JArray());
                                            current = current[nodeName];
                                        }
                                        else if (isArray && nodeFound == true)
                                        {
                                            current = current[nodeName];
                                        }
                                        else if (isArray == false && nodeFound == false)
                                        {
                                            if (current.Type != JTokenType.Array)
                                            {
                                                ((JObject)current).Add(nodeName, new JObject());
                                            }
                                            else if (current.Type == JTokenType.Array)
                                            {
                                                //going for the last item inside this array
                                                JArray ja = (JArray)current;
                                                int lenght = ja.Count();
                                                if (lenght == 0)
                                                {
                                                    //add a new object
                                                    JObject njob = new JObject();
                                                    njob.Add(nodeName, new JObject());
                                                    ja.Add(njob);
                                                    lenght = ja.Count();
                                                }
                                                    
                                                //we are editing the last entry
                                                    
                                                bool subNodeFound = false;
                                                try
                                                {
                                                    var xtemp = ((JObject)(ja[lenght - 1]))[nodeName];
                                                    if (xtemp == null)
                                                    {
                                                        subNodeFound = false;
                                                    }
                                                    else
                                                    {
                                                        subNodeFound = true;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    subNodeFound = false;
                                                }
                                                if(subNodeFound == false){
                                                    ((JObject)(ja[lenght - 1])).Add(nodeName, new JObject());
                                                }
                                                current = (JObject)(ja[lenght - 1])[nodeName];
                                            }
                                        }
                                            
                                    }
                                }
                            }
                        }
                    }

                    //close the reader
                    if (reader.IsClosed == false)
                    {
                        reader.Close();
                    }

                }
                    

            }

            return honey;
        }

        private JObject ProcessPost(ref Environment env)
        {
            var conn = Hive.GetConnection();
            Dictionary<string,int> hooks = new Dictionary<string,int>();

            JObject honey = new JObject();
            JObject nector = env.Nector;
            String sql = "";
            //Going through the root nodes
            foreach (var rootNode in nector)
            {
                string rootNodeKey = rootNode.Key;
                string realTableName = Hive.GetRealCombName(rootNodeKey);
                bool can = Bee.Hive.CanCreate(realTableName, ref env);
                if (can == false)
                {
                    throw new Exception("Drone Security: You have no rights to create " + rootNodeKey);
                }
                if (rootNode.Value.Type == JTokenType.Array) //this is an array
                {
                    //go through all the objects in the array
                    foreach (JObject nodeObject in rootNode.Value)
                    {
                        Bee.Workers.PostObject(nodeObject, realTableName, ref hooks, ref env);
                    }
                }
                else
                {
                    JObject nodeObject = (JObject)rootNode.Value;
                    Bee.Workers.PostObject(nodeObject, realTableName, ref hooks, ref env);
                }
            }

            return honey;
        }

        private void PostObject(JObject nodeObject, string realTableName, ref Dictionary<string,int> hooks)
        {
            var conn = Hive.GetConnection();
            String nodeObjSql = "INSERT INTO " + realTableName + " ( ";
            String valuesSqlPartial = "";
            string _r = "";
            int lastInsertId = 0;
            Dictionary<string, JArray> excecuteLaiter = new Dictionary<string, JArray>();
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in nodeObject)
            {
                string columnName = nodeObjectAttribute.Key;
                if (columnName.Equals("_o"))
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
                        nodeObjSql = nodeObjSql + " " + fkColumnName + " ,";
                        //get the type of column
                        valuesSqlPartial = valuesSqlPartial + " " + fkValue + " ,";
                    }
                }else if (columnName.Equals("_r"))
                {
                    //register repalcement value
                    //when this object has been 
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    hooks.Add(value,0);
                    _r = value;
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //these will be executed laiter because theie foreign key 
                    //may not yet be available at this time
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    excecuteLaiter.Add(columnName, arr);
                }else
                {
                    //we assume its a normal attribute
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    nodeObjSql = nodeObjSql + " " + columnName + " ,";
                    string qv = Hive.GetQuoted(value, columnName, realTableName);
                    valuesSqlPartial = valuesSqlPartial + " " + qv + " ,";
                }
            }//end for loop
            nodeObjSql = nodeObjSql.Trim(',');
            valuesSqlPartial = valuesSqlPartial.Trim(',');
            nodeObjSql = nodeObjSql + " ) VALUES ( " + valuesSqlPartial + " ) ";

            if(!String.IsNullOrEmpty(_r)){
                nodeObjSql = nodeObjSql + "; SELECT SCOPE_IDENTITY();";
            }

            using(SqlCommand sqlCmd = new SqlCommand(nodeObjSql, conn)){                              
                if (!String.IsNullOrEmpty(_r))
                {             
                    string res = sqlCmd.ExecuteScalar().ToString();
                    lastInsertId = Convert.ToInt32(res);
                }
                else
                {
                    sqlCmd.ExecuteNonQuery(); 
                }
            }

            if (!String.IsNullOrEmpty(_r))
            {
                hooks[_r] = lastInsertId;
            }

            //execute laiter
            foreach (KeyValuePair<string,JArray> arrayItem in excecuteLaiter)
            {
                string colName = arrayItem.Key;
                JArray arr = arrayItem.Value;
                string localRealTableName = Hive.GetRealCombName(colName);
                foreach (JObject localNodeObject in arr)
                {
                    PostObject(localNodeObject, localRealTableName, ref hooks);
                }
            }
            
        }

        private JObject ProcessUpdate(ref Environment env)
        {
            var conn = Hive.GetConnection();

            JObject honey = new JObject();
            JObject nector = env.Nector;
            String sql = "";
            //Going through the root nodes
            foreach (var rootNode in nector)
            {
                string rootNodeKey = rootNode.Key;
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
                        UpdateObject(nodeObject, realTableName);
                    }
                }
                else
                {
                    JObject nodeObject = (JObject)rootNode.Value;
                    UpdateObject(nodeObject, realTableName);
                }
            }

            return honey;
        }

        private void UpdateObject(JObject nodeObject, string realTableName)
        {
            var conn = Hive.GetConnection();
            String nodeObjSql = "UPDATE " + realTableName + " SET ";
            String whereSql = "";
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in nodeObject)
            {
                string columnName = nodeObjectAttribute.Key;
                if (columnName.Equals(Bee.Scents.Where))
                {
                    //get where clause
                    JObject whereObject = (JObject)nodeObjectAttribute.Value;
                    string whereRes = ProcessWhere(whereObject);
                    whereSql = whereSql + whereRes;
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //for update operations the nested objets are executed imediately
                    string localRealTableName = Hive.GetRealCombName(columnName);
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    foreach (JObject localNodeObject in arr)
                    {
                        UpdateObject(localNodeObject, localRealTableName);
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

        }

        private string ProcessWhere(JObject where)
        {
            string str = "";
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in where)
            {
                string columnName = nodeObjectAttribute.Key;
                if (nodeObjectAttribute.Value.Type == JTokenType.Object)
                {
                    JObject whereObject = (JObject)nodeObjectAttribute.Value;
                    string nestedWhere = ProcessWhere(whereObject);
                    str = str + nestedWhere;
                }
                else
                {
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    string qv = Hive.GetQuoted(value, nodeObjectAttribute.Value.Type);
                    str = str + columnName + " " + qv;
                }
            }
            //the thining
            str = str.Replace("_and_", " AND ");
            str = str.Replace("_or_", " OR ");
            str = str.Replace("_ltoe", " <= ");
            str = str.Replace("_gtoe", " >= ");
            str = str.Replace("_lt", " < ");
            str = str.Replace("_gt", " > ");
            str = str.Replace("_e", " = ");
            
            return str;
        }

        private string ProcessWhere(JObject where, string realTableName)
        {
            string str = "";
            //go through all the attributes of this nodeObject
            foreach (var nodeObjectAttribute in where)
            {
                string columnName = nodeObjectAttribute.Key;
                if (nodeObjectAttribute.Value.Type == JTokenType.Object)
                {
                    JObject whereObject = (JObject)nodeObjectAttribute.Value;
                    string nestedWhere = ProcessWhere(whereObject);
                    str = str + nestedWhere;


                }
                else
                {
                    dynamic columValue = nodeObjectAttribute.Value;
                    string value = columValue;
                    string qv = Hive.GetQuoted(value, nodeObjectAttribute.Value.Type);
                    str = str + " " + realTableName + "." + columnName + " " + qv;
                }
            }
            //the thining
            str = str.Replace("_and_", " AND ");
            str = str.Replace("_or_", " OR ");
            str = str.Replace("_ltoe", " <= ");
            str = str.Replace("_gtoe", " >= ");
            str = str.Replace("_lt", " < ");
            str = str.Replace("_gt", " > ");
            str = str.Replace("_e", " = ");

            return str;
        }

        private JObject ProcessDelete(ref Environment env)
        {
            var conn = Hive.GetConnection();

            JObject honey = new JObject();
            JObject nector = env.Nector;
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
                        DeleteObject(nodeObject, realTableName);
                    }
                }
                else
                {
                    JObject nodeObject = (JObject)rootNode.Value;
                    DeleteObject(nodeObject, realTableName);
                }
            }

            return honey;
        }

        private void DeleteObject(JObject nodeObject, string realTableName)
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
                    string whereRes = ProcessWhere(whereObject);
                    whereSql = whereSql + whereRes;
                }
                else if (nodeObjectAttribute.Value.Type == JTokenType.Array)
                {
                    //for update operations the nested objets are executed imediately
                    string localRealTableName = Hive.GetRealCombName(columnName);
                    JArray arr = (JArray)nodeObjectAttribute.Value;
                    foreach (JObject localNodeObject in arr)
                    {
                        DeleteObject(localNodeObject, localRealTableName);
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

        
        

        private NodeObject ProcessJObject(JObject jobj, String realTableName, string realChildTableName, string path)
        {
            NodeObject no = new NodeObject();
            no.PartialSql = "";
            no.Node = new JObject();
            no.InnerJoin = "";
            //INNER JOIN ParentTable ON ThisChildTable.ParentTableId = ParentTable.ParentTableId
            no.InnerJoin = no.InnerJoin + " INNER JOIN " + realTableName + " ON ";
            string pkName = Hive.GetPK(realTableName);
            no.InnerJoin = no.InnerJoin + "  " + realChildTableName + "." + pkName + " = ";
            no.InnerJoin = no.InnerJoin + "  " + realTableName + "." + pkName + " ";


            //go through all the attributes of this configuration
            foreach (var tableConfigItem in jobj)
            {
                string tablekey = tableConfigItem.Key;               
                dynamic tablekeyValue = tableConfigItem.Value;
                if (tablekey.Equals("a"))
                {
                    string attributsStr = tablekeyValue;
                    string[] cols = attributsStr.Split(' ');
                    foreach (string col in cols)
                    {
                        string asStr = path + "_" + col;
                        no.PartialSql = no.PartialSql + " " + realTableName + "." + col + " AS " + asStr + ",";
                    }
                    
                }
                else
                {
                    //check if type if array           
                    string realNavObjTableName = Hive.GetRealCombName(tablekey);
                    JObject navObj = new JObject();
                    string pathToUse = path + "_";
                    if (tableConfigItem.Value.Type == JTokenType.Array) //a collection of these
                    {
                        //get the attributes of a single object of this collection
                        foreach (JObject childTableConfig in tableConfigItem.Value)
                        {
                            navObj = childTableConfig;
                        }
                        pathToUse = pathToUse + "a" + tablekey;
                        NodeObject newNodeObject = ProcessJObject(navObj, realNavObjTableName, realTableName, pathToUse);
                        no.PartialSql = no.PartialSql + newNodeObject.PartialSql;
                        no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                        no.Node.Add(tablekey, new JArray() as dynamic);
                    }
                    else //a parent table
                    {
                        navObj = tableConfigItem.Value as JObject;
                        pathToUse = pathToUse + "o" + tablekey;
                        NodeObject newNodeObject = ProcessJObject(navObj, realNavObjTableName, realTableName, pathToUse);
                        no.PartialSql = no.PartialSql + newNodeObject.PartialSql;
                        no.InnerJoin = no.InnerJoin + " \n " + newNodeObject.InnerJoin;
                        no.Node.Add(tablekey, newNodeObject.Node);
                    }
                   
                }
            }



            return no;
        }

        private void Unload(ref Environment env)
        {
            foreach (Upload item in env.Uploads)
            {
                if (File.Exists(item.FullSavedFileName))
                {
                    File.Delete(item.FullSavedFileName);
                }
            }
        }

        private void Upload(ref Environment env, String key, String base64String)
        {
            String dbName = Bee.Hive.GetDbName();
            string filesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + dbName + "/");
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }
            string userComb = (String)Bee.Engine.Hive[Bee.Engine.RawHiveDefiniationSecurityNodeName][Bee.Engine.RawHiveDefiniationSecurityUserCombNodeName];
            string primaryKeyCell = Bee.Hive.GetPK(userComb);
            string uniqueFileName = "bee_" + DateTime.Now.Ticks + "_" + (String)env.CurrentUser[primaryKeyCell] + "_image.jpeg";
            string bodyPath = "";
            JToken storeNode = Bee.Engine.Hive["store"];
            if (storeNode != null)
            {
                JToken uploadParam = storeNode[key];
                if (uploadParam != null)
                {
                    JToken pathToken = uploadParam["path"];
                    if (pathToken != null)
                    {
                        String pt = (String)pathToken;
                        filesPath += pt + "/";
                        bodyPath += pt + "/";
                        //check if directory exists
                        if (!Directory.Exists(filesPath))
                        {
                            Directory.CreateDirectory(filesPath);
                        }
                    }
                    JToken resizeToken = uploadParam["resize"];
                    if (resizeToken != null)
                    {
                        //save to an original size directory
                        //check if directory exists
                        string originalPath = filesPath + "original/";
                        if (!Directory.Exists(originalPath))
                        {
                            Directory.CreateDirectory(originalPath);
                        }
                        
                        String fileNameToSave = originalPath + Path.GetFileName(uniqueFileName);
                        File.WriteAllBytes(fileNameToSave, Convert.FromBase64String(base64String));
                        string url = "http://" + Request.RequestUri.Authority + "/" + dbName + "/" +  bodyPath + "/" +  uniqueFileName;
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
                        foreach (var resize in (JObject)resizeToken)
                        {
                            string resizeName = resize.Key;
                            bodyPath += resizeName + "/";
                            int[] wh = ((String)resize.Value).Split('x').Select<String, int>(x => Convert.ToInt32(x)).ToArray<int>();
                            string sizePath = filesPath + resizeName + "/";
                            //check if directory exists
                            if (!Directory.Exists(sizePath))
                            {
                                Directory.CreateDirectory(sizePath);
                            }
                            //save a resized copy here of the original
                            // Read a file and resize it.
                            String fileNameToSaveTemp = originalPath + Path.GetFileName(uniqueFileName);
                            byte[] photoBytes = File.ReadAllBytes(fileNameToSaveTemp);
                            int quality = 70;
                            Size size = new Size(wh[0], wh[1]);
                            using (MemoryStream inStream = new MemoryStream(photoBytes))
                            {
                                using (MemoryStream outStream = new MemoryStream())
                                {
                                    using (ImageFactory imageFactory = new ImageFactory())
                                    {
                                        // Load, resize, set the format and quality and save an image.
                                        var format = new ImageProcessor.Imaging.Formats.JpegFormat();
                                        imageFactory.Load(inStream)
                                                .Resize(size)
                                                .Format(format)
                                                .Quality(quality)
                                                .Save(outStream);
                                    }

                                    // Do something with the stream.
                                    Bee.Hive.Save(bodyPath, sizePath, uniqueFileName, Request.RequestUri.Authority, key, dbName, outStream, ref env);
                                }
                            }
                        }//foreach (var resize in (JObject)resizeToken)
                    }//if (resizeToken != null)
                    else
                    {
                        //just save the file here
                        if (!Directory.Exists(filesPath))
                        {
                            Directory.CreateDirectory(filesPath);
                        }
                        String fileNameToSave = filesPath + Path.GetFileName(uniqueFileName);
                        File.WriteAllBytes(fileNameToSave, Convert.FromBase64String(base64String));
                        //an immediate jelly
                        if (!env.ImmediateJellyValues.ContainsKey(Bee.Scents.JellyImmediate+key))
                        {
                            env.ImmediateJellyValues.Add(Bee.Scents.JellyImmediate + key, uniqueFileName);
                        }
                    }
                } //if (uploadParam != null)
                else
                {
                    //just save the file here
                    if (!Directory.Exists(filesPath))
                    {
                        Directory.CreateDirectory(filesPath);
                    }
                    String fileNameToSave = filesPath + Path.GetFileName(uniqueFileName);
                    File.WriteAllBytes(fileNameToSave, Convert.FromBase64String(base64String));
                    //an immediate jelly
                    if (!env.ImmediateJellyValues.ContainsKey(Bee.Scents.JellyImmediate+key))
                    {
                        env.ImmediateJellyValues.Add(Bee.Scents.JellyImmediate + key, uniqueFileName);
                    }
                }
            }
            else
            { //if there is no store configuration
                if (!Directory.Exists(filesPath))
                {
                    Directory.CreateDirectory(filesPath);
                }
                String fileNameToSave = filesPath + Path.GetFileName(uniqueFileName);
                File.WriteAllBytes(fileNameToSave, Convert.FromBase64String(base64String));
                //an immediate jelly
                if (!env.ImmediateJellyValues.ContainsKey(Bee.Scents.JellyImmediate+key))
                {
                    env.ImmediateJellyValues.Add(Bee.Scents.JellyImmediate + key, uniqueFileName);
                }
            }      
        }

        private void Upload(ref Environment env)
        {
            String dbName = Bee.Hive.GetDbName();
            string filesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + dbName + "/");
            if(!Directory.Exists(filesPath)){
                Directory.CreateDirectory(filesPath);
            }
            System.Web.HttpFileCollection fileCollection = System.Web.HttpContext.Current.Request.Files;
            for (int i = 0; i < fileCollection.Count; i++)
            {
                System.Web.HttpPostedFile postedFile = fileCollection[i];
                if (postedFile.ContentLength > 0)
                {
                    string key = fileCollection.AllKeys[i];
                    string originalFileName = postedFile.FileName;
                    string uniqueFileName = "bee_" + DateTime.Now.Ticks + "_" + originalFileName;
                    string bodyPath = "";

                    bool isImage = originalFileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                    originalFileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
                    originalFileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase);
                    
                    JToken storeNode = Bee.Engine.Hive["store"];
                    if (storeNode != null)
                    {
                        JToken uploadParam = storeNode[key];
                        if (uploadParam != null)
                        {
                            JToken pathToken = uploadParam["path"];
                            if (pathToken != null)
                            {
                                String pt = (String)pathToken;
                                filesPath += pt + "/";
                                bodyPath += pt + "/";
                                //check if directory exists
                                if (!Directory.Exists(filesPath))
                                {
                                    Directory.CreateDirectory(filesPath);
                                }
                            }
                            if (isImage)
                            {
                                JToken resizeToken = uploadParam["resize"];
                                if (resizeToken != null)
                                {
                                    //save to an original size directory
                                    //check if directory exists
                                    string originalPath = filesPath + "original/";
                                    if (!Directory.Exists(originalPath))
                                    {
                                        Directory.CreateDirectory(originalPath);
                                    }
                                    Bee.Hive.Save(bodyPath, originalPath, uniqueFileName, Request.RequestUri.Authority, key, dbName, postedFile, ref env);
                                    foreach (var resize in (JObject)resizeToken)
                                    {
                                        string resizeName = resize.Key;
                                        bodyPath += resizeName + "/";
                                        int[] wh = ((String)resize.Value).Split('x').Select<String, int>(x => Convert.ToInt32(x)).ToArray<int>();
                                        string sizePath = filesPath + resizeName + "/";
                                        //check if directory exists
                                        if (!Directory.Exists(sizePath))
                                        {
                                            Directory.CreateDirectory(sizePath);
                                        }
                                        //save a resized copy here of the original
                                        // Read a file and resize it.
                                        String fileNameToSaveTemp = originalPath + Path.GetFileName(uniqueFileName);
                                        byte[] photoBytes = File.ReadAllBytes(fileNameToSaveTemp);
                                        int quality = 70;
                                        
                                             
                                        Size size = new Size(wh[0],wh[1]);
                                        using (MemoryStream inStream = new MemoryStream(photoBytes))
                                        {
                                            using (MemoryStream outStream = new MemoryStream())
                                            {
                                                using (ImageFactory imageFactory = new ImageFactory())
                                                {
                                                    // Load, resize, set the format and quality and save an image.
                                                    if (originalFileName.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
                                                        originalFileName.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        var format = new ImageProcessor.Imaging.Formats.JpegFormat();
                                                        imageFactory.Load(inStream)
                                                               .Resize(size)
                                                               .Format(format)
                                                               .Quality(quality)
                                                               .Save(outStream);
                                                    }
                                                    else if (originalFileName.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        var format = new ImageProcessor.Imaging.Formats.PngFormat();
                                                        imageFactory.Load(inStream)
                                                               .Resize(size)
                                                               .Format(format)
                                                               .Quality(quality)
                                                               .Save(outStream);
                                                    }
                                                }
 
                                                // Do something with the stream.
                                                Bee.Hive.Save(bodyPath, sizePath, uniqueFileName, Request.RequestUri.Authority, key, dbName, outStream, ref env);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //just save the file here
                                    Bee.Hive.Save(bodyPath,filesPath, uniqueFileName, Request.RequestUri.Authority, key, dbName, postedFile, ref env);
                                }
                            }
                            else
                            {
                                Bee.Hive.Save(bodyPath, filesPath, uniqueFileName, Request.RequestUri.Authority, key, dbName, postedFile, ref env);
                            }
                        }
                        else
                        {
                            Bee.Hive.Save(bodyPath, filesPath, uniqueFileName, Request.RequestUri.Authority, key, dbName, postedFile, ref env);
                        }
                    }
                    else
                    {
                        Bee.Hive.Save(bodyPath, filesPath, uniqueFileName, Request.RequestUri.Authority, key, dbName, postedFile, ref env);
                    }


                    
                    
                    
                }
            }
        }
    }

   
}

