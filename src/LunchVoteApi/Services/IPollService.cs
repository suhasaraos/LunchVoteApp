using LunchVoteApi.Models.DTOs;

namespace LunchVoteApi.Services;

/// <summary>
/// Service interface for poll operations.
/// </summary>
public interface IPollService
{
    /// <summary>
    /// Creates a new poll for a group. Deactivates any existing active poll for the group.
    /// </summary>
    /// <param name="request">The poll creation request.</param>
    /// <returns>The ID of the newly created poll.</returns>
    Task<Guid> CreatePollAsync(CreatePollRequest request);
    
    /// <summary>
    /// Gets the currently active poll for a group.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <returns>The active poll, or null if none exists.</returns>
    Task<ActivePollResponse?> GetActivePollAsync(string groupId);
    
    /// <summary>
    /// Gets the results for a poll.
    /// </summary>
    /// <param name="pollId">The poll identifier.</param>
    /// <returns>The poll results, or null if poll not found.</returns>
    Task<PollResultsResponse?> GetPollResultsAsync(Guid pollId);
    
    /// <summary>
    /// Gets all group IDs that have active polls.
    /// </summary>
    /// <returns>A list of group IDs with active polls.</returns>
    Task<List<string>> GetActiveGroupIdsAsync();
}
