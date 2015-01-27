using System;
using System.IO;
using CommandLine;
using consul_net;
using Newtonsoft.Json;

namespace consulctl
{
    class Program
    {
        private static bool DisplayHelpIfEmpty( string[] args )
        {
            if( ( args == null ) || ( args.Length == 0 ) )
            {
                Console.WriteLine( new Options().GetUsage() );
                return true;
            }
            return false;
        }


        static void Main( string[] args )
        {
            //args = new[] { "service.json" };
            if( DisplayHelpIfEmpty( args ) ) return;

            var options = new Options();
            if( Parser.Default.ParseArguments( args, options ) )
            {
                Process( options );
            }
            else
            {
                Console.WriteLine( options.GetUsage() );
            }
        }

        static void Process( Options options )
        {
            var client = new ConsulClient( options.GetHostString() );
            var sr = new StreamReader( options.ServiceDefinitionFilePath );
            var serviceJson = sr.ReadToEnd();
            var serviceDef = ServiceDefinition.CreateFromJson( serviceJson );

            if( options.Unregister )
            {
                bool success = client.UnregisterAsync( serviceDef.Name ).Result;
                if( success )
                {
                    ConsoleEx.WriteLineSuccess( "successfully unregistered service." );
                }
                else
                {
                    ConsoleEx.WriteLineError( "could not unregister service." );
                }
            }
            else
            {
                bool success = client.RegisterAsync( serviceDef ).Result;
                if( success )
                {
                    ConsoleEx.WriteLineSuccess( "successfully registered service." );
                }
                else
                {
                    ConsoleEx.WriteLineError( "could not register service." );
                }
            }
        }
    }
}
