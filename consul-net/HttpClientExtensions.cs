using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Consul
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonContentAsync( this HttpClient client, string url, object content )
        {
            var request = new HttpRequestMessage( HttpMethod.Post, url );
            var serviceJson = JsonConvert.SerializeObject( content );
            request.Content = new StringContent( serviceJson );
            return await client.SendAsync( request );
        }
    }

    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> DeserializeJsonAsync<T>( this HttpResponseMessage response )
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>( json );
        }
    }
}
