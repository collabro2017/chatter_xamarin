using Chadder.Data.Response;
using Chadder.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Util
{
    public static class HttpExtensions
    {
        public static async Task<WebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            return await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
        }

        public static async Task PostAsync(this HttpWebRequest request, byte[] data)
        {
            using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, request))
            {
                await requestStream.WriteAsync(data, 0, data.Length);
            }
        }
    }

    public class ChadderRequest
    {
#if !WINDOWS_PHONE && !WINDOWS_APP
        public ChadderRequest()
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateRemoteCertificate;
        }
        static private bool ValidateRemoteCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (certificate.Subject.Contains("service.chadder.im"))
            {
                return certificate.GetCertHashString() == "FD096DBACCFA2A0C32554D3825FDB82F72209057" && sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            }
            return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
        }
#else
        public ChadderRequest()
        {
        }
#endif

        protected CookieContainer cookieContainer = new CookieContainer();

        public string Token { get; set; }

        protected async Task<Stream> PostRequest(Uri uri, byte[] byteArray, string type = "application/json")
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(uri);

            if (Token != null)
                request.Headers["Authorization"] = "Bearer " + Token;
            request.ContentType = type;
            request.Method = "POST";
            request.Accept = "application/bson";
#if !WINDOWS_APP
            request.ContentLength = byteArray.Length;
            request.AllowAutoRedirect = false;
#endif
            await request.PostAsync(byteArray);
            var response = await request.GetResponseAsync();
            return response.GetResponseStream();
        }
        private async Task<Stream> PostRequestAPIStream(string url, object data)
        {
            var uri = new Uri(url);
            string str = null;
            if (data != null)
                str = JsonConvert.SerializeObject(data);
            byte[] byteArray = null;
            if (str != null)
                byteArray = Encoding.UTF8.GetBytes(str);
            else
                byteArray = new byte[0];

            return await PostRequest(uri, byteArray);
        }
        public async Task<T> PostRequestAPI<T>(string url, object data = null) where T : BasicResponse, new()
        {
            try
            {
                return await Task.Run<T>(async () =>
                {
                    var content = await PostRequestAPIStream(url, data);
                    Newtonsoft.Json.Bson.BsonReader reader = new Newtonsoft.Json.Bson.BsonReader(content);
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(reader);
                });
            }
            catch (Exception ex)
            {
                Insight.Track(ex.Message, ChadderError.CONNECTION_ERROR);
                var r = new T();
                r.Error = ChadderError.CONNECTION_ERROR;
                r.InnerException = ex;
                return r;
            }
        }


        public async Task<JObject> PostRequestAPI(string url, object data = null)
        {
            try
            {
                return await Task.Run<JObject>(async () =>
                {
                    var content = await PostRequestAPIStream(url, data);
                    Newtonsoft.Json.Bson.BsonReader reader = new Newtonsoft.Json.Bson.BsonReader(content);
                    return JObject.Load(reader);
                });
            }
            catch (Exception ex)
            {
                Insight.Track(ex.Message, ChadderError.CONNECTION_ERROR);
                var r = new BasicResponse();
                r.Error = ChadderError.CONNECTION_ERROR;
                r.InnerException = ex;
                return JObject.FromObject(r);
            }
        }
    }
}
