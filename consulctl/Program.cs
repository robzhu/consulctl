using System;

namespace Consulctl
{
    class Program
    {
        static int Main( string[] args )
        {
            bool test = false;
            if( test )
            {
                var argStr = "-r -k motd";
                args = argStr.Split();
            }

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
