using PucBank.Models.Enums;

namespace PucBank.Models;

public class Transaction
{
  public string? TransactionId { get; set; }
  public TransactionsType TransactionType { get; set; }
  public string? TransactionTitle { get; set; }
  public int TransactionAmount { get; set; }
  public DateTime TransactionDate { get; set; }
  public int CurrentBalance { get; set; }
}