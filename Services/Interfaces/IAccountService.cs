using PucBank.Models;

namespace PucBank.Services.Interfaces;
public interface IAccountService
{
    Account CreateAccount(string firstName, string lastName, double balance);
    void Deposit(Account user, double depositAmount, string transactionTitle);
    void Withdraw(Account user, double withdrawAmount, string transactionTitle);
}