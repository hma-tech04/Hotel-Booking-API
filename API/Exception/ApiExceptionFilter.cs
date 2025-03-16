using API.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        ApiResponse<object> response;
        if (exception is CustomException customException){
            response = new ApiResponse<object>(
                (int)customException.Code,
                customException.Message
            );
            context.HttpContext.Response.StatusCode = (int)customException.Code;
        }else{
            response = new ApiResponse<object>(
                500,
                "Internal Server Error"
            );
            context.HttpContext.Response.StatusCode = 500;
        }
        context.Result = new JsonResult(response.GetResponse());
    }
}