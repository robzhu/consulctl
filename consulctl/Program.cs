using System;

namespace Consulctl
{
    class Program
    {
        static int Main( string[] args )
        {
            //Console.WriteLine("Attach debugger...");
            //Console.ReadLine();

            //args = new string[] { "--help" };
            //args = new string[] { "-h", "172.17.8.101", "-s", "service.json" };
            //args = new string[] { "-h", "172.17.8.101", "-d", "-s", "service.json" };
            //var argStr = "--read --key meow -h 172.17.8.101";
            //args = argStr.Split();

            var commandProcessor = new ConsulCommandLineTool();
            var result = commandProcessor.Process( args );

            if( result.ShowHelp )
            {
                Console.WriteLine( result.HelpText );
                return -1;
            }
            else if( result.ShowValue )
            {
                Console.WriteLine( result.Value );
                return 0;
            }
            else if( result.Success )
            {
                ConsoleEx.WriteLineSuccess( result.Message );
                return 0;
            }
            else
            {
                ConsoleEx.WriteLineError( result.Message );
                return -1;
            }
        }
    }
}
