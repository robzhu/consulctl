using System;

namespace Consulctl
{
    public static class ConsoleEx
    {
        public static void ClearAndWriteSuccess( string value )
        {
            Console.Clear();
            WriteLineSuccess( value );
        }

        public static void ClearAndWriteError( string value )
        {
            Console.Clear();
            WriteLineError( value );
        }

        public static void WriteLineSuccess( string value )
        {
            WriteLineInColor( value, ConsoleColor.Green );
        }

        public static void WriteLineError( string value )
        {
            WriteLineInColor( value, ConsoleColor.Red );
        }

        public static void WriteLineInColor( string value, ConsoleColor color )
        {
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine( value );

            Console.ForegroundColor = oldColor;
        }
    }
}
