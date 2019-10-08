using System;
using System.Threading.Tasks;

namespace Unidevel.Extensions.Networking
{
    public interface IRestClient
    {
        Task<string> QueryAsync(Uri uri, string request, string method = "POST", object headers = null, string authorization = null);
        Task<Rs> QueryAsync<Rq, Rs>(Uri uri, Rq request, string method = "POST", object headers = null, string authorization = null);
        Task SubmitAsync(Uri uri, string request, string method = "POST", object headers = null, string authorization = null);
        Task SubmitAsync<Rq>(Uri uri, Rq request, string method = "POST", object headers = null, string authorization = null);
    }
}