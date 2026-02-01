using LunchVoteApi.Data;
using LunchVoteApi.Exceptions;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunchVoteApi.Services;

/// <summary>
/// Service implementation for vote operations.
/// </summary>
public class VoteService : IVoteService
{
    private readonly LunchVoteDbContext _dbContext;
    private readonly ILogger<VoteService> _logger;
    
    public VoteService(LunchVoteDbContext dbContext, ILogger<VoteService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task SubmitVoteAsync(VoteRequest request)
    {
        // Validate that the poll exists and is active
        var poll = await _dbContext.Polls
            .Include(p => p.Options)
            .Where(p => p.Id == request.PollId)
            .FirstOrDefaultAsync();
        
        if (poll == null)
        {
            throw new Exceptions.ValidationException("Invalid poll ID.");
        }
        
        if (!poll.IsActive)
        {
            throw new Exceptions.ValidationException("This poll is no longer active.");
        }
        
        // Validate that the option belongs to this poll
        var optionExists = poll.Options.Any(o => o.Id == request.OptionId);
        if (!optionExists)
        {
            throw new Exceptions.ValidationException("Invalid option ID for this poll.");
        }
        
        // Check if voter has already voted (explicit check before insert)
        var existingVote = await _dbContext.Votes
            .Where(v => v.PollId == request.PollId && v.VoterToken == request.VoterToken)
            .FirstOrDefaultAsync();
        
        if (existingVote != null)
        {
            throw new DuplicateVoteException();
        }
        
        // Create the vote
        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            PollId = request.PollId,
            OptionId = request.OptionId,
            VoterToken = request.VoterToken,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Votes.Add(vote);
        
        try
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Vote {VoteId} recorded for poll {PollId}, option {OptionId}", 
                vote.Id, request.PollId, request.OptionId);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // Handle race condition where another vote was submitted between check and insert
            throw new DuplicateVoteException();
        }
    }
    
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        var innerException = ex.InnerException;
        if (innerException != null)
        {
            var message = innerException.Message;
            return message.Contains("UNIQUE constraint") || 
                   message.Contains("duplicate key") ||
                   message.Contains("UQ_Vote_Poll_VoterToken");
        }
        return false;
    }
}
