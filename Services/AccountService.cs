using PucBank.Models;
using PucBank.Models.Enums;
using PucBank.Services.Interfaces;

namespace PucBank.Services;

public class AccountService(ILogger<AccountService> logger) : IAccountService
{
    private readonly ILogger<AccountService> _logger = logger;

    public Account CreateAccount(string firstName, string lastName, int balance)
    {
        _logger.LogInformation("Creating account for {FirstName} {LastName} with balance {Balance}", firstName, lastName, balance);

        var user = new Account
        {
            Owner = new Owner
            {
                FirstName = firstName,
                LastName = lastName
            },
            Balance = balance,
            UserHistory = new TransactionHistory()
        };

        _logger.LogInformation("Account created! {FirstName} {LastName}, R${Balance},00", firstName, lastName, balance);
        return user;
    }

    public void Deposit(Account user, int depositAmount)
    {
        if (depositAmount <= 0)
        {
            throw new ArgumentException("Invalid deposit amount");
        }

        _logger.LogInformation("Depositing R${DepositAmount},00 for {User}", depositAmount, user);
        user.Balance += depositAmount;

        var depositTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            TransactionType = TransactionsType.Deposit,
            Amount = depositAmount,
            Date = DateTime.Now,
            Balance = user.Balance
        };

        var userTransactions = user.UserHistory;
        userTransactions.Transactions.Add(depositTransaction);
    }

    public void Withdraw(Account user, int withdrawAmount)
    {
        if (withdrawAmount <= 0 || withdrawAmount > user.Balance)
        {
            throw new ArgumentException("Invalid withdraw amount");
        }

        _logger.LogInformation("Withdrawing R${WithdrawAmount},00 for {User}", withdrawAmount, user);
        user.Balance -= withdrawAmount;

        var withdrawTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            TransactionType = TransactionsType.Withdraw,
            Amount = withdrawAmount,
            Date = DateTime.Now,
            Balance = user.Balance
        };

        var userTransactions = user.UserHistory;
        userTransactions.Transactions.Add(withdrawTransaction);
    }
}