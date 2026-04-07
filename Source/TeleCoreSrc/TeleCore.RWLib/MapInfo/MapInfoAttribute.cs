using System;

namespace TeleCore.RWLib;

public class MapInfoAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}