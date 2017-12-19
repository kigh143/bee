using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorEngine.Templating;
using System.IO;

namespace Bee
{
    public class Queen
    {
        public static string Eat(string jellyDefinition, ref Environment env)
        {
            var output = "";
            string[] jellyParts = jellyDefinition.Trim().Split(' ').Where(
                jp => !String.IsNullOrEmpty(jp.Trim())
            ).Select(jp => jp.Trim()).ToArray<string>();
            switch (jellyParts[0])
            {
                case "join":
                    output = join(jellyParts.Skip(1).ToList<string>(), ref env);
                    break;
                case "encrypt":
                    output = crypt(jellyParts[1], jellyParts[2], ref env);
                    break;
                case "dencrypt":
                    output = decrypt(jellyParts[1], jellyParts[2], ref env);
                    break;
                case "getcode":
                    output = getcode(jellyParts[1], ref env);
                    break;
                case "temp":
                    output = temp(jellyParts[1], ref env);
                    break;
                case "sendemail":
                    output = sendemail(jellyParts[1], jellyParts[2], jellyParts[3], ref env);
                    break;
                case "sendsms":
                    output = sendsms(jellyParts[1], jellyParts[2], ref env);
                    break;
                case "getrandomecode":
                    output = getrandomecode(jellyParts[1], ref env);
                    break;
                case "comma":
                    output = comma(jellyParts[1], ref env);
                    break;
                default:
                    output = unpack(jellyParts[0], ref env);
                    break;
            }
            return output;
        }

        public static string comma(String number, ref Environment env)
        {
            number = Bee.Queen.unpack(number, ref env);
            Decimal x = Convert.ToDecimal(number);
            String res = x.ToString("#,##0");
            return res;
        }


        public static string sendsms(String phoneNumber, String templateName, ref Environment env)
        {
            phoneNumber = Bee.Queen.unpack(phoneNumber, ref env);
            String msg = Bee.Queen.temp(templateName, ref env);
            //get the sms provider
            String provider = (String)Bee.Engine.Hive.SelectToken("sms.provider");
            if (provider.Equals("magezi"))
            {
                Bee.Sms.SendViaMagezi(new List<string>() { phoneNumber }, msg);
            };
            return "ok";
        }

        public static string sendemail(String to, String subject, String templateName, ref Environment env)
        {
            to = Bee.Queen.unpack(to, ref env);
            subject = Bee.Queen.unpack(subject, ref env);
            String msg = Bee.Queen.temp(templateName, ref env);
            //get the email provider
            String provider = (String)Bee.Engine.Hive.SelectToken("email.provider"); 
            if(provider.Equals("mailjet")){
                //find out why this is not returning a response
                //Bee.Mail.SendViaMailJet(to, subject, msg).Wait();
            };
            return "ok";
        }

        public static string temp(String templateName, ref Environment env)
        {
            templateName = Bee.Queen.unpack(templateName, ref env);
            string dbName = Bee.Hive.GetDbName();
            string filesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/" + dbName + "/");
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }
            string fullName = filesPath + Path.GetFileName(templateName);
            if (File.Exists(fullName))
            {
                string template = "";
                using (StreamReader sr = new StreamReader(fullName))
                {
                    template = sr.ReadToEnd();
                }

                bool isCahed = RazorEngine.Engine.Razor.IsTemplateCached(templateName, null);
                if (!isCahed)
                {
                    var result = RazorEngine.Engine.Razor.RunCompile(template, templateName, null, new
                    {
                        Hive = Bee.Engine.Hive,
                        Env = env
                    });
                    return result;
                }
                else
                {
                    var result = RazorEngine.Engine.Razor.Run(templateName, null, new
                    {
                        Hive = Bee.Engine.Hive,
                        Env = env
                    });
                    return result;
                }
                
            }

            
            return "";
        }

        public static string getrandomecode(String length, ref Environment env)
        {
            length = Bee.Queen.unpack(length, ref env);
            bool useUpperCase = true;
            bool useLowerCase = true;
            bool useNumericXters = true;
            bool useSpecialXters = true;
            bool areUnique = true;
            int numberOfXters = Convert.ToInt32(length);

            RandomStringGenerator RSG = new RandomStringGenerator(useUpperCase, useLowerCase, useNumericXters, useSpecialXters);
            RSG.UniqueStrings = areUnique;
            String code = RSG.Generate(numberOfXters);
            return code;
        }


