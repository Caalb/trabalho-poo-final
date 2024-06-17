using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using PucBank.Models;
using PucBank.Services.Interfaces;

namespace PucBank.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;

        public HomeController(ILogger<HomeController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

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
                var user = _accountService.CreateAccount(firstName, lastName, balance);
                TempData["User"] = JsonConvert.SerializeObject(user);
                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                ModelState.AddModelError("", "Error creating account");
                return View("Index");
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
            try
            {
                var userJson = TempData["User"]?.ToString();
                if (userJson == null)
                {
                    return RedirectToAction("Index");
                }

                var user = JsonConvert.DeserializeObject<Account>(userJson);
                _accountService.Deposit(user, depositAmount);
                TempData["User"] = JsonConvert.SerializeObject(user);

                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during deposit");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("ShowMenu");
            }
        }

        [HttpPost]
        [Route("Home/Withdraw")]
        public IActionResult Withdraw([FromForm] int withdrawAmount)
        {
            try
            {
                var userJson = TempData["User"]?.ToString();
                if (userJson == null)
                {
                    return RedirectToAction("Index");
                }

                var user = JsonConvert.DeserializeObject<Account>(userJson);
                _accountService.Withdraw(user, withdrawAmount);
                TempData["User"] = JsonConvert.SerializeObject(user);

                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during withdraw");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("ShowMenu");
            }
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
}
