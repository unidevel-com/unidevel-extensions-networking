using System;
using System.Threading.Tasks;

namespace Unidevel.Extensions.Networking
{
    public interface IRestClient
    {
        Uri BaseUri { get; set; }

        Task<string> QueryAsync(string url, string request, string method = "POST");
        Task<Rs> QueryAsync<Rq, Rs>(string url, Rq request, string method = "POST") where Rq : class where Rs : class;
        Task<string> QueryAsync(string url, string method = "GET");
        Task<Rs> QueryAsync<Rs>(string url, string method = "GET") where Rs : class;

        Task SubmitAsync(string url, string method = "GET");
        Task SubmitAsync(string url, string request, string method = "POST");
        Task SubmitAsync<Rq>(string url, Rq request, string method = "POST") where Rq : class;
    }
}