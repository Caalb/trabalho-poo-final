using System;
using System.Xml;
using PucBank.Models;

namespace PucBank.Services.Interfaces;

public interface IAccountService
{
    TransactionHistory ImportReceipt(XmlDocument receipt);
    XmlDocument ExportReceipt();
}