using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Threading;

namespace CommonLIB.Network.HTTP
{
    public static class HttpTool
    {
        public static void Post(Dictionary<string, object> pam, string url)
        {
            ThreadPool.QueueUserWorkItem(Run, new object[] { pam, url });
        }
        public static void Run(object o)
        {
            try
            {
                Dictionary<string, object> pam = (Dictionary<string, object>)(((object[])o)[0]);
                string url = (string)(((object[])o)[1]);
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                //request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                string pamString = "DeviceNumber=Push&Platform=Web&VersionCode=0";
                foreach (var v in pam)
                {
                    pamString += "&" + v.Key + "=" + v.Value.ToString();
                }
                byte[] data = Encoding.UTF8.GetBytes(pamString);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Flush();
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                byte[] dataTemp = new byte[response.ContentLength];
                responseStream.Read(dataTemp, 0, dataTemp.Length);
                responseStream.Close();
                requestStream.Close();
                responseStream.Dispose();
                requestStream.Dispose();
            }
            catch { }
        }

        public static string GetString(string url, Dictionary<string, object> pam)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                string pamString = "DeviceNumber=Push&Platform=Web&VersionCode=0";
                foreach (var v in pam)
                {
                    pamString += "&" + v.Key + "=" + v.Value.ToString();
                }
                byte[] data = Encoding.UTF8.GetBytes(pamString);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Flush();
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                byte[] dataTemp = new byte[response.ContentLength];
                responseStream.Read(dataTemp, 0, dataTemp.Length);
                responseStream.Close();
                requestStream.Close();
                responseStream.Dispose();
                requestStream.Dispose();
                return Encoding.UTF8.GetString(dataTemp);
            }
            catch { return null; }
        }
    }
}
