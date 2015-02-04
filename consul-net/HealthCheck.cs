
namespace Consul
{
    //Exit code 0 - Check is passing
    //Exit code 1 - Check is warning
    //Any other code - Check is failing
    public class HealthCheck
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Script { get; set; }
        public string Interval { get; set; }
        public string Notes { get; set; }
        public string TTL { get; set; }
    }
}
