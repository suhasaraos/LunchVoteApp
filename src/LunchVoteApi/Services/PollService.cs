using LunchVoteApi.Data;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunchVoteApi.Services;

/// <summary>
/// Service implementation for poll operations.
/// </summary>
public class PollService : IPollService
{
    private readonly LunchVoteDbContext _dbContext;
    private readonly ILogger<PollService> _logger;
    
    public PollService(LunchVoteDbContext dbContext, ILogger<PollService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Guid> CreatePollAsync(CreatePollRequest request)
    {
        // Validate options
        if (request.Options.Count < 2)
        {
            throw new Exceptions.ValidationException("At least 2 options are required.");
        }
        
        foreach (var option in request.Options)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                throw new Exceptions.ValidationException("Option text cannot be empty.");
            }
            if (option.Length > 100)
            {
                throw new Exceptions.ValidationException("Each option must not exceed 100 characters.");
            }
        }
        
        // Deactivate existing active poll for this group
        var existingActivePoll = await _dbContext.Polls
            .Where(p => p.GroupId == request.GroupId && p.IsActive)
            .FirstOrDefaultAsync();
        
        if (existingActivePoll != null)
        {
            existingActivePoll.IsActive = false;
            _logger.LogInformation("Deactivated existing poll {PollId} for group {GroupId}", 
                existingActivePoll.Id, request.GroupId);
        }
        
        // Create new poll
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            Question = request.Question,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Options = request.Options.Select(text => new Option
            {
                Id = Guid.NewGuid(),
                Text = text
            }).ToList()
        };
        
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Created new poll {PollId} for group {GroupId} with {OptionCount} options", 
            poll.Id, request.GroupId, poll.Options.Count);
        
        return poll.Id;
    }
    
    public async Task<ActivePollResponse?> GetActivePollAsync(string groupId)
    {
        var poll = await _dbContext.Polls
            .Include(p => p.Options)
            .Where(p => p.GroupId == groupId && p.IsActive)
            .FirstOrDefaultAsync();
        
        if (poll == null)
        {
            return null;
        }
        
        return new ActivePollResponse
        {
            PollId = poll.Id,
            GroupId = poll.GroupId,
            Question = poll.Question,
            Options = poll.Options.Select(o => new PollOptionDto
            {
                OptionId = o.Id,
                Text = o.Text
            }).ToList()
        };
    }
    
    public async Task<PollResultsResponse?> GetPollResultsAsync(Guid pollId)
    {
        var poll = await _dbContext.Polls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .Where(p => p.Id == pollId)
            .FirstOrDefaultAsync();
        
        if (poll == null)
        {
            return null;
        }
        
        // Calculate vote counts per option
        var voteCounts = poll.Votes
            .GroupBy(v => v.OptionId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = poll.Options.Select(o => new OptionResultDto
        {
            OptionId = o.Id,
            Text = o.Text,
            Count = voteCounts.GetValueOrDefault(o.Id, 0)
        }).ToList();
        
        return new PollResultsResponse
        {
            PollId = poll.Id,
            Question = poll.Question,
            Results = results,
            TotalVotes = poll.Votes.Count
        };
    }
    
    public async Task<List<string>> GetActiveGroupIdsAsync()
    {
        return await _dbContext.Polls
            .Where(p => p.IsActive)
            .Select(p => p.GroupId)
            .Distinct()
            .ToListAsync();
    }
}
