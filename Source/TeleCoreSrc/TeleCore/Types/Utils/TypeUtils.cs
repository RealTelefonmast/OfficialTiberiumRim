using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TeleCore.Unsorted;

public static class TypeUtils
{
    public static bool IsAnonymousType(this Type type, out bool isDisplayClass)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var hasCompilerGeneratedAttribute = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
        //bool isGeneric = type.IsGenericType;
        var hasCompilerStrings = type.Name.StartsWith("<>") || type.Name.StartsWith("VB$");
        var hasFlags =
            EnumUtils.HasFlag(type.Attributes,
                TypeAttributes.NotPublic); // type.Attributes.HasFlag(TypeAttributes.NotPublic);

        isDisplayClass = type.Name.Contains("DisplayClass");
        ;
        return hasCompilerGeneratedAttribute && hasCompilerStrings && hasFlags;
    }
}