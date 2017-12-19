using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace Bee
{
    public class Curl
    {
        HttpWebRequest httpWebRequest;
        WebRequest request;

        public Curl(string url, string accessToken, string contentType, string method)
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Headers["Authorization"] = accessToken;
            httpWebRequest.ContentType = contentType;
            httpWebRequest.Method = method;
        }

        public Curl(string url)
        {
            request = WebRequest.Create(url);
            request.Method = "GET";

        }

        public string CallGet()
        {
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        string result = streamReader.ReadToEnd();
                        return result;
                    }
                }
            }

        }

        public void Call()
        {
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                //string json = new JavaScriptSerializer().Serialize(new
                //{
                //    email = strEmail,
                //    password = strPassword
                //});

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
        }




    }
}