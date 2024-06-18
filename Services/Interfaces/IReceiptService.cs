using System;
using System.Xml;
using PucBank.Models;

namespace PucBank.Services.Interfaces;

public interface IReceiptService
{
    public TransactionHistory ImportReceipt(IFormFile receipt);
    public XmlDocument ExportReceipt(TransactionHistory receipt);
}