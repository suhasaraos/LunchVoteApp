using LunchVoteApi.Models.DTOs;

namespace LunchVoteApi.Services;

/// <summary>
/// Service interface for vote operations.
/// </summary>
public interface IVoteService
{
    /// <summary>
    /// Submits a vote for a poll.
    /// </summary>
    /// <param name="request">The vote request.</param>
    /// <exception cref="LunchVoteApi.Exceptions.ValidationException">
    /// Thrown when the option doesn't belong to the poll.
    /// </exception>
    /// <exception cref="LunchVoteApi.Exceptions.DuplicateVoteException">
    /// Thrown when the voter has already voted in this poll.
    /// </exception>
    Task SubmitVoteAsync(VoteRequest request);
}
