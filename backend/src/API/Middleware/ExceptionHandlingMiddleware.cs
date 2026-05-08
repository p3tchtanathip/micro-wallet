using Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code;
        string message;

        switch (exception)
        {
            case BadRequestException ex:
                code = HttpStatusCode.BadRequest;   // 400
                message = ex.Message;
                break;

            case UnauthorizedAccessException ex:
                code = HttpStatusCode.Unauthorized; // 401
                message = ex.Message;
                break;

            case ForbiddenAccessException ex:
                code = HttpStatusCode.Forbidden;    // 403
                message = ex.Message;
                break;

            case NotFoundException ex: 
                code = HttpStatusCode.NotFound;     // 404
                message = ex.Message;
                break;

            case ConflictException ex: 
                code = HttpStatusCode.Conflict;     // 409
                message = ex.Message;
                break;

            default:
                code = HttpStatusCode.InternalServerError;
                message = "Internal Server Error";
                break;
        }

        logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var response = new { statusCode = (int)code, error = message };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}