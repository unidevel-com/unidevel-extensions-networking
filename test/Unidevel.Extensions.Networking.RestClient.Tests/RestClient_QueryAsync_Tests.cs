using System;
using System.Threading.Tasks;
using Xunit;

namespace Unidevel.Extensions.Networking.Tests
{
    public class RestClient_QueryAsync_Tests
    {
        [Fact]
        public async Task Query_String_Get()
        {
            IRestClient restClient = new RestClient();
            var result = await restClient.QueryAsync("http://www.google.com");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Query_UserDto_GetSingle()
        {
            IRestClient restClient = new RestClient("https://jsonplaceholder.typicode.com");
            var result = await restClient.QueryAsync<UserDto>("posts/1");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task Query_UserDto_GetMultiple()
        {
            IRestClient restClient = new RestClient();
            var result = await restClient.QueryAsync<UserDto[]>("https://jsonplaceholder.typicode.com/posts");

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task Submit_UserDto_PostSingle()
        {
            IRestClient restClient = new RestClient();
            var userDto = new UserDto { Id = 5, Body = "Bobody", UserId = 6, Title = "Tititle" };
            
            await restClient.SubmitAsync<UserDto>("https://jsonplaceholder.typicode.com/posts", userDto);            
        }
    }
}
