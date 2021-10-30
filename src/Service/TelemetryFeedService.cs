using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TimHanewich.TelemetryFeed.Sql;
using System.Net.Http.Headers;

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
    
    
    
    
    }
}