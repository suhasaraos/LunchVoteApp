namespace LunchVoteApi.Exceptions;

/// <summary>
/// Thrown when request validation fails.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException() 
        : base("Validation failed.")
    {
    }
    
    public ValidationException(string message) 
        : base(message)
    {
    }
    
    public ValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
