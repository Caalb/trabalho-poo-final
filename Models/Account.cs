using System.ComponentModel.DataAnnotations;

namespace PucBank.Models;

public class Account
{
  [Required]
  public Owner Owner { get; set; } = new();
  [Required]
  public double Balance { get; set; }

  public TransactionHistory AccountHistory { get; set; } = new();
}
