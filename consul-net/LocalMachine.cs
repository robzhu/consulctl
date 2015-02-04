using System.Linq;
using System.Net;

namespace Consul
{
    public static class LocalMachine
    {
        public static IPAddress[] GetLocalMachineIPAddresses()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry( hostName );
            return hostEntry.AddressList;
        }

        public static bool IsLocalIP( string ip )
        {
            var ips = GetLocalMachineIPAddresses();
            return ips.Any( ipAddress => ipAddress.ToString() == ip );
        }
    }
}
