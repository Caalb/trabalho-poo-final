namespace PucBank.Models;

public class TransactionHistory
{
  public List<Transaction> Transactions { get; set; } = [];
  public double GetCurrentBalance()
  {
    return Transactions.Count == 0 ? 0 : Transactions[^1].CurrentBalance;
  }
}