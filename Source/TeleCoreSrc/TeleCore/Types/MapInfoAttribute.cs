using System;

namespace TeleCore.Types;

public class MapInfoAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}