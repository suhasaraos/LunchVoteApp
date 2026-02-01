using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LunchVoteApi.Models;
using LunchVoteApi.Models.DTOs;
using Xunit;

namespace LunchVoteApi.Tests.Integration;

[Collection("Integration Tests")]
public class PollsControllerIntegrationTests
{
    private readonly HttpClient _client;

    public PollsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePoll_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = $"platform-{Guid.NewGuid()}",
            Question = "Where should we eat today?",
            Options = new List<string> { "Sushi", "Burgers", "Thai", "Pizza" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        result.Should().NotBeNull();
        result!.PollId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePoll_WithInsufficientOptions_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            GroupId = "platform",
            Question = "Test question?",
            Options = new List<string> { "Only one" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivePoll_WithExistingPoll_Returns200Ok()
    {
        // Arrange - Use a shorter group ID to stay within 50 char limit
        var groupId = $"active-{Guid.NewGuid():N}"; // active- (7) + 32 = 39 chars, well under 50
        var createRequest = new CreatePollRequest
        {
            GroupId = groupId,
            Question = "Test poll question?",
            Options = new List<string> { "Option A", "Option B" }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync($"/api/polls/active?groupId={groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ActivePollResponse>();
        result.Should().NotBeNull();
        result!.GroupId.Should().Be(groupId);
        result.Question.Should().Be("Test poll question?");
        result.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActivePoll_WithNonexistentGroup_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/polls/active?groupId=nonexistent-group-{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Error.Should().Be("NotFound");
    }

    [Fact]
    public async Task GetActivePoll_WithMissingGroupId_Returns400BadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/polls/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPollResults_WithExistingPoll_Returns200Ok()
    {
        // Arrange
        var createRequest = new CreatePollRequest
        {
            GroupId = $"test-results-{Guid.NewGuid()}",
            Question = "Test results question?",
            Options = new List<string> { "Option A", "Option B" }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePollResponse>();

        // Act
        var response = await _client.GetAsync($"/api/polls/{createResult!.PollId}/results");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PollResultsResponse>();
        result.Should().NotBeNull();
        result!.PollId.Should().Be(createResult.PollId);
        result.TotalVotes.Should().Be(0);
        result.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPollResults_WithNonexistentPoll_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/polls/{Guid.NewGuid()}/results");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
