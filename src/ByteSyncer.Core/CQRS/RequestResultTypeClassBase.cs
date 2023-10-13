using CommunityToolkit.Diagnostics;

namespace ByteSyncer.Core.CQRS
{
    public record RequestResultTypeClassBase<TResultType, TResult>(TResultType ResultType, TResult? Result, Exception? Exception)
        where TResultType : Enum
        where TResult : class
    {
        public TResult GetResult()
        {
            Guard.IsNotNull(Result);

            return Result;
        }

        public TResult? GetResultOrDefault()
        {
            return Result;
        }

        public Exception GetException()
        {
            Guard.IsNotNull(Exception);

            return Exception;
        }

        public Exception? GetExceptionOrDefault()
        {
            return Exception;
        }
    }

    public record RequestResultTypeStructBase<TResultType, TResult>(TResultType ResultType, TResult? Result, Exception? Exception)
        where TResultType : Enum
        where TResult : struct
    {
        public TResult GetResult()
        {
            Guard.IsTrue(Result.HasValue);

            return Result.Value;
        }

        public TResult? GetResultOrDefault()
        {
            return Result;
        }

        public Exception GetException()
        {
            Guard.IsNotNull(Exception);

            return Exception;
        }

        public Exception? GetExceptionOrDefault()
        {
            return Exception;
        }
    }
}
