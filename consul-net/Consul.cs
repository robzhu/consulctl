using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace consul_net
{
    public sealed class ConsulClient : IDisposable
    {
        const string RegisterServicePath = "v1/agent/service/register";
        const string UnregisterServiceTemplate = "v1/agent/service/deregister/{0}";

        private HttpClient NetClient { get; set; }
        public Uri Host { get; private set; }

        public ConsulClient( string host )
        {
            Host = new Uri( host );
            NetClient = new HttpClient()
            {
                BaseAddress = Host
            };
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
