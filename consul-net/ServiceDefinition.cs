using Newtonsoft.Json;

namespace consul_net
{
    public class ServiceDefinition
    {
        public static ServiceDefinition CreateFromJson( string json )
        {
            return JsonConvert.DeserializeObject<ServiceDefinition>( json );
        }

        public string Name { get; set; }
        public int Port { get; set; }
        public string Node { get; set; }
        public string[] Tags { get; set; }
        //Health check
    }
}
