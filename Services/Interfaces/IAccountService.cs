using PucBank.Models;

namespace PucBank.Services.Interfaces;
public interface IAccountService
{
    Account CreateAccount(string firstName, string lastName, int balance);
    void Deposit(Account user, int depositAmount);
    void Withdraw(Account user, int withdrawAmount);
}