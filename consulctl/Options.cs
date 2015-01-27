using CommandLine;
using CommandLine.Text;

namespace consulctl
{
    class Options
    {
        [Option( 'h', "host", DefaultValue="localhost", HelpText = "The machine name or IP address of the host running consul" )]
        public string Host { get; set; }

        [Option( 'p', "port", DefaultValue = 8500, HelpText = "The consul HTTP API port" )]
        public int Port { get; set; }

        [Option( 'u', "unregister", DefaultValue = false, HelpText = "Unregisters the service specified in the service definition path" )]
        public bool Unregister { get; set; }

        //[ValueOption( 's', "service", Required = true, HelpText = "Path to the service definition file." )]
        [ValueOption( 0 )]
        public string ServiceDefinitionFilePath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo( "Consul command line tool", "v0.1" ),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine( "http://www.consul.io/docs/agent/http.html" );
            help.AddPreOptionsLine( "\n" );
            help.AddPreOptionsLine( "Register a service:                consulctl 'svc.json'" );
            help.AddPreOptionsLine( "Unregister a service:              consulctl -u 'svc.json'" );
            help.AddPreOptionsLine( "Register a service on host/port:   consulctl -h consul.eze.com -p 9000 'svc.json'" );
            help.AddOptions( this );
            help.AddPostOptionsLine( "  <service definition file path>" );
            help.AddPostOptionsLine( "\n\n" );
            return help;
        }

        public string GetHostString()
        {
            return string.Format( "http://{0}:{1}", Host, Port );
        }
    }
}
