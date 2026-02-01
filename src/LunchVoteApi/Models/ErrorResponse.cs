namespace LunchVoteApi.Models;

/// <summary>
/// Standard error response model for API errors.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code (e.g., "ValidationError", "NotFound", "AlreadyVoted").
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable error description.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    public ErrorResponse() { }
    
    public ErrorResponse(string error, string message)
    {
        Error = error;
        Message = message;
    }
}
