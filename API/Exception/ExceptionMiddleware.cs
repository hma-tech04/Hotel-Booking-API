using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using API.DTOs.Response;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }

        if (context.Response.StatusCode == (int)HttpStatusCode.BadRequest && context.Items["errors"] is ModelStateDictionary modelState)
        {
            await HandleValidationAsync(context, modelState);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        ErrorCode statusCode = (ErrorCode)HttpStatusCode.InternalServerError; 
        string message = ex.Message;

        if (ex is CustomException customEx)
        {
            statusCode = customEx.Code;
            message = customEx.Message;
        }

        //context.Response.StatusCode = statusCode;

        var response = new ApiResponse<string>(
            Code: statusCode,
            Message: message
        );

        var json = JsonSerializer.Serialize(response.GetResponse());
        await context.Response.WriteAsync(json);
    }

    private static async Task HandleValidationAsync(HttpContext context, ModelStateDictionary modelState)
    {
        context.Response.ContentType = "application/json";

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var errors = modelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        var response = new ApiResponse<object>(
            Code: ErrorCode.BadRequest,
            Message: "Validation failed",
            Data: errors
        );

        var json = JsonSerializer.Serialize(response.GetResponse());
        await context.Response.WriteAsync(json);
    }
}
