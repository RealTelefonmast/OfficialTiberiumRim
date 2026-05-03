using System;

namespace TeleCore.Unsorted;

internal static class TypeUtility
{
    public static bool IsNullableType(this Type type)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static Type GetNonNullableType(this Type type)
    {
        return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
    }

    public static TypeCode GetTypeCode(this Type type)
    {
        return Type.GetTypeCode(type);
    }

    public static bool IsInteger(this Type type)
    {
        type = GetNonNullableType(type);
        if (type.IsEnum) return false;

        return GetTypeCode(type) switch
        {
            TypeCode.Byte => true,
            TypeCode.SByte => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            TypeCode.UInt16 => true,
            TypeCode.UInt32 => true,
            TypeCode.UInt64 => true,
            _ => false
        };
    }

    public static bool IsFloatingPoint(this Type type)
    {
        type = GetNonNullableType(type);

        return GetTypeCode(type) switch
        {
            TypeCode.Single => true,
            TypeCode.Double => true,
            TypeCode.Decimal => true,
            _ => false
        };
    }

    public static bool IsNumeric(this Type type)
    {
        return IsInteger(type) || type.IsFloatingPoint();
    }
}