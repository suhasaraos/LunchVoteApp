namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Response model for retrieving an active poll.
/// </summary>
public class ActivePollResponse
{
    /// <summary>
    /// The poll ID.
    /// </summary>
    public Guid PollId { get; set; }
    
    /// <summary>
    /// The group ID this poll belongs to.
    /// </summary>
    public string GroupId { get; set; } = string.Empty;
    
    /// <summary>
    /// The poll question.
    /// </summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// The available voting options.
    /// </summary>
    public List<PollOptionDto> Options { get; set; } = new();
}

/// <summary>
/// A voting option within a poll.
/// </summary>
public class PollOptionDto
{
    /// <summary>
    /// The option ID.
    /// </summary>
    public Guid OptionId { get; set; }
    
    /// <summary>
    /// The option text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
