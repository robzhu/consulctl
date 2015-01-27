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
            }
            else if( result.ShowValue )
            {
                Console.WriteLine( result.Value );
            }
            else if( result.Success )
            {
                ConsoleEx.WriteLineSuccess( result.Message );
            }
            else
            {
                ConsoleEx.WriteLineError( result.Message );
            }
            return (int)result.Code;
        }
    }
}
