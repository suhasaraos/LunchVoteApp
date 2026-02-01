namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Response model for poll creation.
/// </summary>
public class CreatePollResponse
{
    /// <summary>
    /// The ID of the newly created poll.
    /// </summary>
    public Guid PollId { get; set; }
}
