namespace LunchVoteApi.Models.Entities;

/// <summary>
/// Represents a lunch voting poll for a specific group.
/// </summary>
public class Poll
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The team/group identifier (e.g., "platform", "security", "apps").
    /// </summary>
    public string GroupId { get; set; } = string.Empty;
    
    /// <summary>
    /// The poll question (e.g., "Where should we eat today?").
    /// </summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this poll is currently active. Only one poll per group should be active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// When the poll was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// The voting options for this poll.
    /// </summary>
    public List<Option> Options { get; set; } = new();
    
    /// <summary>
    /// The votes cast for this poll.
    /// </summary>
    public List<Vote> Votes { get; set; } = new();
}
