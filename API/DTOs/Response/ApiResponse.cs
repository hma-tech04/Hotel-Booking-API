namespace API.DTOs.Response;
public class ApiResponse<T>
{   
    public int code { get; set; }
    public string message { get; set; }
    public T data { get; set; }
    public ApiResponse(int code, string message, T data)
    {
        this.code = code;
        this.message = message;
        this.data = data;
    }
}