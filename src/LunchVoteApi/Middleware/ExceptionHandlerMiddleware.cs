using System.Text.Json;
using LunchVoteApi.Exceptions;
using LunchVoteApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LunchVoteApi.Middleware;

/// <summary>
/// Global exception handler middleware that converts exceptions to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    
    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        var (statusCode, errorResponse) = exception switch
        {
            Exceptions.ValidationException ex => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse("ValidationError", ex.Message)
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                new ErrorResponse("NotFound", ex.Message)
            ),
            DuplicateVoteException ex => (
                StatusCodes.Status409Conflict,
                new ErrorResponse("AlreadyVoted", ex.Message)
            ),
            DbUpdateException ex when IsDuplicateKeyException(ex) => (
                StatusCodes.Status409Conflict,
                new ErrorResponse("AlreadyVoted", "This device has already voted in this poll.")
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("InternalError", "An unexpected error occurred.")
            )
        };
        
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
    
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        // Check for SQL Server unique constraint violation (error 2627 or 2601)
        var innerException = ex.InnerException;
        if (innerException != null)
        {
            var message = innerException.Message;
            return message.Contains("UNIQUE constraint") || 
                   message.Contains("duplicate key") ||
                   message.Contains("UQ_Vote_Poll_VoterToken");
        }
        return false;
    }
}
