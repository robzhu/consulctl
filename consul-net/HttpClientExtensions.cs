using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Consul
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostJsonContentAsync( this HttpClient client, string url, object content )
        {
            var request = new HttpRequestMessage( HttpMethod.Post, url );
            var json = JsonConvert.SerializeObject( content );
            request.Content = new StringContent( json, Encoding.UTF8, "application/json" );
            return await client.SendAsync( request );
        }

        public static async Task<HttpResponseMessage> PutJsonContentAsync( this HttpClient client, string url, object content )
        {
            var request = new HttpRequestMessage( HttpMethod.Put, url );
            var json = JsonConvert.SerializeObject( content );
            request.Content = new StringContent( json, Encoding.UTF8, "application/json" );
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
