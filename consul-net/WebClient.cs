using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Consul
{
    public sealed class WebClient : IWebClient
    {
        const string RegisterServicePath = "v1/agent/service/register";
        const string UnregisterServiceTemplate = "v1/agent/service/deregister/{0}";
        const string KeyTemplate = "v1/kv/{0}";

        private HttpClient NetClient { get; set; }
        public Uri Host { get; private set; }

        public WebClient( string host )
        {
            Host = new Uri( host );
            NetClient = new HttpClient()
            {
                BaseAddress = Host
            };
        }

        public async Task<bool> IsHostReachableAsync()
        {
            try
            {
                var response = await NetClient.GetAsync( Host );
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync( ServiceDefinition service )
        {
            var request = new HttpRequestMessage( HttpMethod.Post, RegisterServicePath );
            var serviceJson = JsonConvert.SerializeObject( service );
            request.Content = new StringContent( serviceJson );
            var response = await NetClient.SendAsync( request );
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnregisterAsync( string serviceName )
        {
            var request = new HttpRequestMessage( HttpMethod.Get, string.Format( UnregisterServiceTemplate, serviceName ) );
            var response = await NetClient.SendAsync( request );
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateKeyAsync( string key, string value )
        {
            var request = new HttpRequestMessage( HttpMethod.Put, string.Format( KeyTemplate, key ) );
            request.Content = new StringContent( value );
            var response = await NetClient.SendAsync( request );
            return response.IsSuccessStatusCode;
        }

        public async Task<string> ReadKeyAsync( string key )
        {
            var request = new HttpRequestMessage( HttpMethod.Get, string.Format( KeyTemplate, key ) );
            var response = await NetClient.SendAsync( request );
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> DeleteKeyAsync( string key )
        {
            var request = new HttpRequestMessage( HttpMethod.Delete, string.Format( KeyTemplate, key ) );
            var response = await NetClient.SendAsync( request );
            return response.IsSuccessStatusCode;
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private bool _disposed = false;
        private void Dispose( bool disposing )
        {
            if( _disposed ) return;
            if( NetClient != null )
            {
                NetClient.Dispose();
            }
            _disposed = true;
        }
    }
}
