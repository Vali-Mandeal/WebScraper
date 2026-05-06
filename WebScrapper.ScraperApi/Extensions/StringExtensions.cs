using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebScrapper.ScraperApi.Extensions;

public static class StringExtensions
{
    public static decimal GetPrice(this string priceString)
    {
        var numericString = Regex.Match(priceString, @"[\d.,]+").Value;

        if (string.IsNullOrEmpty(numericString))
        {
            throw new FormatException("No numeric value found in the price string.");
        }

        numericString = numericString.Replace(",", ".");

        return decimal.Parse(numericString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    public static int GetId(this string url)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(url);
        byte[] hash = sha256.ComputeHash(bytes);

        int numericId = BitConverter.ToInt32(hash, 0);

        numericId = Math.Abs(numericId) % 1000000;

        return numericId;
    }
}
