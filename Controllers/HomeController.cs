using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PucBank.Models;

namespace PucBank.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Route("Home/CreateAccount")]
    public IActionResult CreateAccount([FromForm] string firstName, [FromForm] string lastName, [FromForm] int balance)
    {
        try
        {
            _logger.LogInformation("Creating account for {FirstName} {LastName} with balance {Balance}", firstName, lastName, balance);

            Account user = new()
            {
                Owner = new Owner
                {
                    FirstName = firstName,
                    LastName = lastName
                },
                Balance = balance,
                UserHistory = new TransactionHistory()
            };

            TempData["User"] = JsonConvert.SerializeObject(user);
            _logger.LogInformation("Account created! {FirstName} {LastName}, R${Balance},00", firstName, lastName, balance);
            return RedirectToAction("ShowMenu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError("", "Error creating account");
            return View();
        }
    }

    [Route("Home/ShowMenu")]
    public IActionResult ShowMenu()
    {
        if (TempData["User"] == null)
        {
            return RedirectToAction("Index");
        }

        var userJson = TempData["User"].ToString();
        var user = JsonConvert.DeserializeObject<Account>(userJson);

        TempData["User"] = userJson;

        return View("Menu", user);
    }

    [HttpPost]
    [Route("Home/Deposit")]
    public IActionResult Deposit([FromForm] int depositAmount)
    {
        if (depositAmount <= 0)
        {
            ModelState.AddModelError("", "Invalid deposit amount");
            return RedirectToAction("ShowMenu");
        }

        var userJson = TempData["User"]?.ToString();
        if (userJson == null)
        {
            return RedirectToAction("Index");
        }

        var user = JsonConvert.DeserializeObject<Account>(userJson);

        _logger.LogInformation("Depositing R${DepositAmount},00 for {User}", depositAmount, user);
        user.Balance += depositAmount;
        TempData["User"] = JsonConvert.SerializeObject(user);

        var depositTransaction = new Transaction()
        {
            TransactionId = Guid.NewGuid().ToString(),
            TransactionType = "Deposit",
            Amount = depositAmount,
            Date = DateTime.Now
        };

        var userHistory = user.UserHistory.Transactions;
        userHistory.Add(depositTransaction);

        return RedirectToAction("ShowMenu");
    }

    [HttpPost]
    [Route("Home/Withdraw")]
    public IActionResult Withdraw([FromForm] int withdrawAmount)
    {
        var userJson = TempData["User"]?.ToString();
        if (userJson == null)
        {
            return RedirectToAction("Index");
        }

        var user = JsonConvert.DeserializeObject<Account>(userJson);

        if (withdrawAmount <= 0 || withdrawAmount > user?.Balance || user?.Balance < withdrawAmount)
        {
            ModelState.AddModelError("", "Invalid withdraw amount");
            return RedirectToAction("ShowMenu");
        }

        _logger.LogInformation("Withdrawing R${WithdrawAmount},00 for {User}", withdrawAmount, user);
        user.Balance -= withdrawAmount;

        TempData["User"] = JsonConvert.SerializeObject(user);

        return RedirectToAction("ShowMenu");
    }

    [Route("Home/History")]
    public IActionResult History()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
