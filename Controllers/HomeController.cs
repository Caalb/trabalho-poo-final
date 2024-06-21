using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PucBank.Models;
using PucBank.Models.Enums;
using PucBank.Services.Interfaces;

namespace PucBank.Controllers;

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
            TempData.Keep("User");
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
        TempData.Keep("User");

        return View("Menu", user);
    }

    [HttpPost]
    [Route("Home/Deposit")]
    public IActionResult Deposit([FromForm] double depositAmount, [FromForm] string depositTitle)
    {
        try
        {
            var userJson = TempData["User"]?.ToString();
            if (userJson == null)
            {
                _logger.LogWarning("User data not found in TempData");
                return RedirectToAction("Index");
            }

            var user = JsonConvert.DeserializeObject<Account>(userJson);
            _accountService.Deposit(user, depositAmount, depositTitle);
            TempData["User"] = JsonConvert.SerializeObject(user);
            TempData.Keep("User");

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
    public IActionResult Withdraw([FromForm] double withdrawAmount, [FromForm] string withdrawTitle)
    {
        try
        {
            var userJson = TempData["User"]?.ToString();
            if (userJson == null)
            {
                _logger.LogWarning("User data not found in TempData");
                return RedirectToAction("Index");
            }

            var user = JsonConvert.DeserializeObject<Account>(userJson);
            _accountService.Withdraw(user, withdrawAmount, withdrawTitle);
            TempData["User"] = JsonConvert.SerializeObject(user);
            TempData.Keep("User");

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
                _logger.LogWarning("User data not found in TempData");
                return RedirectToAction("Index");
            }

            var user = JsonConvert.DeserializeObject<Account>(userJson);

            var receipt = _receiptService.ImportReceipt(history);

            user.AccountHistory = receipt;
            user.Balance = receipt.GetCurrentBalance();
            _logger.LogInformation("User data updated with imported receipt");

            TempData["User"] = JsonConvert.SerializeObject(user);
            TempData.Keep("User");

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

            return File(stream, "application/xml", $"extrato.xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting receipt");
            ModelState.AddModelError("", ex.Message);
            TempData.Keep("User");
            return RedirectToAction("ShowMenu");
        }
    }

    [HttpPost]
    [Route("Home/EditTransaction")]
    public IActionResult EditTransaction([FromForm] string transactionId, [FromForm] double amount)
    {
        try
        {
            var userJson = TempData["User"]?.ToString();
            if (userJson == null)
            {
                _logger.LogWarning("User data not found in TempData");
                return RedirectToAction("Index");
            }

            var user = JsonConvert.DeserializeObject<Account>(userJson);

            var transaction = user.AccountHistory.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found.", transactionId);
                ModelState.AddModelError("", "Transaction not found.");
                TempData["User"] = JsonConvert.SerializeObject(user);
                TempData.Keep("User");
                return RedirectToAction("ShowMenu");
            }

            var currentBalanceWithoutTransaction = transaction.TransactionType == TransactionType.Deposit ?
                user.Balance - transaction.TransactionAmount :
                user.Balance + transaction.TransactionAmount;

            var newPotentialBalance = transaction.TransactionType == TransactionType.Deposit ?
                currentBalanceWithoutTransaction + amount :
                currentBalanceWithoutTransaction - amount;

            if (newPotentialBalance < 0)
            {
                _logger.LogWarning("Editing this transaction would result in a negative balance.");
                ModelState.AddModelError("", "Editing this transaction would result in a negative balance.");
                TempData["User"] = JsonConvert.SerializeObject(user);
                TempData.Keep("User");
                return RedirectToAction("ShowMenu");
            }

            transaction.CurrentBalance = newPotentialBalance;
            user.Balance = newPotentialBalance;
            transaction.TransactionAmount = amount;

            TempData["User"] = JsonConvert.SerializeObject(user);
            TempData.Keep("User");
            return RedirectToAction("ShowMenu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing transaction");
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction("ShowMenu");
        }
    }

    [HttpPost]
    [Route("Home/DeleteTransaction")]
    public IActionResult DeleteTransaction([FromForm] string TransactionId)
    {
        try
        {
            var userJson = TempData["User"]?.ToString();
            if (userJson == null)
            {
                _logger.LogWarning("User data not found in TempData");
                return RedirectToAction("Index");
            }

            var user = JsonConvert.DeserializeObject<Account>(userJson);

            var transaction = user.AccountHistory.Transactions.FirstOrDefault(t => t.TransactionId == TransactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found.", TransactionId);
                ModelState.AddModelError("", "Transaction not found.");
                TempData["User"] = JsonConvert.SerializeObject(user);
                TempData.Keep("User");
                return RedirectToAction("ShowMenu");
            }

            var currentBalanceWithoutTransaction = transaction.TransactionType == TransactionType.Deposit ?
                user.Balance - transaction.TransactionAmount :
                user.Balance + transaction.TransactionAmount;

            user.Balance = currentBalanceWithoutTransaction;
            user.AccountHistory.Transactions.Remove(transaction);

            TempData["User"] = JsonConvert.SerializeObject(user);
            TempData.Keep("User");
            return RedirectToAction("ShowMenu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction");
            ModelState.AddModelError("", ex.Message);
            return RedirectToAction("ShowMenu");
        }
    }

    [HttpPost]
    [Route("Home/SearchTransaction")]
    public IActionResult SearchTransaction([FromForm] string transactionTitle)
    {
        try
        {
            var userJson = TempData["User"]?.ToString();
            if (userJson == null)
            {
                _logger.LogWarning("User data not found in TempData");
                return Json(new { success = false, message = "User data not found" });
            }

            TempData.Keep("User");

            var user = JsonConvert.DeserializeObject<Account>(userJson);
            var transactions = user.AccountHistory.Transactions;
            var transaction = transactions.FirstOrDefault(t => t.TransactionTitle == transactionTitle);

            if (transaction == null)
            {
                return Json(new { success = false, message = "Transação não encontrada." });
            }

            return PartialView("_TransactionResultModal", transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching transaction");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error")]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
