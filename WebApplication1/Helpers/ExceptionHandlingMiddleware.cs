using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unexpected error occurred.");

        var response = exception switch
        {
            ApplicationException => new ProblemDetails { Status = (int)HttpStatusCode.BadRequest, Title = "Application exception occurred." },
            KeyNotFoundException => new ProblemDetails { Status = (int)HttpStatusCode.NotFound, Title = "The request endpoint not found." },
            UnauthorizedAccessException => new ProblemDetails { Status = (int)HttpStatusCode.Unauthorized, Title = "Unauthorized." },
            _ => new ProblemDetails { Status = (int)HttpStatusCode.InternalServerError, Title = "Internal server error. Please retry later." }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.Status;
        await context.Response.WriteAsJsonAsync(response);
    }
}