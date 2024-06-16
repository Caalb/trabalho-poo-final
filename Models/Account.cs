using System.ComponentModel.DataAnnotations;

namespace PucBank.Models

{
  public class Account
  {
    [Required]
    public Owner Owner { get; set; } = new();
    [Required]
    public int Balance { get; set; }
  }
}