using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Unidevel.Extensions.Networking
{
    public class RestClient: IRestClient
    {
        private readonly ILogger<RestClient> _logger;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions();

        public Uri BaseUri { get; set; }

        public RestClient(ILogger<RestClient> logger = null)
        {
            _logger = logger;
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public RestClient(Uri baseUri, ILogger<RestClient> logger = null) : this(logger)
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
            BaseUri = baseUri;
        }

        public RestClient(string baseUrl, ILogger<RestClient> logger = null) : this(new Uri(baseUrl), logger)
        {
        }

        protected virtual Task BeforeWebClientRequest(string url, string request, string method, WebClient webClient) => Task.CompletedTask;
        protected virtual Task AfterWebClientRequest(string url, string request, string method, WebClient webClient) => Task.CompletedTask;


        public async Task<string> QueryAsync(string url, string method = RestMethod.GET)
        {
            return await QueryAsync(url, null, method);
        }

        public async Task<Rs> QueryAsync<Rs>(string url, string method = RestMethod.GET) where Rs : class
        {
            return await QueryAsync<object, Rs>(url, (object)null, method);
        }

        public async Task<Rs> QueryAsync<Rq, Rs>(string url, Rq request, string method = RestMethod.POST) where Rq : class where Rs : class
        {
            var requestMessage = await serializeRequest(request);
            var responseMessage = await QueryAsync(url, requestMessage, method);

            var result = await deserializeResponse<Rs>(responseMessage);
            return result;
        }

        public async Task<string> QueryAsync(string url, string request, string method = RestMethod.POST)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (String.IsNullOrWhiteSpace(method)) throw new ArgumentException("Must not be empty or whitespace.", nameof(method));
            if ((request == null) && (method != "GET")) throw new InvalidOperationException("Empty request is allowed only for 'GET' method.");

            var callUrl = buildUrl(url);

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                await BeforeWebClientRequest(callUrl, request, method, webClient);

                string result;

                _logger?.LogInformation($"Http '{method}' on {callUrl} started.");

                if (request == null)
                    result = await webClient.DownloadStringTaskAsync(callUrl);
                else
                    result = await webClient.UploadStringTaskAsync(callUrl, method, request);
                
                _logger?.LogInformation($"Http '{method}' on {callUrl} completed.");

                await AfterWebClientRequest(callUrl, request, method, webClient);

                return result;
            }
        }

        private string buildUrl(string url)
        {
            if (BaseUri == null)
            {
                if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException("Must not be null, empty or whitespace when no BaseUri provided.");

                return url;
            }
            else
            {
                var baseUrl = BaseUri.AbsoluteUri;
                if (String.IsNullOrWhiteSpace(url)) return baseUrl;

                if (baseUrl.EndsWith("/"))
                    return $"{baseUrl}{url}";
                else
                    return $"{baseUrl}/{url}";
            }
        }

        public async Task SubmitAsync(string url, string method = RestMethod.GET)
        {
            await SubmitAsync(url, null, method);
        }

        public async Task SubmitAsync<Rq>(string url, Rq request, string method = RestMethod.POST) where Rq : class => await SubmitAsync(url, await serializeRequest(request), method);

        public async Task SubmitAsync(string url, string request, string method = RestMethod.POST) => await QueryAsync(url, request, method);

        private Task<string> serializeRequest<Rq>(Rq request) where Rq : class => Task.FromResult(request != null ? JsonSerializer.Serialize(request, options) : null);

        private Task<Rs> deserializeResponse<Rs>(string responseMessage) where Rs : class
        {
            return Task.FromResult(responseMessage != null ? JsonSerializer.Deserialize<Rs>(responseMessage, options) : null);
        }
    }
}
