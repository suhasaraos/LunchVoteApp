using LunchVoteApi.Models.Entities;

namespace LunchVoteApi.Data;

public static class MockDataSeeder
{
    public static void Seed(LunchVoteDbContext dbContext)
    {
        // Seed only if database is empty
        if (dbContext.Polls.Any())
        {
            return;
        }

        var pollId1 = Guid.NewGuid();
        var pollId2 = Guid.NewGuid();

        var option1Id = Guid.NewGuid();
        var option2Id = Guid.NewGuid();
        var option3Id = Guid.NewGuid();
        var option4Id = Guid.NewGuid();
        var option5Id = Guid.NewGuid();
        var option6Id = Guid.NewGuid();

        // Active poll for "platform" group
        var platformPoll = new Poll
        {
            Id = pollId1,
            GroupId = "platform",
            Question = "Where should the Platform team eat today?",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Options = new List<Option>
            {
                new() { Id = option1Id, PollId = pollId1, Text = "Sushi Palace" },
                new() { Id = option2Id, PollId = pollId1, Text = "Burger Joint" },
                new() { Id = option3Id, PollId = pollId1, Text = "Thai Garden" },
                new() { Id = option4Id, PollId = pollId1, Text = "Pizza Heaven" }
            }
        };

        // Active poll for "security" group
        var securityPoll = new Poll
        {
            Id = pollId2,
            GroupId = "security",
            Question = "Security team lunch preference?",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Options = new List<Option>
            {
                new() { Id = option5Id, PollId = pollId2, Text = "Mexican Grill" },
                new() { Id = option6Id, PollId = pollId2, Text = "Indian Curry House" }
            }
        };

        dbContext.Polls.AddRange(platformPoll, securityPoll);

        // Add some mock votes for the platform poll
        var votes = new List<Vote>
        {
            new() { Id = Guid.NewGuid(), PollId = pollId1, OptionId = option1Id, VoterToken = "mock-voter-1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), PollId = pollId1, OptionId = option1Id, VoterToken = "mock-voter-2", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), PollId = pollId1, OptionId = option2Id, VoterToken = "mock-voter-3", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), PollId = pollId1, OptionId = option3Id, VoterToken = "mock-voter-4", CreatedAt = DateTime.UtcNow }
        };

        dbContext.Votes.AddRange(votes);
        dbContext.SaveChanges();
    }
}
