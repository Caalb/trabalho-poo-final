using PucBank.Models.Enums;

namespace PucBank.Models;

public class Transaction
{
  public string? TransactionId { get; set; }
  public TransactionsType TransactionType { get; set; }
  public int Amount { get; set; }
  public DateTime Date { get; set; }
}