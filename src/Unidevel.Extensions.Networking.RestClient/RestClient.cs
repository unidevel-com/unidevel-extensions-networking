using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Unidevel.Extensions.Networking
{
    public class RestClient : IRestClient
    {
        public async Task<Rs> QueryAsync<Rq, Rs>(Uri uri, Rq request, string method = "POST", object headers = null, string authorization = null)
        {
            var requestMessage = await serializeRequest(request);
            var responseMessage = await QueryAsync(uri, requestMessage, method, headers, authorization);

            return await deserializeResponse<Rs>(responseMessage);
        }

        public async Task<string> QueryAsync(Uri uri, string request, string method = "POST", object headers = null, string authorization = null)
        {
            if ((request == null) && (method != "GET")) throw new InvalidOperationException("Empty request is allowed only for 'GET' method.");

            using (var webClient = new WebClient())
            {
                if (headers != null)
                {
                    var properties = headers.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var property in properties)
                    {
                        webClient.Headers.Add(property.Name, property.GetValue(headers).ToString());
                    }
                }

                // override only if not provided

                if (webClient.Headers[HttpRequestHeader.ContentType] == null)
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                // override always

                if (authorization != null)
                    webClient.Headers[HttpRequestHeader.Authorization] = authorization;

                if (request == null)
                    return await webClient.DownloadStringTaskAsync(uri);
                else
                    return await webClient.UploadStringTaskAsync(uri, method, request);
            }
        }

        public async Task SubmitAsync<Rq>(Uri uri, Rq request, string method = "POST", object headers = null, string authorization = null) => await SubmitAsync(uri, await serializeRequest(request), method, headers, authorization);

        public async Task SubmitAsync(Uri uri, string request, string method = "POST", object headers = null, string authorization = null) => await QueryAsync(uri, request, method, headers, authorization);

        private Task<string> serializeRequest<Rq>(Rq request) => Task.FromResult(JsonConvert.SerializeObject(request));

        private Task<Rs> deserializeResponse<Rs>(string responseMessage) => Task.FromResult(JsonConvert.DeserializeObject<Rs>(responseMessage));
    }
}
