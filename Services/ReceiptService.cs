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
        // Convert the TransactionHistory object back to XmlDocument
        XmlDocument receiptXml = ConvertTransactionHistoryToXml(receipt);

        return receiptXml;
    }

    private TransactionHistory ConvertXmlToTransactionHistory(XmlDocument receiptXml)
    {
        // Conversion logic here
        return new TransactionHistory();
    }

    private XmlDocument ConvertTransactionHistoryToXml(TransactionHistory receipt)
    {
        // Conversion logic here
        XmlDocument document = new XmlDocument();
        // Populate the XmlDocument with data from the TransactionHistory object
        return document;
    }
}