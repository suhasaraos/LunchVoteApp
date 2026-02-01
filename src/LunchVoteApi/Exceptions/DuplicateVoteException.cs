namespace LunchVoteApi.Exceptions;

/// <summary>
/// Thrown when a voter attempts to vote more than once in a poll.
/// </summary>
public class DuplicateVoteException : Exception
{
    public DuplicateVoteException() 
        : base("This device has already voted in this poll.")
    {
    }
    
    public DuplicateVoteException(string message) 
        : base(message)
    {
    }
    
    public DuplicateVoteException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
