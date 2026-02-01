using LunchVoteApi.Data;
using LunchVoteApi.Exceptions;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Models.Entities;
using LunchVoteApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace LunchVoteApi.Tests.Services;

public class VoteServiceTests : IDisposable
{
    private readonly LunchVoteDbContext _dbContext;
    private readonly VoteService _voteService;
    private readonly Mock<ILogger<VoteService>> _loggerMock;

    public VoteServiceTests()
    {
        var options = new DbContextOptionsBuilder<LunchVoteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LunchVoteDbContext(options);
        _loggerMock = new Mock<ILogger<VoteService>>();
        _voteService = new VoteService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private async Task<(Guid PollId, Guid OptionId)> CreateTestPollAsync()
    {
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            GroupId = "platform",
            Question = "Where should we eat?",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Options = new List<Option>
            {
                new() { Id = Guid.NewGuid(), Text = "Sushi" },
                new() { Id = Guid.NewGuid(), Text = "Pizza" }
            }
        };

        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        return (poll.Id, poll.Options.First().Id);
    }

    [Fact]
    public async Task SubmitVoteAsync_WithValidData_CreatesVote()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync();
        var request = new VoteRequest
        {
            PollId = pollId,
            OptionId = optionId,
            VoterToken = "test-token-123"
        };

        // Act
        await _voteService.SubmitVoteAsync(request);

        // Assert
        var vote = await _dbContext.Votes.FirstOrDefaultAsync(v => v.VoterToken == "test-token-123");
        vote.Should().NotBeNull();
        vote!.PollId.Should().Be(pollId);
        vote.OptionId.Should().Be(optionId);
    }

    [Fact]
    public async Task SubmitVoteAsync_WithInvalidPollId_ThrowsValidationException()
    {
        // Arrange
        var request = new VoteRequest
        {
            PollId = Guid.NewGuid(),
            OptionId = Guid.NewGuid(),
            VoterToken = "test-token-123"
        };

        // Act
        Func<Task> act = async () => await _voteService.SubmitVoteAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>()
            .WithMessage("*Invalid poll ID*");
    }

    [Fact]
    public async Task SubmitVoteAsync_WithInvalidOptionId_ThrowsValidationException()
    {
        // Arrange
        var (pollId, _) = await CreateTestPollAsync();
        var request = new VoteRequest
        {
            PollId = pollId,
            OptionId = Guid.NewGuid(), // Invalid option
            VoterToken = "test-token-123"
        };

        // Act
        Func<Task> act = async () => await _voteService.SubmitVoteAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>()
            .WithMessage("*Invalid option ID*");
    }

    [Fact]
    public async Task SubmitVoteAsync_WithDuplicateVote_ThrowsDuplicateVoteException()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync();
        var request = new VoteRequest
        {
            PollId = pollId,
            OptionId = optionId,
            VoterToken = "test-token-123"
        };

        // First vote
        await _voteService.SubmitVoteAsync(request);

        // Act - Second vote with same token
        Func<Task> act = async () => await _voteService.SubmitVoteAsync(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateVoteException>();
    }

    [Fact]
    public async Task SubmitVoteAsync_WithInactivePoll_ThrowsValidationException()
    {
        // Arrange
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            GroupId = "platform",
            Question = "Inactive poll",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            Options = new List<Option>
            {
                new() { Id = Guid.NewGuid(), Text = "Option" }
            }
        };

        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        var request = new VoteRequest
        {
            PollId = poll.Id,
            OptionId = poll.Options.First().Id,
            VoterToken = "test-token-123"
        };

        // Act
        Func<Task> act = async () => await _voteService.SubmitVoteAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>()
            .WithMessage("*no longer active*");
    }

    [Fact]
    public async Task SubmitVoteAsync_MultipleDifferentVoters_AllVotesRecorded()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync();

        // Act
        for (int i = 0; i < 5; i++)
        {
            await _voteService.SubmitVoteAsync(new VoteRequest
            {
                PollId = pollId,
                OptionId = optionId,
                VoterToken = $"voter-{i}"
            });
        }

        // Assert
        var voteCount = await _dbContext.Votes.CountAsync(v => v.PollId == pollId);
        voteCount.Should().Be(5);
    }
}
