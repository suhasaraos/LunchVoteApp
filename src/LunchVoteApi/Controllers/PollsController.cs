using LunchVoteApi.Exceptions;
using LunchVoteApi.Models;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LunchVoteApi.Controllers;

/// <summary>
/// API controller for poll operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly ILogger<PollsController> _logger;
    
    public PollsController(IPollService pollService, ILogger<PollsController> logger)
    {
        _pollService = pollService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates a new poll for a group.
    /// </summary>
    /// <param name="request">The poll creation request.</param>
    /// <returns>The ID of the newly created poll.</returns>
    /// <response code="201">Poll created successfully.</response>
    /// <response code="400">Validation error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreatePollResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new ErrorResponse("ValidationError", string.Join(" ", errors)));
        }
        
        var pollId = await _pollService.CreatePollAsync(request);
        
        var response = new CreatePollResponse { PollId = pollId };
        
        return CreatedAtAction(
            nameof(GetPollResults),
            new { pollId = pollId },
            response
        );
    }
    
    /// <summary>
    /// Gets the active poll for a group.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <returns>The active poll.</returns>
    /// <response code="200">Active poll found.</response>
    /// <response code="404">No active poll for this group.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ActivePollResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivePoll([FromQuery] string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return BadRequest(new ErrorResponse("ValidationError", "GroupId is required."));
        }
        
        var poll = await _pollService.GetActivePollAsync(groupId);
        
        if (poll == null)
        {
            return NotFound(new ErrorResponse("NotFound", "No active poll found for this group."));
        }
        
        return Ok(poll);
    }
    
    /// <summary>
    /// Gets the results for a poll.
    /// </summary>
    /// <param name="pollId">The poll identifier.</param>
    /// <returns>The poll results.</returns>
    /// <response code="200">Poll results retrieved.</response>
    /// <response code="404">Poll not found.</response>
    [HttpGet("{pollId}/results")]
    [ProducesResponseType(typeof(PollResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPollResults(Guid pollId)
    {
        var results = await _pollService.GetPollResultsAsync(pollId);
        
        if (results == null)
        {
            return NotFound(new ErrorResponse("NotFound", "Poll not found."));
        }
        
        return Ok(results);
    }
}
