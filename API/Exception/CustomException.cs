public class CustomException : Exception
{
    public ErrorCode Code { get; }

    public CustomException(ErrorCode code, string message) 
        : base(message)
    {
        Code = code;
    }

    public CustomException(ErrorCode code, string message, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
    }
}
