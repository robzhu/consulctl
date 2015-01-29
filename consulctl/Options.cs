using CommandLine;
using CommandLine.Text;

namespace Consulctl
{
    public class Options
    {
        [Option( 'h', "host", DefaultValue="localhost", HelpText = "The machine name or IP address of the host running consul" )]
        public string Host { get; set; }

        [Option( 'p', "port", DefaultValue = 8500, HelpText = "The consul HTTP API port" )]
        public int Port { get; set; }



        [Option( 's', "svc", MutuallyExclusiveSet = "main", Required = false, HelpText = "Perform a service-related operation." )]
        public string ServiceArgument { get; set; }

        [Option( 'k', "key", MutuallyExclusiveSet = "main", Required = false, HelpText = "Perform a key-value-related operation." )]
        public string Key { get; set; }



        [Option( 'd', "delete", MutuallyExclusiveSet = "action", Required = false, HelpText = "Removes the specified key or service" )]
        public bool Delete { get; set; }

        [Option( 'c', "create", MutuallyExclusiveSet = "action", Required = false, HelpText = "Adds the specified key or service" )]
        public bool Create { get; set; }

        [Option( 'r', "read", MutuallyExclusiveSet = "action", Required = false, HelpText = "Reads the specified key or service" )]
        public bool Read { get; set; }



        [ValueOption( 0 )]
        public string Value { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo( "\nConsul command line tool", "v0.3" ),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true, 
            };
            help.AddPreOptionsLine( "http://www.consul.io/docs/agent/http.html" );
            help.AddOptions( this );
            help.AddPostOptionsLine( "  <service definition file path> | <k-v pair value file path>" );
            help.AddPostOptionsLine( "\n" );
            help.AddPostOptionsLine( "Register a service:                consulctl -c -s service.json" );
            help.AddPostOptionsLine( "Register a service on host/port:   consulctl -h consul.eze.com:8501 -p 9000 -c -s svc.json" );
            help.AddPostOptionsLine( "Read the nodes of a service:       consulctl -r -s myService" );
            help.AddPostOptionsLine( "Unregister a service:              consulctl -d -s service.json" );
            help.AddPostOptionsLine( "" );
            help.AddPostOptionsLine( "Create a key:                      consulctl -c -k motd hello" );
            help.AddPostOptionsLine( "Read a key:                        consulctl -r -k motd" );
            help.AddPostOptionsLine( "Delete a key:                      consulctl -d -k motd" );
            help.AddPostOptionsLine( "" );
            help.AddPostOptionsLine( "" );
            return help;
        }

        public string GetHostString()
        {
            return string.Format( "http://{0}:{1}", Host, Port );
        }

        public bool IsServiceOperation()
        {
            return ( !string.IsNullOrEmpty( ServiceArgument ) );
        }

        public bool IsKeyValueOperation()
        {
            return ( !string.IsNullOrEmpty( Key ) );
        }

        public bool IsMissingMainOption()
        {
            return ( !IsServiceOperation() && !IsKeyValueOperation() );
        }

        public bool MultipleMainOptions()
        {
            return ( IsServiceOperation() && IsKeyValueOperation() );
        }

        public bool IsMissingSubOption()
        {
            return !Create && !Read && !Delete;
        }

        public bool MultipleSubOptions()
        {
            int subOptions = 0;
            if( Read ) subOptions++;
            if( Create ) subOptions++;
            if( Delete ) subOptions++;

            return subOptions > 1;
        }
    }
}
