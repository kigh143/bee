using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bee
{
    public class Mail
    {

        public static async Task SendViaMailJet(String to, String subject, String msg)
        {
            string senderEmail = (String)Bee.Engine.Hive.SelectToken("email.sender");
            string senderName = (String)Bee.Engine.Hive.SelectToken("email.name");
            string publickey = (String)Bee.Engine.Hive.SelectToken("email.publickey");
            string apiSecret = (String)Bee.Engine.Hive.SelectToken("email.privatekey");

           
            MailjetClient client = new MailjetClient(publickey, apiSecret);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }.Property(Send.FromEmail, senderEmail)
            .Property(Send.FromName, senderName)
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, msg)
            .Property(Send.Recipients, new JArray (){
                new JObject {
                    {"Email", to}
                }
            });
            //.Property(Send.TextPart, "Dear passenger, welcome to Mailjet! May the delivery force be with you!")
            MailjetResponse response = await client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
            }
        }


        //public static void  RunAsync(String toEmail, String subject, String msg)
        //{
        //    //try to send email
        //    try
        //    {
        //        string senderEmail = (String)Bee.Engine.Hive.SelectToken("email.sender");
        //        string apiKey = (String)Bee.Engine.Hive.SelectToken("email.key");
        //        string apiSecret = (String)Bee.Engine.Hive.SelectToken("email.secret");

        //        //string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/EmailTemplates/signupverificationcode.html");
        //        //string allText = System.IO.File.ReadAllText(templatePath);
        //        //string nem = fileDocsUser.FirstName.ToUpper() + " " + fileDocsUser.LastName.ToUpper();
        //        //allText = allText.Replace("tigidiName", nem);
        //        //allText = allText.Replace("tigidiCode", fileDocsUser.VerificationCode);

        //        //
        //        //string senderEmail = WebConfigurationManager.AppSettings["emailSender"];
        //        //if (String.IsNullOrEmpty(senderEmail))
        //        //{
        //        //    senderEmail = "quisoplay@gmail.com";
        //        //}
        //        MailJetClient client = new MailJetClient("4a75690468200c81130b9d52ceb38a8d", "ecd8ba20e0d0565544a52414f9aad70b");
        //        System.Net.Mail.MailMessage staff = new System.Net.Mail.MailMessage(senderEmail, toEmail, subject, msg);
        //        staff.IsBodyHtml = true;

        //        var x = client.SendMessage(staff);
        //    }
        //    catch (Exception ex)
        //    {
        //        Xpsion xp = new Xpsion()
        //        {
        //            Controller = "FileDocsUserController",
        //            Date = DateTime.Now,
        //            Message = "Error trying to send signup verfication code :" + ex.Message,
        //            Method = "PostFileDocsUser",
        //            StackTrace = ex.StackTrace,
        //            Other = ""
        //        };
        //        unitOfWork.XpsionRepository.Insert(xp);
        //        unitOfWork.Save();
        //    }
        //}
    }
}