        public static string getcode(String codeName, ref Environment env)
        {
            codeName = Bee.Queen.unpack(codeName, ref env);
            bool useUpperCase = false;
            bool useLowerCase = false;
            bool useNumericXters = false;
            bool useSpecialXters = false;
            bool areUnique = true;
            int numberOfXters = 4;

            //get user settings
            JToken codeConfig = Bee.Engine.Hive.SelectToken("code." + codeName);
            //the case
            //upper&lower upper lower lower&upper
            JToken val = codeConfig["case"];//
            if(val != null){
                String caseStyle = (String)val;
                if (caseStyle.Equals("upper&lower") || caseStyle.Equals("lower&upper"))
                {
                    useUpperCase = true;
                    useLowerCase = true;
                }
                else if (caseStyle.Equals("upper"))
                {
                    useUpperCase = true;
                }
                else if (caseStyle.Equals("lower"))
                {
                    useLowerCase = true;
                }
            }
            
            //the numerics
            val = codeConfig["useNumerics"];
            if (val != null)
            {
                useNumericXters = Convert.ToBoolean(val);
            }
            //the specials
            val = codeConfig["useSpecials"];
            if (val != null)
            {
                useSpecialXters = Convert.ToBoolean(val);
            }
            //length
            val = codeConfig["length"];
            if (val != null)
            {
                numberOfXters = Convert.ToInt32(val);
            }

            RandomStringGenerator RSG = new RandomStringGenerator(useUpperCase, useLowerCase, useNumericXters, useSpecialXters);
            RSG.UniqueStrings = areUnique;
            String code = RSG.Generate(numberOfXters);
            return code;
        }

        public static List<String> EatAlot(string jellyDefinition, ref Environment env)
        {
            var output = new List<String>();
            string[] jellyParts = jellyDefinition.Trim().Split(' ').Where(
                jp => !String.IsNullOrEmpty(jp.Trim())
            ).Select(jp => jp.Trim()).ToArray<string>();
            switch (jellyParts[0])
            {
                case "split":
                    output = split(jellyParts[1], jellyParts[2], ref env);
                    break;
                default:
                    break;
            }
            return output;
        }


        private static List<String> split(string whatToSplit, string niddle, ref Environment env)
        {
            whatToSplit = Bee.Queen.unpack(whatToSplit, ref env);
            niddle = Bee.Queen.unpack(niddle, ref env);

            string[] str = whatToSplit.Split(niddle[0]);

            return str.ToList<String>();
        }


        private static string join(List<string> args, ref Environment env)
        {                                                                                                                                                           
            string str = "";
            foreach (string arg in args)
            {
                str += Bee.Queen.unpack(arg, ref env);
            }
            return str;
        }

        private static string crypt(string whatToencrypt, string encryptionKey, ref Environment env)
        {
            string str = "";
            whatToencrypt = Bee.Queen.unpack(whatToencrypt, ref env);
            encryptionKey = Bee.Queen.unpack(encryptionKey, ref env); 
            str = Encrypter.Encrypt(whatToencrypt, encryptionKey);
            return str;
        }

        private static string decrypt(string whatToDencrypt, string encryptionKey, ref Environment env)
        {
            string str = "";
            whatToDencrypt = Bee.Queen.unpack(whatToDencrypt, ref env);
            encryptionKey = Bee.Queen.unpack(encryptionKey, ref env); 
            str = Encrypter.Decrypt(whatToDencrypt, encryptionKey);
            return str;
        }

        public static string Lay(string egg, JObject externalParameters, JObject contextObject = null, bool isFlowerCall = false){
            var eggStr = "";
            string[] eggParts = egg.Split(' ');
            string eggFunction = "";
            List<string> eggArguments = new List<string>();
            for (int i = 0; i < eggParts.Length; i++)
            {
                string eggPart = eggParts[i];
                eggPart = eggPart.Trim();
                if (!String.IsNullOrEmpty(eggPart))
                {
                    if (i == 0)
                    {
                        eggFunction = eggPart;
                    }
                    else
                    {
                        eggArguments.Add(eggPart);
                    }
                }
            }
            switch (eggFunction)
            {
                case "join":
                    eggStr = join(eggArguments, externalParameters, contextObject, isFlowerCall);
                    break;
                case "encrypt":
                    eggStr = encrypt(eggArguments[0], eggArguments[1], externalParameters, contextObject, isFlowerCall);
                    break;
                case "dencrypt":
                    eggStr = dencrypt(eggArguments[0], eggArguments[1], externalParameters, contextObject, isFlowerCall);
                    break;
                default:
                    break;
            }
            return eggStr;
        }

