using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Sms
    {
        public static void SendViaMagezi(List<string> numbers, String msg)
        {
            try
            {

                string nameOfSender = (String)Bee.Engine.Hive.SelectToken("sms.name");
                string nameOfapp = (String)Bee.Engine.Hive.SelectToken("sms.app");
                string password = (String)Bee.Engine.Hive.SelectToken("sms.password");
                string user = (String)Bee.Engine.Hive.SelectToken("sms.user");
                List<string> done = new List<string>();

                for (var s = 0; s < numbers.Count(); s++)
                {
                    string url = "http://relay.magezi.net/tosms.php?tel=" + numbers[s];
                    url = url + "&msg=" + msg;
                    url = url + "&name=" + nameOfSender; 
                    url = url + "&app=" + nameOfapp;
                    url = url + "&relay_password=" + password;
                    url = url + "&relay_user=" + user;

                    Curl curl = new Curl(url);
                    String x = curl.CallGet();
                    done.Add(x);
                }

                //nyd
                //to save sent messages to the db in one of the configured
                //or auto tables 
                //also record the current id of the user who sent the sms
                //should we put provision for users to get accounts to 
                //these sms providers via their only app, or we tell them
                //to go and create accounts like on magezi and put their configurations here
                //how are we going to handle these errors and responses
            }
            catch (Exception ex)
            {
                
            }
        }

        //public static string SendViaClockWork(List<string> numbers, string msg)
        //{
        //    try
        //    {
        //        Clockwork.API api = new Clockwork.API("6ffe2c3f15cc1e231c181d4c13c256b7f1d52133");
        //        SMSResult result = api.Send(
        //            new SMS
        //            {
        //                To = to,
        //                Message = msg
        //            });

        //        if (result.Success)
        //        {
        //            return "SMS Sent to " + result.SMS.To + ", Clockwork ID: " + result.ID;
        //        }
        //        else
        //        {
        //            return "SMS to " + result.SMS.To + " failed, Clockwork Error: " + result.ErrorCode + " " + result.ErrorMessage;
        //        }
        //    }
        //    catch (APIException ex)
        //    {
        //        // You’ll get an API exception for errors
        //        // such as wrong username or password
        //        return "API Exception: " + ex.Message;
        //    }
        //    catch (WebException ex)
        //    {
        //        // Web exceptions mean you couldn’t reach the Clockwork server
        //        return "Web Exception: " + ex.Message;
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        // Argument exceptions are thrown for missing parameters,
        //        // such as forgetting to set the username
        //        return "Argument Exception: " + ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Something else went wrong, the error message should help
        //        return "Unknown Exception: " + ex.Message;
        //    }
        //}
    }
}