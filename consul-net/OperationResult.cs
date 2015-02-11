
namespace Consul
{
    public class OperationResult
    {
        public static OperationResult SuccessResult { get; private set; }
        public static OperationResult UnexpectedError
        {
            get
            {
                var result = new OperationResult();
                OperationErrors.Init( result, OperationErrorCodes.UnexpectedError );
                return result;
            }
        }

        public static OperationResult Create( OperationErrorCodes code )
        {
            var opResult = new OperationResult();
            OperationErrors.Init( opResult, code );
            return opResult;
        }

        static OperationResult()
        {
            var result = new OperationResult();
            OperationErrors.Init( result, OperationErrorCodes.Success );
            SuccessResult = result;
        }

        public int ConsulIndex { get; set; }
        public string Message { get; set; }
        public OperationErrorCodes Code { get; set; }
        public bool Success { get { return Code == OperationErrorCodes.Success; } }
    }

    public class OperationResult<T> : OperationResult
    {
        public static OperationResult<T> UnexpectedError
        {
            get
            {
                var result = new OperationResult<T>();
                OperationErrors.Init( result, OperationErrorCodes.UnexpectedError );
                return result;
            }
        }

        public static OperationResult<T> Create( OperationErrorCodes code )
        {
            var opResult = new OperationResult<T>();
            OperationErrors.Init( opResult, code );
            return opResult;
        }

        public T Value { get; set; }

        public OperationResult() : this( default( T ) ) { }
        public OperationResult( T value )
        {
            OperationErrors.Init( this, OperationErrorCodes.Success );
            Value = value;
        }
    }
}
