using System;

[AttributeUsage(AttributeTargets.Class)]
public class TeleCoreStartupClassAttribute : Attribute;

[AttributeUsage(AttributeTargets.Assembly)]
public class TeleIdentifierAttribute : Attribute;