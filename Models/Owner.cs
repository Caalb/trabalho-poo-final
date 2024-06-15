using System.ComponentModel.DataAnnotations;

namespace PucBank.Models
{
  public class Owner
  {
    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }
  }
}