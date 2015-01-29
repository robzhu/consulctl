using System;
using System.Threading.Tasks;

namespace Consul
{
    public interface IWebClient : IDisposable
    {
        Uri Host { get; }
        Task<bool> IsHostReachableAsync();

        Task<bool> RegisterServiceAsync( ServiceDefinition service );
        Task<ServiceDefinitionOutput[]> ReadServicesByNameAsync( string serviceName );
        Task<bool> UnregisterServiceAsync( string serviceId );

        Task<bool> CreateKeyAsync( string key, string value );
        Task<ValueEntry[]> ReadKeyAsync( string key );
        Task<string> ReadKeySimpleAsync( string key );
        Task<bool> DeleteKeyAsync( string key );
    }
}
