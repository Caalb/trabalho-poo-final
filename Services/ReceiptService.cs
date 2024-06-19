using Microsoft.AspNetCore.Http;
using System.IO;
using System.Xml;
using PucBank.Models;
using PucBank.Services.Interfaces;

namespace PucBank.Services;

public class ReceiptService : IReceiptService
{
    public TransactionHistory ImportReceipt(IFormFile receiptFile)
    {
        XmlDocument receiptXml = new();
        if (receiptFile != null && receiptFile.Length > 0)
        {
            using var stream = receiptFile.OpenReadStream();
            receiptXml.Load(stream);
        }

        // Convert the XmlDocument to TransactionHistory object
        TransactionHistory transactionHistory = ConvertXmlToTransactionHistory(receiptXml);

        return transactionHistory;
    }

    public XmlDocument ExportReceipt(TransactionHistory receipt)
    {
        XmlDocument receiptXml = ConvertTransactionHistoryToXml(receipt);
        return receiptXml;
    }

    private XmlDocument ConvertTransactionHistoryToXml(TransactionHistory transactions)
    {
        XmlDocument document = new XmlDocument();

        XmlElement root = document.CreateElement("Transactions");
        document.AppendChild(root);

        foreach (var transaction in transactions.Transactions)
        {
            XmlElement transactionElement = document.CreateElement("Transaction");

            XmlElement dateElement = document.CreateElement("Date");
            dateElement.InnerText = transaction.TransactionDate.ToString();
            transactionElement.AppendChild(dateElement);

            XmlElement typeElement = document.CreateElement("Type");
            typeElement.InnerText = transaction.TransactionType.ToString();
            transactionElement.AppendChild(typeElement);

            XmlElement amountElement = document.CreateElement("Amount");
            amountElement.InnerText = transaction.TransactionAmount.ToString();
            transactionElement.AppendChild(amountElement);

            root.AppendChild(transactionElement);
        }

        return document;
    }
}