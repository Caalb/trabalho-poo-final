using System;
using System.Xml;
using PucBank.Models;

namespace PucBank.Services.Interfaces;

public interface IReceiptService
{
    TransactionHistory ImportReceipt(IFormFile receiptFile, TransactionHistory currentHistory);
    XmlDocument ExportReceipt(TransactionHistory receipt);
}