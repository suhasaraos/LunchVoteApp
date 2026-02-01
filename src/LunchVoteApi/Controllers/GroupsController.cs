using LunchVoteApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LunchVoteApi.Controllers;

/// <summary>
/// API controller for group operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GroupsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly ILogger<GroupsController> _logger;
    
    public GroupsController(IPollService pollService, ILogger<GroupsController> logger)
    {
        _pollService = pollService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all group IDs that have active polls.
    /// </summary>
    /// <returns>An array of group IDs with active polls.</returns>
    /// <response code="200">List of group IDs retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveGroups()
    {
        var groupIds = await _pollService.GetActiveGroupIdsAsync();
        return Ok(groupIds);
    }
}
