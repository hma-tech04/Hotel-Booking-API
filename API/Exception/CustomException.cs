public class CustomException : Exception
{
    public ErrorCode Code { get; }

    public CustomException(ErrorCode Code, string message) 
        : base(message)
    {
        this.Code = Code;
    }
}
