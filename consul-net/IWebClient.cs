using System;
using System.Threading.Tasks;

namespace Consul
{
    public interface IWebClient : IDisposable
    {
        Uri Host { get; }
        Task<bool> IsHostReachableAsync();

        Task<bool> RegisterAsync( ServiceDefinition service );
        Task<bool> UnregisterAsync( string serviceName );

        Task<bool> CreateKeyAsync( string key, string value );
        Task<string> ReadKeyAsync( string key );
        Task<bool> DeleteKeyAsync( string key );
    }
}
