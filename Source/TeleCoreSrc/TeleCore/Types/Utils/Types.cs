using System;

namespace TeleCore.Types.Utils;

public static class Types
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

        switch (GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
        }

        return false;
    }
}