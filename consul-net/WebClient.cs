using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Consul
{
    public sealed class WebClient : IWebClient
    {
        const string AllServicesUrl = "v1/catalog/services";
        const string RegisterServiceUrl = "v1/agent/service/register";
        const string UnregisterServiceUrlTemplate = "v1/agent/service/deregister/{0}";
        const string ReadServiceUrlTemplate = "v1/catalog/service/{0}";
        const string KeyValueUrlTemplate = "v1/kv/{0}";

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


        //HTTP POST localhost:8500/v1/agent/service/register 
        //{
        //  "id": "redis1",
        //  "name": "redis",
        //  "port": 8000,
        //  "tags": [ "master", "v1" ],
        //  "check": {
        //    "script": "/usr/local/bin/check_redis.py",
        //    "interval": "10s"
        //  }
        //}
        public async Task<bool> RegisterServiceAsync( ServiceDefinition service )
        {
            var response = await NetClient.PostJsonContentAsync( RegisterServiceUrl, service );
            return response.IsSuccessStatusCode;
        }

        //HTTP GET localhost:8500/v1/agent/service/deregister/{serviceName}
        public async Task<bool> UnregisterServiceAsync( string serviceId )
        {
            var existingService = await ReadServiceByIdAsync( serviceId );
            if( existingService == null )
            {
                return false;
            }

            //http://www.consul.io/docs/agent/http.html#_v1_agent_service_deregister_lt_serviceID_gt_
            //this is not a bug, documentation says unregister should be perfomed via HTTP GET
            var url = string.Format( UnregisterServiceUrlTemplate, serviceId );
            var response = await NetClient.GetAsync( url );
            return response.IsSuccessStatusCode;
        }
        
        public async Task<ServiceDefinitionOutput> ReadServiceByIdAsync( string serviceId )
        {
            ServiceDefinitionOutput[] allServices = await ReadAllServicesAsync();
            var match = allServices.Where( s => s.ServiceId == serviceId ).FirstOrDefault();
            return match;
        }

        //HTTP GET localhost:8500/v1/catalog/services
        public async Task<ServiceDefinitionOutput[]> ReadAllServicesAsync()
        {
            var response = await NetClient.GetAsync( AllServicesUrl );
            var json = await response.Content.ReadAsStringAsync();

            //the keys are the names of the services, so we need to use a JObject here.
            JObject jobject = JObject.Parse( json );
            var serviceNames = jobject.Properties().Select( p => p.Name ).ToList();

            List<ServiceDefinitionOutput> servicesList = new List<ServiceDefinitionOutput>();
            foreach( var serviceName in serviceNames )
            {
                servicesList.AddRange( await ReadServicesByNameAsync( serviceName ) );
            }

            return servicesList.ToArray();
        }

        //HTTP GET localhost:8500/v1/catalog/service/{serviceName}
        public async Task<ServiceDefinitionOutput[]> ReadServicesByNameAsync( string serviceName )
        {
            var url = string.Format( ReadServiceUrlTemplate, serviceName );
            var response = await NetClient.GetAsync( url );
            return await response.DeserializeJsonAsync<ServiceDefinitionOutput[]>();
        }

        public async Task<bool> CreateKeyAsync( string key, string value )
        {
            var request = new HttpRequestMessage( HttpMethod.Put, string.Format( KeyValueUrlTemplate, key ) );
            request.Content = new StringContent( value );
            var response = await NetClient.SendAsync( request );
            return response.IsSuccessStatusCode;
        }

        public async Task<ValueEntry[]> ReadKeyAsync( string key )
        {
            var url = string.Format( KeyValueUrlTemplate, key );
            var response = await NetClient.GetAsync( url );
            return await response.DeserializeJsonAsync<ValueEntry[]>();
        }

        public async Task<string> ReadKeySimpleAsync( string key )
        {
            ValueEntry[] values = await ReadKeyAsync( key );
            if( values == null ) return null;
            return values[ 0 ].GetDecodedValue();
        }

        public async Task<bool> DeleteKeyAsync( string key )
        {
            var request = new HttpRequestMessage( HttpMethod.Delete, string.Format( KeyValueUrlTemplate, key ) );
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
