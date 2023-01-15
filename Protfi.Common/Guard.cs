using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Protfi.Common;

[DebuggerStepThrough]
public static class Guard
{
    public static void NotNull<T>(Expression<Func<T>> reference, T value) => NotNull(value, GetParameterName(reference));

    public static void NotNull<T>(T value, string name)
    {
        if (value == null)
            throw new ArgumentNullException(name, "Parameter cannot be null.");
    }
        
    public static void NotNullOrEmpty(Expression<Func<string>> reference, string value)
    {
        NotNull<string>(reference, value);
        if (value.Length == 0)
            throw new ArgumentException("Parameter cannot be empty.", GetParameterName(reference));
    }

    public static void NotNullOrEmpty(string value, string name)
    {
        NotNull(value, name);
        if (value.Length == 0)
            throw new ArgumentException("Parameter cannot be empty.", name);
    }

    public static void FileExists(string path, string fileDescription = null)
    {
        if (!File.Exists(path))
        {
            var fileName = !String.IsNullOrEmpty(fileDescription) ? $"{fileDescription} file" : "File";
            throw new ArgumentException($"{fileName} '{path}' does not exist.");
        }
    }
        
    public static void IsMatch(string value, string regex, string name)
    {
        if (!Regex.IsMatch(value, regex))
        {
            throw new ArgumentException($"Parameter '{name}' does not match the expected pattern.", name);
        }
    }

    public static void IsValid<T>(Expression<Func<T>> reference, T value, Func<T, bool> validate, string message)
    {
        if (!validate(value))
            throw new ArgumentException(message, GetParameterName(reference));
    }
        
    public static void IsValid<T>(Expression<Func<T>> reference, T value, Func<T, bool> validate, string format, params object[] args)
    {
        if (!validate(value))
            throw new ArgumentException(string.Format(format, args), GetParameterName(reference));
    }

    public static void NotEmptyList<T>(ICollection<T> list, string name)
    {
        if (list==null || !list.Any())
            throw new ArgumentException($"The {name} list cannot be null or empty.");
    }

    private static string GetParameterName(Expression reference)
    {
        var lambda = reference as LambdaExpression;
        Debug.Assert(lambda != null, "lambda != null");
        var member = lambda.Body as MemberExpression;

        Debug.Assert(member != null, "member != null");
        return member.Member.Name;
    }
}