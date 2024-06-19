using System.Diagnostics;
using System.Xml;
using System.Text;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using PucBank.Models;
using PucBank.Services.Interfaces;

namespace PucBank.Controllers
{
    public class HomeController(ILogger<HomeController> logger, IAccountService accountService, IReceiptService receiptService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IAccountService _accountService = accountService;
        private readonly IReceiptService _receiptService = receiptService;

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

        [HttpPost]
        [Route("Home/ImportHistory")]
        public IActionResult ImportHistory(IFormFile history)
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

                user.AccountHistory = receipt;
                user.Balance = receipt.GetBalance();

                TempData["User"] = JsonConvert.SerializeObject(user);

                return RedirectToAction("ShowMenu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing receipt");
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("ShowMenu");
            }
        }

        [HttpPost]
        [Route("Home/ExportHistory")]
        public IActionResult ExportHistory()
        {
            try
            {
                var userJson = TempData["User"]?.ToString();
             

                var user = JsonConvert.DeserializeObject<Account>(userJson);
                var transactions = user.AccountHistory;

                if (transactions == null)
                {
                    _logger.LogWarning("No transactions found for the user.");
                    ModelState.AddModelError("", "No transactions to export.");
                    TempData.Keep("User");
                    return RedirectToAction("ShowMenu");
                }

                var xml = _receiptService.ExportReceipt(transactions);

                if (xml == null)
                {
                    _logger.LogError("Error generating XML.");
                    ModelState.AddModelError("", "Error generating export file.");
                    TempData.Keep("User");
                    return RedirectToAction("ShowMenu");
                }

                var xmlString = xml.OuterXml;
                if (string.IsNullOrEmpty(xmlString))
                {
                    _logger.LogError("XML string is empty.");
                    ModelState.AddModelError("", "Generated XML is empty.");
                    TempData.Keep("User");
                    return RedirectToAction("ShowMenu");
                }

                var bytes = Encoding.UTF8.GetBytes(xmlString);
                var stream = new MemoryStream(bytes);

                TempData.Keep("User");

                return File(stream, "application/xml", "receipt.xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting receipt");
                ModelState.AddModelError("", ex.Message);
                TempData.Keep("User");
                return RedirectToAction("ShowMenu");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
