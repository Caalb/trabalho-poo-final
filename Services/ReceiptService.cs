using System.Xml;
using PucBank.Models;
using PucBank.Services.Interfaces;
using PucBank.Models.Enums;

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

        TransactionHistory newTransactionHistory = ConvertXmlToTransactionHistory(receiptXml);

        return newTransactionHistory;
    }

    private static XmlDocument ConvertTransactionHistoryToXml(TransactionHistory transactions)
    {
        XmlDocument document = new XmlDocument();

        // Create the root element
        XmlElement root = document.CreateElement("Transactions");
        document.AppendChild(root);

        // Iterate through each transaction in the transaction history
        foreach (var transaction in transactions.Transactions)
        {
            // Create a Transaction element for each transaction
            XmlElement transactionElement = document.CreateElement("Transaction");

            // Create and append the Date element
            XmlElement dateElement = document.CreateElement("Date");
            dateElement.InnerText = transaction.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss");
            transactionElement.AppendChild(dateElement);

            // Create and append the Type element
            XmlElement typeElement = document.CreateElement("Type");
            typeElement.InnerText = transaction.TransactionType.ToString();
            transactionElement.AppendChild(typeElement);

            // Create and append the Amount element
            XmlElement amountElement = document.CreateElement("Amount");
            amountElement.InnerText = transaction.TransactionAmount.ToString(); // No specific formatting
            transactionElement.AppendChild(amountElement);

            // Create and append the CurrentBalance element
            XmlElement balanceElement = document.CreateElement("CurrentBalance");
            balanceElement.InnerText = transaction.CurrentBalance.ToString(); // No specific formatting
            transactionElement.AppendChild(balanceElement);

            // Append the Transaction element to the root
            root.AppendChild(transactionElement);
        }

        return document;
    }

    private static TransactionHistory ConvertXmlToTransactionHistory(XmlDocument receiptXml)
    {
        TransactionHistory transactions = new();

        XmlNodeList? transactionNodes = receiptXml.SelectNodes("//Transaction");
        if (transactionNodes != null)
        {
            foreach (XmlNode transactionNode in transactionNodes)
            {
                Transaction newTransaction = new();

                // Parse the Date element
                XmlNode? dateNode = transactionNode.SelectSingleNode("Date");
                if (dateNode != null)
                {
                    newTransaction.TransactionDate = DateTime.Parse(dateNode.InnerText);
                }

                // Parse the Type element
                XmlNode? typeNode = transactionNode.SelectSingleNode("Type");
                if (typeNode != null)
                {
                    newTransaction.TransactionType = Enum.Parse<TransactionType>(typeNode.InnerText);
                }

                // Parse the Amount element
                XmlNode? amountNode = transactionNode.SelectSingleNode("Amount");
                if (amountNode != null)
                {
                    newTransaction.TransactionAmount = double.Parse(amountNode.InnerText);
                }

                // Parse the CurrentBalance element
                XmlNode? balanceNode = transactionNode.SelectSingleNode("CurrentBalance");
                if (balanceNode != null)
                {
                    newTransaction.CurrentBalance = double.Parse(balanceNode.InnerText);
                }

                transactions.Transactions.Add(newTransaction);
            }
        }

        return transactions;
    }

    public XmlDocument ExportReceipt(TransactionHistory receipt)
    {
        XmlDocument receiptXml = ConvertTransactionHistoryToXml(receipt);
        return receiptXml;
    }

}