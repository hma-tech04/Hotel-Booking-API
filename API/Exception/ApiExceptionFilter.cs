using API.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        ApiResponse<object> response;

        if (exception is CustomException customException)
        {
            response = new ApiResponse<object>(
                (int)customException.Code,
                customException.Message
            );
            context.HttpContext.Response.StatusCode = (int)customException.Code;
        }
        else
        {
            response = new ApiResponse<object>(
                (int)HttpStatusCode.InternalServerError,
                "A system error has occurred. Please try again later.",
                new { error = exception.Message}
            );
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        context.Result = new JsonResult(response.GetResponse());
        context.ExceptionHandled = true; 
    }
}
