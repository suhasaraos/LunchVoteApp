namespace LunchVoteApi.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException() 
        : base("Resource not found.")
    {
    }
    
    public NotFoundException(string message) 
        : base(message)
    {
    }
    
    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
