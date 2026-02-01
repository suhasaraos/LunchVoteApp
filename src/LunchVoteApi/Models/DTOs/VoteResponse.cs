namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Response model for vote submission.
/// </summary>
public class VoteResponse
{
    /// <summary>
    /// Success message.
    /// </summary>
    public string Message { get; set; } = "Vote recorded successfully.";
}
