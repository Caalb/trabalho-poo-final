using System.Diagnostics;
using System.Xml;

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
        private readonly IReceiptService _receiptService;

        public HomeController(ILogger<HomeController> logger, IAccountService accountService, IReceiptService receiptService)
        {
            _logger = logger;
            _accountService = accountService;
            _receiptService = receiptService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/CreateAccount")]
        public IActionResult CreateAccount([FromForm] string firstName, [FromForm] string lastName, [FromForm] double balance)
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
        public IActionResult Deposit([FromForm] double depositAmount)
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
        public IActionResult Withdraw([FromForm] double withdrawAmount)
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

        [Route("Home/ImportHistory")]
        public IActionResult ImportHistory(XmlDocument history)
        {
            try
            {
                var userJson = TempData["User"]?.ToString();
                if (userJson == null)
                {
                    return RedirectToAction("Index");
                }

                var user = JsonConvert.DeserializeObject<Account>(userJson);
                var receipt = _receiptService.ImportReceipt(history);

                // Overwrites user properties based on new receipt (TransactionHistory)

                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing receipt");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("ShowMenu");
            }
            return View();
        }

         [Route("Home/ExportHistory")]
        public IActionResult ExportHistory()
        {
            try
            {
                var userJson = TempData["User"]?.ToString();
                if (userJson == null)
                {
                    return RedirectToAction("Index");
                }

                var user = JsonConvert.DeserializeObject<Account>(userJson);
                var transactions = user.TransactionHistory;

                // Exports
                 _receiptService.ExportHistory(transactions);


                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting receipt");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("ShowMenu");
            }
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
