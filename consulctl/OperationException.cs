using System;

namespace Consulctl
{
    public class OperationException : Exception
    {
        public OperationResult Result { get; private set; }

        public OperationException( OperationResult error )
            : base( error.Message )
        {
            Result = error;
        }
    }
}
