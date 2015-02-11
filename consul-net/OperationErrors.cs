using System.Collections.Generic;

namespace Consul
{
    public enum OperationErrorCodes
    {
        Success,
        UnexpectedError,
    }

    public static class OperationErrors
    {
        private static Dictionary<OperationErrorCodes, string> _errorStrings;

        static OperationErrors()
        {
            _errorStrings = new Dictionary<OperationErrorCodes, string>();
            _errorStrings[ OperationErrorCodes.UnexpectedError ] = "Any unexpected error has occured.";
            _errorStrings[ OperationErrorCodes.Success ] = "Operation completed successfully.";
        }

        public static void Init( OperationResult opResult, OperationErrorCodes code )
        {
            opResult.Code = code;

            if( _errorStrings.ContainsKey( code ) )
            {
                opResult.Message = _errorStrings[ code ];
            }
            else
            {
                opResult.Message = string.Format( "TODO, ADD ERROR MESSAGE FOR {0}", code.ToString() );
            }
        }
    }
}
