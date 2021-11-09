using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TimHanewich.TelemetryFeed.Sql;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimHanewich.TelemetryFeed.Service
{
    public class TelemetryFeedService
    {
        private Guid Key;

        public TelemetryFeedService(Guid auth_key)
        {
            Key = auth_key;
        }

        private async Task<string> ExecuteSqlAsync(string query) //The returned string is the response (in JSON probably)
        {
            HttpRequestMessage req = PrepareHttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://telemetryfeedapi.azurewebsites.net/api/sql");
            req.Content = new StringContent(query);

            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string backback = await resp.Content.ReadAsStringAsync();

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request to server failed with code '" + resp.StatusCode.ToString() + "'. Msg: " + backback);
            }

            return backback;
        }

        private HttpRequestMessage PrepareHttpRequestMessage()
        {
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("key", Key.ToString());
            return req;
        }
    
    
    


        //UPLOADS

        public async Task UploadRegisteredUserAsync(RegisteredUser user)
        {
            await ExecuteSqlAsync(user.ToSqlInsert());
        }

        public async Task UploadSessionAsync(Session s)
        {
            await ExecuteSqlAsync(s.ToSqlInsert());
        }

        public async Task UploadTelemetrySnapshotAsync(TelemetrySnapshot ts)
        {
            HttpRequestMessage req = PrepareHttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://telemetryfeedapi.azurewebsites.net/api/telemetrysnapshot");
            
            //Write the body
            byte[] bytes = ts.ToBytes();
            MemoryStream ms = new MemoryStream(bytes);
            req.Content = new StreamContent(ms);

            //Set the content header now that the content is loaded in
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            //Post it
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            if (resp.StatusCode != HttpStatusCode.Created)
            {
                string body = await resp.Content.ReadAsStringAsync();
                throw new Exception("Upload of TelemetrySnapshot failed with code '" + resp.StatusCode.ToString() + "'. Msg: " + body);
            }
        }
    
    
    
        //Downloads

        public async Task<Session[]> DownloadSessionsAsync(Guid owner_id)
        {
            string cmd = CoreSqlExtensions.DownloadSessions(owner_id);

            //Call
            string response = null;
            try
            {
                response = await ExecuteSqlAsync(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception("Request to download sessions failed. Msg: " + ex.Message);
            }

            //Parse
            JArray ja = null;
            try
            {
                ja = JArray.Parse(response);
            }
            catch
            {
                throw new Exception("Internal error. Unable to parse response from service.");
            }

            //Get each
            List<Session> ToReturn = new List<Session>();
            foreach (JObject jo in ja)
            {
                ToReturn.Add(JsonConvert.DeserializeObject<Session>(jo.ToString()));
            }

            return ToReturn.ToArray();
        }
    
        public async Task<RegisteredUser> DownloadRegisteredUserAsync(string username)
        {
            string cmd = CoreSqlExtensions.DownloadRegisteredUser(username);
            
            //Call
            string response = null;
            try
            {
                response = await ExecuteSqlAsync(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while calling service. Msg: " + ex.Message);
            }

            //Parse the body
            JArray ja = JArray.Parse(response);
            if (ja.Count == 0)
            {
                throw new Exception("Unable to find Registered User with Username '" + username + "'");
            }


            //Parse the body
            RegisteredUser ToReturn = null;
            try
            {
                ToReturn = JsonConvert.DeserializeObject<RegisteredUser>(ja[0].ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while parsing returned content. Msg: " + ex.Message);
            }

            return ToReturn;
        }

        public async Task<RegisteredUser> DownloadRegisteredUserAsync(Guid id)
        {
            string cmd = CoreSqlExtensions.DownloadRegisteredUser(id);
            string response = null;
            try
            {
                response = await ExecuteSqlAsync(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while downloading registered user '" + id.ToString() + "': " + ex.Message);
            }

            //Parse the body
            JArray ja = JArray.Parse(response);
            if (ja.Count == 0)
            {
                throw new Exception("Unable to find Registered User with Id '" + id.ToString() + "'");
            }


            //Parse the body
            RegisteredUser ToReturn = null;
            try
            {
                ToReturn = JsonConvert.DeserializeObject<RegisteredUser>(ja[0].ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error while parsing returned content. Msg: " + ex.Message);
            }

            return ToReturn;
        }

        public async Task<Session[]> DownloadRecentSessionsAsync(int top = 5)
        {
            string cmd = CoreSqlExtensions.DownloadRecentSessions(top);
            string response = null;
            try
            {
                response = await ExecuteSqlAsync(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception("Failure while downloading recent sessions: " + ex.Message);
            }

            JArray ja = JArray.Parse(response);

            List<Session> ToReturn = new List<Session>();
            foreach (JObject jo in ja)
            {
                Session s = JsonConvert.DeserializeObject<Session>(jo.ToString());
                ToReturn.Add(s);
            }

            return ToReturn.ToArray();
        }


    }
}