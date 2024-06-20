using PucBank.Models.Enums;

namespace PucBank.Models;

public class Transaction
{
  public string? TransactionId { get; set; }
  public TransactionType TransactionType { get; set; }
  public string? TransactionTitle { get; set; }
  public double TransactionAmount { get; set; }
  public DateTime TransactionDate { get; set; }
  public double CurrentBalance { get; set; }
}