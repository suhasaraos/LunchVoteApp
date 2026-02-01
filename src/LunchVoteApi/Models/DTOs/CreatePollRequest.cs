using System.ComponentModel.DataAnnotations;

namespace LunchVoteApi.Models.DTOs;

/// <summary>
/// Request model for creating a new poll.
/// </summary>
public class CreatePollRequest
{
    /// <summary>
    /// The team/group identifier.
    /// </summary>
    [Required(ErrorMessage = "GroupId is required.")]
    [MaxLength(50, ErrorMessage = "GroupId must not exceed 50 characters.")]
    public string GroupId { get; set; } = string.Empty;
    
    /// <summary>
    /// The poll question.
    /// </summary>
    [Required(ErrorMessage = "Question is required.")]
    [MaxLength(200, ErrorMessage = "Question must not exceed 200 characters.")]
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// The voting options.
    /// </summary>
    [Required(ErrorMessage = "Options are required.")]
    [MinLength(2, ErrorMessage = "At least 2 options are required.")]
    public List<string> Options { get; set; } = new();
}

/// <summary>
/// Custom validation for Options list.
/// </summary>
public class OptionsValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is List<string> options)
        {
            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option))
                {
                    return new ValidationResult("Option text cannot be empty.");
                }
                if (option.Length > 100)
                {
                    return new ValidationResult("Each option must not exceed 100 characters.");
                }
            }
        }
        return ValidationResult.Success;
    }
}
