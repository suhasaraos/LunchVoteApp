using LunchVoteApi.Data;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace LunchVoteApi.Tests.Services;

public class PollServiceTests : IDisposable
{
    private readonly LunchVoteDbContext _dbContext;
    private readonly PollService _pollService;
    private readonly Mock<ILogger<PollService>> _loggerMock;

    public PollServiceTests()
    {
        var options = new DbContextOptionsBuilder<LunchVoteDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LunchVoteDbContext(options);
        _loggerMock = new Mock<ILogger<PollService>>();
        _pollService = new PollService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreatePollAsync_WithValidData_CreatesPollAndReturnsId()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Where should we eat today?",
            Options = new List<string> { "Sushi", "Burgers", "Thai", "Pizza" }
        };

        // Act
        var pollId = await _pollService.CreatePollAsync(request);

        // Assert
        pollId.Should().NotBeEmpty();
        
        var poll = await _dbContext.Polls
            .Include(p => p.Options)
            .FirstAsync(p => p.Id == pollId);
        
        poll.GroupId.Should().Be("platform");
        poll.Question.Should().Be("Where should we eat today?");
        poll.IsActive.Should().BeTrue();
        poll.Options.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreatePollAsync_DeactivatesExistingActivePoll()
    {
        // Arrange
        var firstRequest = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "First poll",
            Options = new List<string> { "Option A", "Option B" }
        };
        
        var firstPollId = await _pollService.CreatePollAsync(firstRequest);
        
        var secondRequest = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Second poll",
            Options = new List<string> { "Option X", "Option Y" }
        };

        // Act
        var secondPollId = await _pollService.CreatePollAsync(secondRequest);

        // Assert
        var firstPoll = await _dbContext.Polls.FindAsync(firstPollId);
        var secondPoll = await _dbContext.Polls.FindAsync(secondPollId);
        
        firstPoll!.IsActive.Should().BeFalse();
        secondPoll!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePollAsync_WithLessThanTwoOptions_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Invalid poll",
            Options = new List<string> { "Only one option" }
        };

        // Act
        Func<Task> act = async () => await _pollService.CreatePollAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>()
            .WithMessage("*2 options*");
    }

    [Fact]
    public async Task CreatePollAsync_WithEmptyOption_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Poll with empty option",
            Options = new List<string> { "Valid", "" }
        };

        // Act
        Func<Task> act = async () => await _pollService.CreatePollAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public async Task GetActivePollAsync_WithExistingActivePoll_ReturnsPoll()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Where should we eat?",
            Options = new List<string> { "Sushi", "Pizza" }
        };
        await _pollService.CreatePollAsync(request);

        // Act
        var result = await _pollService.GetActivePollAsync("platform");

        // Assert
        result.Should().NotBeNull();
        result!.GroupId.Should().Be("platform");
        result.Question.Should().Be("Where should we eat?");
        result.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivePollAsync_WithNoActivePoll_ReturnsNull()
    {
        // Act
        var result = await _pollService.GetActivePollAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPollResultsAsync_WithExistingPoll_ReturnsResults()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Where should we eat?",
            Options = new List<string> { "Sushi", "Pizza" }
        };
        var pollId = await _pollService.CreatePollAsync(request);

        // Act
        var result = await _pollService.GetPollResultsAsync(pollId);

        // Assert
        result.Should().NotBeNull();
        result!.PollId.Should().Be(pollId);
        result.Question.Should().Be("Where should we eat?");
        result.Results.Should().HaveCount(2);
        result.TotalVotes.Should().Be(0);
    }

    [Fact]
    public async Task GetPollResultsAsync_WithNonexistentPoll_ReturnsNull()
    {
        // Act
        var result = await _pollService.GetPollResultsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}
