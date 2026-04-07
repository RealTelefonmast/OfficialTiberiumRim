using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TeleCore.Lib.Serialization;

internal class ParseMethodAttribute : Attribute
{
    public Type Type { get; }
    public string MethodName { get; }

    public ParseMethodAttribute(Type type, string methodName)
    {
        Type = type;
        MethodName = methodName;
    }
}

public static class ParseUtility
{
    private static readonly Dictionary<Type, Delegate> Parsers = new Dictionary<Type, Delegate>();

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
    public static byte ParseByte(string value) => byte.Parse(value);

    [ParseMethod(typeof(sbyte), nameof(ParseSByte))]
    public static sbyte ParseSByte(string value) => sbyte.Parse(value);

    [ParseMethod(typeof(short), nameof(ParseShort))]
    public static short ParseShort(string value) => short.Parse(value);

    [ParseMethod(typeof(ushort), nameof(ParseUShort))]
    public static ushort ParseUShort(string value) => ushort.Parse(value);

    [ParseMethod(typeof(int), nameof(ParseInt))]
    public static int ParseInt(string value) => int.Parse(value);

    [ParseMethod(typeof(uint), nameof(ParseUInt))]
    public static uint ParseUInt(string value) => uint.Parse(value);

    [ParseMethod(typeof(long), nameof(ParseLong))]
    public static long ParseLong(string value) => long.Parse(value);

    [ParseMethod(typeof(ulong), nameof(ParseULong))]
    public static ulong ParseULong(string value) => ulong.Parse(value);

    [ParseMethod(typeof(float), nameof(ParseFloat))]
    public static float ParseFloat(string value) => float.Parse(value);

    [ParseMethod(typeof(double), nameof(ParseDouble))]
    public static double ParseDouble(string value) => double.Parse(value);

    [ParseMethod(typeof(decimal), nameof(ParseDecimal))]
    public static decimal ParseDecimal(string value) => decimal.Parse(value);

    #endregion

    #region Other Value Types

    [ParseMethod(typeof(bool), nameof(ParseBool))]
    public static bool ParseBool(string value) => bool.Parse(value);

    [ParseMethod(typeof(char), nameof(ParseChar))]
    public static char ParseChar(string value) => char.Parse(value);

    [ParseMethod(typeof(DateTime), nameof(ParseDateTime))]
    public static DateTime ParseDateTime(string value) => DateTime.Parse(value);

    [ParseMethod(typeof(DateTimeOffset), nameof(ParseDateTimeOffset))]
    public static DateTimeOffset ParseDateTimeOffset(string value) => DateTimeOffset.Parse(value);

    [ParseMethod(typeof(TimeSpan), nameof(ParseTimeSpan))]
    public static TimeSpan ParseTimeSpan(string value) => TimeSpan.Parse(value);

    [ParseMethod(typeof(Guid), nameof(ParseGuid))]
    public static Guid ParseGuid(string value) => Guid.Parse(value);

    #endregion

    #region Reference Types

    [ParseMethod(typeof(string), nameof(ParseString))]
    public static string ParseString(string value) => value;

    [ParseMethod(typeof(Version), nameof(ParseVersion))]
    public static Version ParseVersion(string value) => Version.Parse(value);

    [ParseMethod(typeof(Uri), nameof(ParseUri))]
    public static Uri ParseUri(string value) => new Uri(value);

    #endregion

    #region Enums

    [ParseMethod(typeof(DayOfWeek), nameof(ParseEnum))]
    public static DayOfWeek ParseEnum(string value) => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), value);
    
    public static TEnum ParseEnum<TEnum>(string value) where TEnum : struct
    {
        return (TEnum)Enum.Parse(typeof(TEnum), value);
    }

    #endregion
    
}