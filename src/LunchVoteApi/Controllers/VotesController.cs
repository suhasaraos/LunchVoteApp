using LunchVoteApi.Models;
using LunchVoteApi.Models.DTOs;
using LunchVoteApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LunchVoteApi.Controllers;

/// <summary>
/// API controller for vote operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VotesController : ControllerBase
{
    private readonly IVoteService _voteService;
    private readonly ILogger<VotesController> _logger;
    
    public VotesController(IVoteService voteService, ILogger<VotesController> logger)
    {
        _voteService = voteService;
        _logger = logger;
    }
    
    /// <summary>
    /// Submits a vote for a poll.
    /// </summary>
    /// <param name="request">The vote request.</param>
    /// <returns>Success message.</returns>
    /// <response code="201">Vote recorded successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Duplicate vote attempt.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitVote([FromBody] VoteRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new ErrorResponse("ValidationError", string.Join(" ", errors)));
        }
        
        await _voteService.SubmitVoteAsync(request);
        
        return StatusCode(StatusCodes.Status201Created, new VoteResponse());
    }
}
