namespace Protfi.Common.Extensions;

public static class DateTimeExtensions
{
    private static readonly DateTime UnixBaseTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long? ToUnixTime(this DateTime? dt) => dt?.ToUnixTime();

    public static DateTime? ToUniversalTime(this DateTime? dt) => dt?.ToUniversalTime();

    public static long ToUnixTime(this DateTime dt)
    {
        var origin = UnixBaseTime;
        var diff = dt - origin;
        return (long)diff.TotalMilliseconds;
    }

    public static int ToIndexDate(this DateTime dt)
    {
        return Convert.ToInt32(dt.ToString("yyyyMMdd"));
    }

    public static DateTime FromUnixTimeStamp(this long unixTimeStamp)
    {
        return UnixBaseTime.AddMilliseconds(unixTimeStamp);
    }
        
    public static DateTime? FromUnixTimeStamp(this long? unixTimeStamp)
    {
        return unixTimeStamp?.FromUnixTimeStamp();
    }

    public static DateTime? FromUnixTimeStamp(this string unixTimeStampStr)
    {
        if (long.TryParse(unixTimeStampStr, out var unixTimeStamp))
            return unixTimeStamp.FromUnixTimeStamp();
            
        return null;
    }

    public static TimeSpan Multiply(this TimeSpan timespan, int multiplier) => TimeSpan.FromTicks(timespan.Ticks * multiplier);

    public static DateTime? AddMinutes(this DateTime? datetime, double minutes) => datetime?.AddMinutes(minutes);

    public static DateTime? AddHours(this DateTime? datetime, double hours)
    {
        if (datetime.HasValue)
            return datetime.Value.AddHours(hours);

        return null;
    }
        
    public static DateTime SubtractMilliseconds(this DateTime dateTime, double milliseconds) => dateTime.AddMilliseconds(-milliseconds);

    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;

    public static DateTime EndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);

    public static long DaysToMilliseconds(int days) => days * 24 * 60 * 60 * 1000;
        
    public static long UnixNow() => DateTime.UtcNow.ToUnixTime();
}