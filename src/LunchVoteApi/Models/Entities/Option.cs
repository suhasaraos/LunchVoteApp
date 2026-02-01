namespace LunchVoteApi.Models.Entities;

/// <summary>
/// Represents a voting option within a poll.
/// </summary>
public class Option
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The poll this option belongs to.
    /// </summary>
    public Guid PollId { get; set; }
    
    /// <summary>
    /// The option text (e.g., "Sushi", "Pizza").
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to the parent poll.
    /// </summary>
    public Poll Poll { get; set; } = null!;
    
    /// <summary>
    /// Votes cast for this option.
    /// </summary>
    public List<Vote> Votes { get; set; } = new();
}
