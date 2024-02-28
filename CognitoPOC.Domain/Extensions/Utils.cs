using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using PhoneNumbers;

namespace CognitoPOC.Domain.Extensions;

public static class Utils
{
    public const int ListLimit = 5;
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();
    private static readonly Regex RxNonDigits = new(@"[^\d]+");
    internal static string GenerateHash(string? value, string? secret)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(secret))
            return string.Empty;
        var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }
    
    public static string? FormatPhone(this string? phoneNumber)
    {
        
        try
        {
            var phone = PhoneUtil.Parse($"+{CleanPhoneNumber(phoneNumber)}", null);

            if (PhoneUtil.IsValidNumber(phone))
            {
                var formattedNumber = PhoneUtil.Format(phone, PhoneNumberFormat.INTERNATIONAL);
                return string.IsNullOrEmpty(formattedNumber) ? phoneNumber : formattedNumber;
            }
        }
        catch
        {
            // ignored
        }

        return phoneNumber;
    }
    public static string CleanPhoneNumber(this string? s)
    {
        var answer = CleanDigits(s);
        return answer.StartsWith("+") || answer.Length == 0 ? answer : $"+{answer}";
    }
    public static string CleanDigits(string? s)
    {
        return string.IsNullOrEmpty(s) ? string.Empty : RxNonDigits.Replace(s, "");
    }

    public static void RemoveNotInCollection<TKey, TData>(IList<TData> collection, TKey[]? keys, Func<TData, TKey> property)
    {
        if (keys != null)
        {
            var i = 0;
            while (i < collection.Count)
            {
                
                if (!keys.Contains(property.Invoke(collection[i])))
                    collection.RemoveAt(i);
                else
                    i++;
            }
        }
    }
    public static void AddNotInCollection<TData>(IList<TData> collection, IEnumerable<TData> source)
    {
        foreach (var t in source)
        {
            if(!collection.Contains(t))
                collection.Add(t);
        }
    }
}