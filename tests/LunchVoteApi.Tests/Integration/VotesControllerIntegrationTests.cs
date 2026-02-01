using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LunchVoteApi.Models;
using LunchVoteApi.Models.DTOs;
using Xunit;

namespace LunchVoteApi.Tests.Integration;

[Collection("Integration Tests")]
public class VotesControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public VotesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid PollId, Guid OptionId)> CreateTestPollAsync(string groupId)
    {
        var createRequest = new CreatePollRequest
        {
            GroupId = groupId[..Math.Min(groupId.Length, 50)], // Ensure groupId <= 50 chars
            Question = "Test question?",
            Options = new List<string> { "Option A", "Option B" }
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        createResponse.EnsureSuccessStatusCode(); // Ensure poll creation succeeded
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePollResponse>();
        
        // Get the poll results to retrieve the options
        var pollResponse = await _client.GetAsync($"/api/polls/{createResult!.PollId}/results");
        pollResponse.EnsureSuccessStatusCode(); // Ensure results retrieval succeeded
        var poll = await pollResponse.Content.ReadFromJsonAsync<PollResultsResponse>();
        
        if (poll?.Results == null || !poll.Results.Any())
        {
            throw new InvalidOperationException($"Poll {createResult.PollId} has no results/options");
        }
        
        return (createResult!.PollId, poll!.Results.First().OptionId);
    }

    [Fact]
    public async Task SubmitVote_WithValidData_Returns201Created()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync($"vote-valid-{Guid.NewGuid()}");
        var voteRequest = new VoteRequest
        {
            PollId = pollId,
            OptionId = optionId,
            VoterToken = $"integration-test-token-{Guid.NewGuid()}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<VoteResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("successfully");
    }

    [Fact]
    public async Task SubmitVote_DuplicateVote_Returns409Conflict()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync($"vote-dup-{Guid.NewGuid()}");
        var voteRequest = new VoteRequest
        {
            PollId = pollId,
            OptionId = optionId,
            VoterToken = $"duplicate-token-{Guid.NewGuid()}"
        };

        // First vote
        await _client.PostAsJsonAsync("/api/votes", voteRequest);

        // Act - Second vote with same token
        var response = await _client.PostAsJsonAsync("/api/votes", voteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Be("AlreadyVoted");
    }

    [Fact]
    public async Task SubmitVote_WithInvalidPollId_Returns400BadRequest()
    {
        // Arrange
        var voteRequest = new VoteRequest
        {
            PollId = Guid.NewGuid(),
            OptionId = Guid.NewGuid(),
            VoterToken = "test-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitVote_WithInvalidOptionId_Returns400BadRequest()
    {
        // Arrange
        var (pollId, _) = await CreateTestPollAsync($"vote-invalid-opt-{Guid.NewGuid()}");
        var voteRequest = new VoteRequest
        {
            PollId = pollId,
            OptionId = Guid.NewGuid(), // Invalid option
            VoterToken = "test-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitVote_VoteCountReflectedInResults()
    {
        // Arrange
        var (pollId, optionId) = await CreateTestPollAsync($"vote-count-{Guid.NewGuid()}");

        // Submit 3 votes
        for (int i = 0; i < 3; i++)
        {
            var voteRequest = new VoteRequest
            {
                PollId = pollId,
                OptionId = optionId,
                VoterToken = $"voter-{Guid.NewGuid()}"
            };
            await _client.PostAsJsonAsync("/api/votes", voteRequest);
        }

        // Act
        var response = await _client.GetAsync($"/api/polls/{pollId}/results");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var results = await response.Content.ReadFromJsonAsync<PollResultsResponse>();
        results.Should().NotBeNull();
        results!.TotalVotes.Should().Be(3);
        
        var votedOption = results.Results.First(r => r.OptionId == optionId);
        votedOption.Count.Should().Be(3);
    }
}