        public static List<Object> LayMany(string egg, JObject externalParameters, JObject contextObject = null, bool isFlowerCall = false)
        {
            List<Object> eggStr = new List<object>();
            string[] eggParts = egg.Split(' ');
            string eggFunction = "";
            List<string> eggArguments = new List<string>();
            for (int i = 0; i < eggParts.Length; i++)
            {
                string eggPart = eggParts[i];
                eggPart = eggPart.Trim();
                if (!String.IsNullOrEmpty(eggPart))
                {
                    if (i == 0)
                    {
                        eggFunction = eggPart;
                    }
                    else
                    {
                        eggArguments.Add(eggPart);
                    }
                }
            }
            switch (eggFunction)
            {
                case "dencrypt":
                    eggStr = split(eggArguments[0], eggArguments[1], externalParameters, contextObject, isFlowerCall);
                    break;
                default:
                    break;
            }
            return eggStr;
        }

        private static string join(List<string> args, JObject extParams, JObject contextObject, bool isFlowerCall)
        {
            string str = "";
            foreach (string arg in args)
            {
                //nyd
                //first process this arg
                //dont forget cases like _s which stand for space, _DT
                bool isExternal = arg.StartsWith("_@") || arg.StartsWith("_f") || arg.StartsWith("_@h_") || arg.StartsWith("_l");
                string argModified = Bee.Queen.prepareParam(arg, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
                str = str + argModified;
            }
            return str;
        }

        private static List<Object> split(string whatToSplit, string niddle, JObject extParams, JObject contextObject, bool isFlowerCall)
        {
            
            bool isExternal = whatToSplit.StartsWith("_@") || whatToSplit.StartsWith("_f") || whatToSplit.StartsWith("_@h_")  || whatToSplit.StartsWith("_l");
            string argModified = Bee.Queen.prepareParam(whatToSplit, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            whatToSplit = argModified;

            isExternal = niddle.StartsWith("_@") || niddle.StartsWith("_f") || niddle.StartsWith("_@h_") || niddle.StartsWith("_l");
            argModified = Bee.Queen.prepareParam(niddle, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            niddle = argModified;

            string[] str = whatToSplit.Split(niddle[0]);

            return str.ToList<Object>();
        }

        private static string encrypt(string whatToencrypt, string encryptionKey, JObject extParams, JObject contextObject, bool isFlowerCall)
        {
            //_@desc _mySweetHoney
            string str = "";
            bool isExternal = whatToencrypt.StartsWith("_@") || whatToencrypt.StartsWith("_f_") || whatToencrypt.StartsWith("_@h_");
            whatToencrypt = Bee.Queen.prepareParam(whatToencrypt, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            isExternal = encryptionKey.StartsWith("_@") || encryptionKey.StartsWith("_f_") || encryptionKey.StartsWith("_@h_");
            encryptionKey = Bee.Queen.prepareParam(encryptionKey, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            str = Encrypter.Encrypt(whatToencrypt, encryptionKey);
            return str;
        }

        private static string dencrypt(string whatToDencrypt, string encryptionKey, JObject extParams, JObject contextObject, bool isFlowerCall)
        {
            //_@desc _mySweetHoney
            string str = "";
            bool isExternal = whatToDencrypt.StartsWith("_@") || whatToDencrypt.StartsWith("_f_") || whatToDencrypt.StartsWith("_@h_");
            whatToDencrypt = Bee.Queen.prepareParam(whatToDencrypt, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            isExternal = encryptionKey.StartsWith("_@") || encryptionKey.StartsWith("_f_") || encryptionKey.StartsWith("_@h_");
            encryptionKey = Bee.Queen.prepareParam(encryptionKey, isExternal ? extParams : contextObject, isExternal, isFlowerCall);
            str = Encrypter.Decrypt(whatToDencrypt, encryptionKey);
            return str;
        }

        

        public static string prepareQuotedParam(string param, JObject context, bool isExternal, bool isFlowerCall)
        {
            //by default the mood is in production so that you have to explicity
            //change it to dev to use _@h_ when done then delete it  or change it to pro
            if ((param.StartsWith("_@h_") && Bee.Hive.Mood == "dev" && Hive.isSeedding == true) || (param.StartsWith("_@h_") && isFlowerCall == true))
            {
                //only iterprete in dev mode
                string hiveValue = "";
                string []potionParts = param.Split('_');
                JObject currentHiveRef = Hive.hiveJObject;
                for (int i = 0; i < potionParts.Length; i++)
			    {
                    string porsh = potionParts[i];
                    porsh = porsh.Trim();
                    if(string.IsNullOrEmpty(porsh) || porsh.Equals("@h")){
                        continue;
                    }
			        currentHiveRef = (JObject)currentHiveRef[porsh];
			    }
                dynamic portValue = currentHiveRef;
                hiveValue = portValue;
                return hiveValue;
            }

            if (isExternal)
            {
                string paramName = param.Substring(2);
                //check if context has this node at the root
                bool hasNode = Bee.Workers.HasNode(context, paramName);
                if (hasNode == false)
                {
                    return param;
                }
                dynamic dynWat = context[paramName];
                string paramValue = dynWat;
                string quotedParam = Bee.Hive.GetQuoted(paramValue, context[paramName].Type);
                return quotedParam;
            }
            return param;
        }

        public static string prepareParam(string param, JObject context, bool isExternal, bool isFlowerCall)
        {
            if ((param.StartsWith("_@h_") && Bee.Hive.Mood == "dev" && Hive.isSeedding == true) || (param.StartsWith("_@h_") && isFlowerCall == true))
            {
                //only iterprete in dev mode
                //_@h_security_secretPotion => .@h.security.secretPotion => security.secretPotion
                param = param.Replace('_', '.').Substring(".@h.".Length);
                String hiveValue = (String)Bee.Engine.Hive.SelectToken(param);
                return hiveValue;
            }else if (param.StartsWith("_@") && isExternal == true)
            {
                param = param.Substring(2);
                //we wil asume that for external params the values are on the root
                //and not deep linked
                dynamic dynWat = context[param];
                param = dynWat;
            }
            else if (param.StartsWith("_f_") && isExternal == true)
            {
                //it should be in the eternal refs
                dynamic dynWat = context[param];
                param = dynWat;
            }
            else if (param.StartsWith("_"))
            {
                param = param.Substring(1);
            }
            else
            {
                //this is the situation
                //"_f_joined" : "join Name _: Description",
                //specifically here we are looking at Name and Description
                //these could be deeply linked Object.Childrens[1].Name
                //nyd the adboev posibility needs to be implemented and tested
                //for now will consider root values
                //check if the parameter is a path
                string parentName = "";
                foreach (var item in context)
                {
                    parentName = item.Key;
                    break;
                }
                JToken content = context[parentName];
                if (context.Type == JTokenType.Object)
                {
                    JObject objContent = (JObject)content;
                    bool hasNode = Bee.Workers.HasNode(objContent, param);
                    if (hasNode == true)
                    {
                        dynamic dynWat = objContent[param];
                        param = dynWat;
                    }
                }
            }
            return param;
        }

        public static string unpack(string jellyPart, ref Environment env)
        {
            //nyd
            //first process this arg
            //dont forget cases like _s which stand for space, _DT
            jellyPart = jellyPart.Trim();
            bool isExternal = jellyPart.StartsWith(Bee.Scents.Scent);
            if(jellyPart.StartsWith(Bee.Scents.AtHive) && ((Bee.Hive.Mood == "dev" && Bee.Hive.isSeedding) ||  env.IsFlowerCall ) )
            {
                var x = Bee.Engine.Hive.SelectToken(jellyPart.Replace(Bee.Scents.Scent, ".").Substring(Bee.Scents.AtHive.Length));
                String xx = x.ToString();
                return xx;
            }
            else if (jellyPart.StartsWith(Bee.Scents.AtUser)) //&& ((Bee.Hive.Mood == "dev" && Bee.Hive.isSeedding) || env.IsFlowerCall)
            {
                string path = jellyPart.Replace(Bee.Scents.Scent, ".").Substring(Bee.Scents.AtUser.Length);
                //'_@u_UserId'
                //'_@u_Name'
                //'_@u_UserInRoles[0]_Role_RoleId'
                //'_@u_UserInRoles[0]_Role_Accesses[0]_CanDo'
                var x = env.CurrentUser.SelectToken(path);
                String xx = x.ToString();
                return xx;
            }
            else if (jellyPart.StartsWith(Bee.Scents.AtDateTime))
            {
                //_@d_now
                string path = jellyPart.Replace(Bee.Scents.Scent, ".").Substring(Bee.Scents.AtDateTime.Length);
                path = Bee.Date.Get(path);
                return path;
            }
            else if (jellyPart.StartsWith(Bee.Scents.ObtainObject)) //_oo_User_UserId
            {
                string path = jellyPart.Substring(Bee.Scents.ObtainObject.Length);
                //look up the value of this
                string[] pathx = path.Split('_'); //User_UserId
                string tokenPath = path.Substring(pathx[0].Length+1).Replace('_', '.'); //UserId
                JToken fkValue = env.RetainedObjects[pathx[0]].SelectToken(tokenPath); //[User] [UserId]
                return fkValue.ToString();
            }
            else if (jellyPart.StartsWith(Bee.Scents.At) && isExternal == true)
            {
                String pt = jellyPart.Substring(2);
                if (env.ContextObject == null)
                {
                    return pt;
                }
                
                var Sample = env.ContextObject[pt];
                if (Sample == null || Sample.Type == JTokenType.Null)
                {
                    if (env.IsFlowerCall && env.IsPosting == true)
                    {
                        string wtf = "";
                        //get the first thing
                        foreach (var item in env.ContextObject)
                        {
                            wtf = (String)item.Value[jellyPart.Substring(2)];
                            break;
                        }
                        return wtf;
                    }
                    return pt;
                }
                else
                {
                    return (String)Sample;
                }
                
            }
            else if (jellyPart.StartsWith(Bee.Scents.JellyImmediate) && !jellyPart.StartsWith(Bee.Scents.JellyAfterMath) && isExternal == true)
            {
                return (string)env.ImmediateJellyValues[jellyPart];
            }
            else if (jellyPart.StartsWith(Bee.Scents.JellyAfterMath) && isExternal == true)
            {
                //return (string)env.AfterMathJellyValues[jellyPart];
                //return (string)env.CurrentHoneyRef[jellyPart.Substring(Bee.Scents.JellyAfterMath.Length)];

                //first check in the context object
                JToken val = env.CurrentHoneyRef[jellyPart.Substring(Bee.Scents.JellyAfterMath.Length)];
                if (val == null && !String.IsNullOrEmpty(env.searchAfterJellyKey))
                {
                    string rep = jellyPart.Substring(Bee.Scents.JellyAfterMath.Length);
                    string ssq = env.searchAfterJellyKey.Replace("~", rep);
                    string strV = env.AfterMathJellyValues[ssq];
                    return strV;
                }
                return (String)val;
            }
            else if (jellyPart.StartsWith(Bee.Scents.Pot))
            {
                return (string)env.Pots[jellyPart];
            }
            else if (jellyPart.StartsWith(Bee.Scents.JellyImmediateList) && !jellyPart.StartsWith(Bee.Scents.JellyAfterMathList) && isExternal == true)
            {   //_lstrSplits[0]
                string[] jellyNameParts = jellyPart.Split('[');
                int index = Convert.ToInt32(jellyNameParts[1].Trim(']'));
                string jellyName = jellyNameParts[0];
                var list = (List<String>)env.ImmediateListJellyValues[jellyName];
                return (index < list.Count())?list[index]:null;
            }
            else if (jellyPart.StartsWith(Bee.Scents.JellyAfterMathList) && isExternal == true)
            {
                string[] jellyNameParts = jellyPart.Split('[');
                int index = Convert.ToInt32(jellyNameParts[1].Trim(']'));
                string jellyName = jellyNameParts[0];
                var list = (List<String>)env.AfterMathListJellyValues[jellyName];
                return (index < list.Count()) ? list[index] : null;
            }
            else if (jellyPart.StartsWith(Bee.Scents.Scent))
            {
                return jellyPart.Substring(1);
            }
            else
            {
                return (String)env.CurrentHoneyRef[jellyPart];
            }
        }

        public static void extend(ref JObject nector, JObject exP)
        {
            dynamic extendCommand = exP["_"];
            if (extendCommand != null)
            {
                List<string> keySents = new List<string>() { "_c", "_f_", "_f" };
                List<string> nectorRoots = new List<string>();
                foreach (var nectorNode in nector)
                {
                    if (!keySents.Contains(nectorNode.Key))
                    {
                        nectorRoots.Add(nectorNode.Key);
                    }
                }
                //extensions are arrays having a one to one mapping to the nector
                JArray extensions = (JArray)exP["_"];
                for (int extIndex = 0; extIndex < extensions.Count() && extIndex < nectorRoots.Count(); extIndex++)
                {
                    JObject jo = (JObject)extensions[extIndex];
                    string combNameForExt = nectorRoots[extIndex];
                    string realCombNameForExt = Hive.GetRealCombName(combNameForExt);
                    Bee.Trap tempTrap = new Trap();
                    Bee.Workers.PrepareAttributes(ref jo, realCombNameForExt, ref tempTrap);

                    //add this to the nector
                    foreach (var joNode in jo)
                    {
                        if (joNode.Key.Equals(Bee.Scents.Attribute))
                        {
                            dynamic xxa = joNode.Value;
                            string xa = xxa;
                            string[] xax = xa.Split(' ');
                            if (Workers.HasNode((JObject)nector[combNameForExt], Bee.Scents.Attribute))
                            {
                                dynamic foo = ((JObject)nector[combNameForExt])[Bee.Scents.Attribute];
                                string xfoo = foo;
                                List<string> xfooParts = xfoo.Split(' ').ToList<string>();
                                for (int ixa = 0; ixa < xax.Length; ixa++)
                                {
                                    string tempXa = xax[ixa];
                                    tempXa = tempXa.Trim();
                                    if (string.IsNullOrEmpty(tempXa))
                                    {
                                        continue;
                                    }
                                    if (xfooParts.Contains(tempXa))
                                    {
                                        continue;
                                    }
                                    xfooParts.Add(tempXa);
                                }
                                string paticoz = "";
                                foreach (var xfooPart in xfooParts)
                                {
                                    paticoz = paticoz + " " + xfooPart;
                                }
                                ((JObject)nector[combNameForExt])[Bee.Scents.Attribute] = paticoz;
                            }
                            else
                            {
                                ((JObject)nector[combNameForExt]).Add(Bee.Scents.Attribute, xa);
                            }
                        }
                        else
                        {
                            ((JObject)nector[combNameForExt]).Add(joNode.Key, joNode.Value);
                        }
                    }
                }
            }
        }


        
        public static void include(ref JObject nector, JObject exP, ref Environment env) //exP are external parameter actually nector from client
        {
            dynamic extendCommand = exP["_"];
            if (extendCommand != null)
            {
                //we dont expect one to have scents at the root of his server side nector
                List<string> nectorRoots = new List<string>();
                foreach (var nectorNode in nector)
                {
                    if (!nectorNode.Key.StartsWith(Bee.Scents.Scent))
                    {
                        nectorRoots.Add(nectorNode.Key);
                    }
                }
                //extensions are arrays having a one to one mapping to the nector
                JArray extensions = (JArray)exP["_"];
                for (int extIndex = 0; extIndex < extensions.Count() && extIndex < nectorRoots.Count(); extIndex++)
                {
                    JObject jo = (JObject)extensions[extIndex];
                    string combNameForExt = nectorRoots[extIndex];
                    string realCombNameForExt = Bee.Hive.GetRealCombName(combNameForExt);

                    
                    //prepare the cells that this structre is looking for
                    Bee.Workers.PrepareCells(ref jo, realCombNameForExt, combNameForExt, ref env);
                    

                    //add this to the nector
                    foreach (var joNode in jo)
                    {
                        if (joNode.Key.Equals(Bee.Scents.Attribute))
                        {
                            dynamic xxa = joNode.Value;
                            string xa = xxa;
                            string[] xax = xa.Split(' ');
                            var temp  = ((JObject)nector[combNameForExt])[Bee.Scents.Attribute];
                            if (temp != null)
                            {
                                dynamic foo = temp;
                                string xfoo = foo;
                                List<string> xfooParts = xfoo.Split(' ').ToList<string>();
                                for (int ixa = 0; ixa < xax.Length; ixa++)
                                {
                                    string tempXa = xax[ixa];
                                    tempXa = tempXa.Trim();
                                    if (string.IsNullOrEmpty(tempXa))
                                    {
                                        continue;
                                    }
                                    if (xfooParts.Contains(tempXa))
                                    {
                                        continue;
                                    }
                                    xfooParts.Add(tempXa);
                                }
                                string paticoz = "";
                                foreach (var xfooPart in xfooParts)
                                {
                                    paticoz = paticoz + " " + xfooPart;
                                }
                                ((JObject)nector[combNameForExt])[Bee.Scents.Attribute] = paticoz;
                            }
                            else
                            {
                                ((JObject)nector[combNameForExt]).Add(Bee.Scents.Attribute, xa);
                            }
                        }
                        else
                        {
                            ((JObject)nector[combNameForExt]).Add(joNode.Key, joNode.Value);
                        }
                    }
                }
            }
        }

    }
}