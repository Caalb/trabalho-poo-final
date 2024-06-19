namespace PucBank.Models;

public class TransactionHistory
{
  public List<Transaction> Transactions { get; set; } = [];
  public double GetCurrentBalance()
  {
    double balance = 0;
    foreach (var transaction in Transactions)
    {
      balance += transaction.CurrentBalance;
    }
    return balance;
  }
}