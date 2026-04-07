using System;
using System.Reflection;

namespace TeleCore.Patching;

[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = true)]
public class RequiredAssemblyAttribute(AssemblyName name) : Attribute
{
    public AssemblyName Name { get; } = name;
}