namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Response model for poll results.
/// </summary>
public class PollResultsResponse
{
    /// <summary>
    /// The poll ID.
    /// </summary>
    public Guid PollId { get; set; }
    
    /// <summary>
    /// The poll question.
    /// </summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// Results for each option.
    /// </summary>
    public List<OptionResultDto> Results { get; set; } = new();
    
    /// <summary>
    /// Total number of votes cast.
    /// </summary>
    public int TotalVotes { get; set; }
}

/// <summary>
/// Vote count for a single option.
/// </summary>
public class OptionResultDto
{
    /// <summary>
    /// The option ID.
    /// </summary>
    public Guid OptionId { get; set; }
    
    /// <summary>
    /// The option text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of votes for this option.
    /// </summary>
    public int Count { get; set; }
}
