using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Universal.Common.Net.Http;
using TelebotNew.Models.Conversation;
//using TelebotNew.Models.Server;
//using TelebotNew.Models.Database;

using UriBuilder = Universal.Common.UriBuilder;
using Microsoft.AspNetCore.Mvc;

namespace TelebotNew
{
    public class AppServiceClient : DeserializingHttpServiceClient
    {
        public static readonly HttpClient client = new HttpClient();

        private string mHost;

        public AppServiceClient(string host)
        {
            mHost = host;
        }

        private UriBuilder Base()
        {
            return new UriBuilder($"https://{mHost}/api");
        }

        protected override async Task<T> DeserializeAsync<T>(HttpContent httpContent, CancellationToken cancellationToken)
        {
            return JsonConvert.DeserializeObject<T>(await httpContent.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
