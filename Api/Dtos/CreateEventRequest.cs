using System.ComponentModel.DataAnnotations;
namespace Api.Dtos;

public class CreateEventRequest
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;
}