namespace LunchVoteApi.Models.Entities;

/// <summary>
/// Represents a vote cast by a participant.
/// </summary>
public class Vote
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The poll this vote was cast in.
    /// </summary>
    public Guid PollId { get; set; }
    
    /// <summary>
    /// The option that was selected.
    /// </summary>
    public Guid OptionId { get; set; }
    
    /// <summary>
    /// Anonymous token identifying the voter's device/browser.
    /// Used to prevent duplicate votes without requiring authentication.
    /// </summary>
    public string VoterToken { get; set; } = string.Empty;
    
    /// <summary>
    /// When the vote was cast (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to the parent poll.
    /// </summary>
    public Poll Poll { get; set; } = null!;
    
    /// <summary>
    /// Navigation property to the selected option.
    /// </summary>
    public Option Option { get; set; } = null!;
}
