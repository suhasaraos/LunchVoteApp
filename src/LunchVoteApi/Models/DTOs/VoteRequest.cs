using System.ComponentModel.DataAnnotations;

namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Request model for submitting a vote.
/// </summary>
public class VoteRequest
{
    /// <summary>
    /// The poll ID to vote in.
    /// </summary>
    [Required(ErrorMessage = "PollId is required.")]
    public Guid PollId { get; set; }
    
    /// <summary>
    /// The selected option ID.
    /// </summary>
    [Required(ErrorMessage = "OptionId is required.")]
    public Guid OptionId { get; set; }
    
    /// <summary>
    /// The anonymous voter token from the client device/browser.
    /// </summary>
    [Required(ErrorMessage = "VoterToken is required.")]
    [MaxLength(64, ErrorMessage = "VoterToken must not exceed 64 characters.")]
    public string VoterToken { get; set; } = string.Empty;
}
