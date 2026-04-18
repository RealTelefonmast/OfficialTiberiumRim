using System;

namespace TeleCore.Unsorted;

public class MapInfoAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}