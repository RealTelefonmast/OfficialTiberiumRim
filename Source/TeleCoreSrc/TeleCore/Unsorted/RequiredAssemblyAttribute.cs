using System;
using System.Reflection;

namespace TeleCore.Unsorted;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = true)]
public class RequiredAssemblyAttribute(AssemblyName name) : Attribute
{
    public AssemblyName Name { get; } = name;
}