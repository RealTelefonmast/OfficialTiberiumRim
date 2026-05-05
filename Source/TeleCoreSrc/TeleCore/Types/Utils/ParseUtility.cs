using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TeleCore.Types.Utils;

internal class ParseMethodAttribute : Attribute
{
    public ParseMethodAttribute(Type type, string methodName)
    {
        Type = type;
        MethodName = methodName;
    }

    public Type Type { get; }
    public string MethodName { get; }
}

public static class ParseUtility
{
    private static readonly Dictionary<Type, Delegate> Parsers = new();

    static ParseUtility()
    {
        InitializeParsers();
    }

    private static void InitializeParsers()
    {
        var methods = typeof(ParseUtility).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(m => new { Method = m, Attribute = m.GetCustomAttribute<ParseMethodAttribute>() })
            .Where(x => x.Attribute != null);

        foreach (var item in methods)
        {
            var parameter = Expression.Parameter(typeof(string), "value");
            var call = Expression.Call(null, item.Method, parameter);
            var lambda = Expression.Lambda(call, parameter);
            Parsers[item.Attribute.Type] = lambda.Compile();
        }
    }

    public static T GeneralParse<T>(string value)
    {
        var curCult = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        if (Parsers.TryGetValue(typeof(T), out var parser))
        {
            var parsed = ((Func<string, T>)parser)(value);
            CultureInfo.CurrentCulture = curCult;
            return parsed;
        }

        CultureInfo.CurrentCulture = curCult;
        throw new NotSupportedException($"Parsing for type {typeof(T)} is not supported.");
    }

    #region Numerics

    [ParseMethod(typeof(byte), nameof(ParseByte))]
    public static byte ParseByte(string value)
    {
        return byte.Parse(value);
    }

    [ParseMethod(typeof(sbyte), nameof(ParseSByte))]
    public static sbyte ParseSByte(string value)
    {
        return sbyte.Parse(value);
    }

    [ParseMethod(typeof(short), nameof(ParseShort))]
    public static short ParseShort(string value)
    {
        return short.Parse(value);
    }

    [ParseMethod(typeof(ushort), nameof(ParseUShort))]
    public static ushort ParseUShort(string value)
    {
        return ushort.Parse(value);
    }

    [ParseMethod(typeof(int), nameof(ParseInt))]
    public static int ParseInt(string value)
    {
        return int.Parse(value);
    }

    [ParseMethod(typeof(uint), nameof(ParseUInt))]
    public static uint ParseUInt(string value)
    {
        return uint.Parse(value);
    }

    [ParseMethod(typeof(long), nameof(ParseLong))]
    public static long ParseLong(string value)
    {
        return long.Parse(value);
    }

    [ParseMethod(typeof(ulong), nameof(ParseULong))]
    public static ulong ParseULong(string value)
    {
        return ulong.Parse(value);
    }

    [ParseMethod(typeof(float), nameof(ParseFloat))]
    public static float ParseFloat(string value)
    {
        return float.Parse(value);
    }

    [ParseMethod(typeof(double), nameof(ParseDouble))]
    public static double ParseDouble(string value)
    {
        return double.Parse(value);
    }

    [ParseMethod(typeof(decimal), nameof(ParseDecimal))]
    public static decimal ParseDecimal(string value)
    {
        return decimal.Parse(value);
    }

    #endregion

    #region Other Value Types

    [ParseMethod(typeof(bool), nameof(ParseBool))]
    public static bool ParseBool(string value)
    {
        return bool.Parse(value);
    }

    [ParseMethod(typeof(char), nameof(ParseChar))]
    public static char ParseChar(string value)
    {
        return char.Parse(value);
    }

    [ParseMethod(typeof(DateTime), nameof(ParseDateTime))]
    public static DateTime ParseDateTime(string value)
    {
        return DateTime.Parse(value);
    }

    [ParseMethod(typeof(DateTimeOffset), nameof(ParseDateTimeOffset))]
    public static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value);
    }

    [ParseMethod(typeof(TimeSpan), nameof(ParseTimeSpan))]
    public static TimeSpan ParseTimeSpan(string value)
    {
        return TimeSpan.Parse(value);
    }

    [ParseMethod(typeof(Guid), nameof(ParseGuid))]
    public static Guid ParseGuid(string value)
    {
        return Guid.Parse(value);
    }

    #endregion

    #region Reference Types

    [ParseMethod(typeof(string), nameof(ParseString))]
    public static string ParseString(string value)
    {
        return value;
    }

    [ParseMethod(typeof(Version), nameof(ParseVersion))]
    public static Version ParseVersion(string value)
    {
        return Version.Parse(value);
    }

    [ParseMethod(typeof(Uri), nameof(ParseUri))]
    public static Uri ParseUri(string value)
    {
        return new Uri(value);
    }

    #endregion

    #region Enums

    [ParseMethod(typeof(DayOfWeek), nameof(ParseEnum))]
    public static DayOfWeek ParseEnum(string value)
    {
        return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), value);
    }

    public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }

    #endregion
}