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
    [Route("Home/CreateNewAccount")]
    public IActionResult CreateAccount([FromForm] string firstName, [FromForm] string lastName, [FromForm] decimal balance)
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

            _logger.LogInformation("Account created! {FirstName} {LastName}, R${Balance},00", firstName, lastName, balance);
            return View("Menu", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError("", "Error creating account");
            return View();
        }
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
