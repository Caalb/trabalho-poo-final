using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult CreateAccount(IFormCollection form)
    {
        try
        {

            string firstName = form["FirstName"];
            string lastName = form["LastName"];
            int balance = 0;

            if (int.TryParse(form["Balance"], out int parsedBalance))
            {
                balance = parsedBalance;
            }

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

            return View("Menu", user);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError("", "Error creating account");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
