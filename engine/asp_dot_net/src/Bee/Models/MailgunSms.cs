//using Clockwork;
//using RestSharp;
//using RestSharp.Authenticators;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Web;

//namespace FileDocs.Tools
//{
//    public class MailgunSms
//    {
//        public static IRestResponse SendSimpleMessage(string to, string msg)
//        {
//            RestClient client = new RestClient();
//            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
//            client.Authenticator = new HttpBasicAuthenticator("api", "key-8296ea7eba0d20fe698ebba657445f29");
//            RestRequest request = new RestRequest();
//            request.AddParameter("domain", "quisoplay.com", ParameterType.UrlSegment);
//            //request.Resource = "quisoplay.com/messages";
//            request.AddParameter("from", "filedocs <quisoplay@gmail.com>");
//            request.AddParameter("to", to);
//            request.AddParameter("subject", "filedocs account verification code");
//            request.AddParameter("html", msg);
//            request.Method = Method.POST;
            
//            return client.Execute(request);
//        }

//        public static void SendSimpleMessage()
//        {
//            // Compose a message
//            MailMessage mail = new MailMessage("quisoplay@gmail.com", "bar@example.com");
//            mail.Subject = "Hello";
//            mail.Body = "Testing some Mailgun awesomness";

//            // Send it!
//            SmtpClient client = new SmtpClient();
//            client.Port = 587;
//            client.DeliveryMethod = SmtpDeliveryMethod.Network;
//            client.UseDefaultCredentials = false;
//            client.Credentials = new System.Net.NetworkCredential("postmaster@YOUR_DOMAIN_NAME", "3kh9umujora5");
//            client.Host = "smtp.mailgun.org";

//            client.Send(mail);
//        }


//        public static string SendClockSms(string to, string msg)
//        {
//            try
//            {
//                Clockwork.API api = new Clockwork.API("6ffe2c3f15cc1e231c181d4c13c256b7f1d52133");
//                SMSResult result = api.Send(
//                    new SMS {
//                                To = to, 
//                                Message = msg 
//                            });

//                if(result.Success)
//                {
//                    return "SMS Sent to " + result.SMS.To + ", Clockwork ID: " + result.ID;
//                }
//                else
//                {
//                    return "SMS to " + result.SMS.To + " failed, Clockwork Error: " + result.ErrorCode + " " + result.ErrorMessage;
//                }
//            }
//            catch (APIException ex)
//            {
//                // You’ll get an API exception for errors
//                // such as wrong username or password
//                return "API Exception: " + ex.Message;
//            }
//            catch (WebException ex)
//            {
//                // Web exceptions mean you couldn’t reach the Clockwork server
//                return "Web Exception: " + ex.Message;
//            }
//            catch (ArgumentException ex)
//            {
//                // Argument exceptions are thrown for missing parameters,
//                // such as forgetting to set the username
//                return "Argument Exception: " + ex.Message;
//            }
//            catch (Exception ex)
//            {
//                // Something else went wrong, the error message should help
//                return "Unknown Exception: " + ex.Message;
//            }
//        }


//    }
//}