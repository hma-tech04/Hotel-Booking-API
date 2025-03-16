namespace API.DTOs.Response;
public class ApiResponse<T>
{   
    public int Code { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public ApiResponse(int Code, string Message, T? Data = default)
    {
        this.Code = Code;
        this.Message = Message;
        this.Data = Data;
    }

    public object GetResponse()
        {
            if (Data == null)
            {
                return new { Code, Message }; 
            }
            
            return new { Code, Message, Data };  
        }
}