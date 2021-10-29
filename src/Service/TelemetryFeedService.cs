using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
    }
}