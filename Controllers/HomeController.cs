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
                Balance = balance
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

    public IActionResult ShowMenu()
    {
        if (TempData["User"] == null)
        {
            return RedirectToAction("Index");
        }

        var userJson = TempData["User"].ToString();
        var user = JsonConvert.DeserializeObject<Account>(userJson);
        return View("Menu", user);
    }

    public IActionResult Deposit([FromForm] int depositAmount)
    {
        var userJson = TempData["User"].ToString();
        var user = JsonConvert.DeserializeObject<Account>(userJson);

        _logger.LogInformation("Depositing R${DepositAmount},00 for {User}", depositAmount, user);

        user.Balance += depositAmount;
        TempData["User"] = JsonConvert.SerializeObject(user);
        return RedirectToAction("ShowMenu");
    }

    public IActionResult Withdraw([FromForm] int withdrawAmount)
    {
        var userJson = TempData["User"].ToString();
        var user = JsonConvert.DeserializeObject<Account>(userJson);
        user.Balance -= withdrawAmount;
        TempData["User"] = JsonConvert.SerializeObject(user);
        return RedirectToAction("ShowMenu");
    }

    public IActionResult History()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
