using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Text;

namespace Protfi.Common.Extensions;

public static class StringExtensions
{
    public const string ValidEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                            + @"([-A-Za-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                            + @"@[A-Za-z0-9][\w\.-]*[A-Za-z0-9]\.[A-Za-z][A-Za-z\.]*[A-Za-z]$";
    public const string ValidUrlPattern = @"(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?";

    private static IReadOnlyCollection<(string Code,string Value)>  HtmlSpecialCodes { get; } = new List<(string,string)>
    {
        ("&nbsp;", " "),
        ("&amp;", "&"),
        ("&quot;", "\""),
        ("&apos;;", "'"),
        ("&lt;", "<"),
        ("&gt;", ">"),
        ("&reg;", "®"),
        ("&copy;", "©"),
        ("&cent;", "¢")
    };

    private static Regex HtmlTagStripper { get; } = new Regex("<[^>]*>", RegexOptions.Compiled | RegexOptions.Multiline);
    private static Regex NewLineStripper { get; } = new Regex("\n+|<br/s*/>", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
    public static Regex ValidEmailRegex { get; } = new (ValidEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ImageExtensions = new HashSet<string>(new[] { "jpg", "jpeg", "gif", "png", "bmp", "tiff", "ico" }, StringComparer.InvariantCultureIgnoreCase);

    public static bool IsImageFile(this string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }
        value = value.Trim().TrimEnd('.');
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        var dotIndex = value.LastIndexOf('.');
        var extension = dotIndex == -1 ? value : value.Substring(dotIndex + 1);
        return ImageExtensions.Contains(extension);
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source.IndexOf(toCheck, comp) >= 0;
    }

    public static bool InCollection(this string value, StringComparison comp, params string[] collectionItems)
    {
        foreach (var collectionItem in collectionItems)
        {
            if (collectionItem.Contains(value, comp))
                return true;
        }

        return false;
    }

    public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
    {
        newValue = newValue.EmptyIfNull();
        int startIndex = 0;
        while (true)
        {
            startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
            if (startIndex == -1)
                break;

            originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

            startIndex += newValue.Length;
        }

        return originalString;
    }
    public static bool IsNotEmpty(this string str)
    {
        return String.IsNullOrEmpty(str) == false;
    }

    public static IEnumerable<string> Split(this string str)
    {
        var parts = str.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        return parts.ToList();
    }

    public static bool IsDouble(this string str)
    {
        double n;
        bool isDouble = Double.TryParse(str, out n);
        return isDouble;
    }

    public static bool EmailIsValid(this string emailAddress)
    {
        try
        {
            bool isValid = ValidEmailRegex.IsMatch(emailAddress);

            return isValid;
        }
        catch (Exception)
        {
            return false;
        }
    }
        
    public static bool IsEmpty(this string str)
    {
        return String.IsNullOrEmpty(str) || str.Equals("null",StringComparison.InvariantCultureIgnoreCase);
    }

    public static string GetHostFromUrl(this string url)
    {
        try
        {
            var myUri = new Uri(url);
            return myUri.Host;
        }
        catch (Exception)
        {
            return url;
        }
    }
    public static string Sentencify(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var final = string.Empty;
        for (var i = 0; i < value.Length; i++)
        {
            final += (char.IsUpper(value[i]) && ((i == 0 || !Char.IsUpper(value[i - 1])) ||
                                                 (i != (value.Length - 1) && !char.IsUpper(value[i + 1]))) ?
                " " : "") + value[i];
        }

        return final.TrimStart(' ');
    }

    public static bool EqualsOrBothNull(this string str, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (str == null && value == null) return true;
        if (str == null || value == null) return false;
        return str.Equals(value, comparisonType);
    }


    public static string NullIfEmpty(this string str) => str.IsEmpty() ? null : str;

    public static string EmptyIfNull(this string str) => str ?? string.Empty;

    public static string Or(this string str, string orValue) => string.IsNullOrEmpty(str) ? orValue : str;
        
    // Case manipulation 

    public static string ToTitleCase(this string baseString, CultureInfo culture = null)
    {
        culture = culture ?? CultureInfo.CurrentCulture;
        var normalizedString = baseString.EmptyIfNull().ToLower().Replace("_", " ");

        return culture.TextInfo.ToTitleCase(normalizedString); // call ToLower to avoid all-uppercase words staying uppercase.
    }

    public static string ExpandPascalCase(this string baseString)
    {
        return Regex.Replace(baseString.EmptyIfNull(), "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
    }


    public static string UpperFirstLetter(this string content)
    {
        return content.EmptyIfNull().First().ToString().ToUpper() + content.Substring(1);
    }

    public static string LowerFirstLetter(this string content)
    {
        return content.EmptyIfNull().First().ToString().ToLower() + content.Substring(1);
    }

    public static string ExtractLdapString(this string ldapString)
    {
        ldapString = FixOrangeStyleLdapString(ldapString);
        if (ldapString == null) return null;

        var ldapUri = new Uri(ldapString);
        var lastSegment = ldapUri.Segments.Last();
        if (lastSegment.IsDistinguishedName())
        {
            var dnSegments = lastSegment.Split(",");
            var firstCn = dnSegments.FirstOrDefault(segment => segment.StartsWith("cn=", StringComparison.CurrentCultureIgnoreCase));
            if (firstCn == null)
                return null;
                
            return firstCn.Split('=').Last();
        }
        else
        {
            return lastSegment;
        }

    }

    public static string FixOrangeStyleLdapString(this string ldapString)
    {
        var uriSegments = ldapString.Split("/");
        if (uriSegments.Length < 3)
            return null;
        if (uriSegments[2].Contains(' '))
            uriSegments[2] = uriSegments[2].ReplaceFirst(" ", "/");
        ldapString = string.Join("/", uriSegments);
        return ldapString;
    }

    public static bool IsLdapString(this string ldapString)
    {
        return ldapString.IsNotEmpty() && ldapString.StartsWith("ldap://", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsDistinguishedName(this string possibleDistinguishedName)
    {
        return possibleDistinguishedName.Contains("cn=", StringComparison.CurrentCultureIgnoreCase)
               || possibleDistinguishedName.Contains("ou=", StringComparison.CurrentCultureIgnoreCase);
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static SecureString ToSecureString(this string source)
    {
        if (String.IsNullOrWhiteSpace(source))
            return null;
        var result = new SecureString();
        foreach (char c in source)
            result.AppendChar(c);
        return result;
    }

    public static string GetInsecurePassword(this SecureString password)
    {
        if (password == null)
            return "";

        var unmanagedString = IntPtr.Zero;
        try
        {

            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(password);
            return Marshal.PtrToStringUni(unmanagedString);
                
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }

    public static string UrlCombine(this string baseUrl, string urlSegment)
    {
        return baseUrl.EndsWith("/")
            ? baseUrl + urlSegment
            : baseUrl + "/" + urlSegment;
    }

    public static string Join(this IEnumerable<string> strings, string separator)
    {
        return String.Join(separator, strings);
    }

    public static int? AsIntOrDefault(this string str, int? defaultValue = null)
    {
        int value;
        if (int.TryParse(str, out value))
            return value;
        return defaultValue;
    }

    public static long? AsLongOrDefault(this string str, long? defaultValue = null)
    {
        long value;
        if (long.TryParse(str, out value))
            return value;
        return defaultValue;
    }

    public static Stream ToStream(this string text, Encoding encoding = null)
    {
        encoding = encoding ?? Encoding.UTF8;
        if (text==null)
        {
            text=String.Empty;
        }
        return new MemoryStream(encoding.GetBytes(text));
            
    }

    public static string RemoveAllWhiteSpaces(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        
        var res = string.Concat(str.Where(c => !c.IsWhiteSpaceCharacter()));
        return res;
    }
        
    public static string ToUnderscoreGuid(this Guid g)
    {
        return g.ToString().Replace('-', '_');
    }
        
    public static bool IsValidUrl(this string url)
    {
        if (url == null) return false;
        var regex = new Regex(ValidUrlPattern);
        return regex.IsMatch(url);
    }

    public static string JoinToSqlColumns(this IEnumerable<string> columns, string table = null)
    {
        return string.Join(", ", columns.Select(col => $"{(table != null ? $"\"{table}\"." : "")}\"{col}\""));
    }
}