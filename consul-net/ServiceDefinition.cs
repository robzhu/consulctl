using System.Diagnostics;
using Newtonsoft.Json;

namespace Consul
{
    public class ServiceDefinition
    {
        public static ServiceDefinition CreateFromJson( string json )
        {
            return JsonConvert.DeserializeObject<ServiceDefinition>( json );
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string Node { get; set; }
        public string[] Tags { get; set; }
        public HealthCheck Check { get; set; }
    }

    public class ServiceDefinitionOutput
    {
        public string Node { get; set; }
        public string Address { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int ServicePort { get; set; }

        [DebuggerDisplay( "Debug: {Items[index]}" )]
        public string[] ServiceTags { get; set; }
    }
}
