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
        const string UnregisterAgentServiceUrlTemplate = "v1/agent/service/deregister/{0}";
        const string CatalogDeregisterUrl = "v1/catalog/deregister";
        
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

        //HTTP GET localhost:8500/v1/catalog/services
        public async Task<OperationResult<ServiceDefinitionOutput[]>> ReadServicesInCatalogAsync( string dc )
        {
            var url = AllServicesUrl;
            if( !string.IsNullOrWhiteSpace( dc ) )
            {
                url += string.Format( "?dc={0}", dc );
            }
            return await ReadServicesInCatalogInternalAsync( url );
        }

        //HTTP GET localhost:8500/v1/catalog/services?wait=10m&index={index}
        public async Task<OperationResult<ServiceDefinitionOutput[]>> BlockingReadServicesInCatalogAsync( string dc, int latestIndex )
        {
            var url = AllServicesUrl;
            if( !string.IsNullOrWhiteSpace( dc ) )
            {
                url += string.Format( "?dc={0}&wait=10m&index={1}", dc, latestIndex );
            }
            else
            {
                url += string.Format( "?wait=10m&index={0}", latestIndex );
            }

            return await ReadServicesInCatalogInternalAsync( url );
        }

        private async Task<OperationResult<ServiceDefinitionOutput[]>> ReadServicesInCatalogInternalAsync( string url )
        {
            var response = await NetClient.GetAsync( url );
            var json = await response.Content.ReadAsStringAsync();

            //the keys are the names of the services, so we need to use a JObject here.
            JObject jobject = JObject.Parse( json );
            var serviceNames = jobject.Properties().Select( p => p.Name ).ToList();

            List<ServiceDefinitionOutput> servicesList = new List<ServiceDefinitionOutput>();
            foreach( var serviceName in serviceNames )
            {
                servicesList.AddRange( await ReadServicesByNameAsync( serviceName ) );
            }

            int index = response.Headers.GetValue<int>( "X-Consul-Index" );

            return new OperationResult<ServiceDefinitionOutput[]>( servicesList.ToArray() )
            {
                ConsulIndex = index,
            };
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
            ServiceDefinitionOutput existingService = await ReadServiceByIdAsync( serviceId );
            if( existingService == null )
            {
                return false;
            }

            //http://www.consul.io/docs/agent/http.html#_v1_agent_service_deregister_lt_serviceID_gt_
            //this is not a bug, documentation says unregister should be perfomed via HTTP GET

            if( LocalMachine.IsLocalIP( existingService.Address ) )
            {
                //service is local, deregister via agent
                var url = string.Format( UnregisterAgentServiceUrlTemplate, serviceId );
                var response = await NetClient.GetAsync( url );
                return response.IsSuccessStatusCode;
            }
            else
            {
                //service is not on the local machine, unregister it via the catalog endpoint
                var response = await NetClient.PutJsonContentAsync( CatalogDeregisterUrl, new
                    {
                        Datacenter = Settings.DC1,
                        Node = existingService.Node,
                        ServiceID = existingService.ServiceId,
                    } );
                return response.IsSuccessStatusCode;
            }
        }
        
        public async Task<ServiceDefinitionOutput> ReadServiceByIdAsync( string serviceId )
        {
            OperationResult<ServiceDefinitionOutput[]> readServicesResult = await ReadServicesInCatalogAsync( null );

            var allServices = readServicesResult.Value;
            var match = allServices.Where( s => s.ServiceId == serviceId ).FirstOrDefault();
            return match;
        }

        
        const string ReadCatalogServiceUrlTemplate = "v1/catalog/service/{0}";
        const string ReadAgentServiceUrlTemplate = "v1/agent/service/{0}";

        //HTTP GET localhost:8500/v1/catalog/service/{serviceName}
        public async Task<ServiceDefinitionOutput[]> ReadServicesByNameAsync( string serviceName )
        {
            var url = string.Format( ReadCatalogServiceUrlTemplate, serviceName );
            var response = await NetClient.GetAsync( url );
            if( response.IsSuccessStatusCode )
            {
                return await response.DeserializeJsonAsync<ServiceDefinitionOutput[]>();
            }
            else
            {
                //could not find a service at the catalog path, look again with the agent path:
                url = string.Format( ReadCatalogServiceUrlTemplate, serviceName );
                response = await NetClient.GetAsync( url );
                return await response.DeserializeJsonAsync<ServiceDefinitionOutput[]>();
            }
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

        public async Task<bool> DeleteNodeAsync( string node, string dataCenter )
        {
            var response = await NetClient.PutJsonContentAsync( CatalogDeregisterUrl, new
            {
                DataCenter = dataCenter,
                Node = node,
            } );
            return response.IsSuccessStatusCode;
        }
    }
}
