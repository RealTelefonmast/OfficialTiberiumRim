using System;

namespace TeleCore.Lib.Serialization.XML.CustomTest.Serialization;

public abstract class XmlConverter
{
    public abstract Type Type { get; }
    public bool IsValueType { get; }

    internal XmlConverter()
    {
        if (Type == null)
        {
            throw new Exception("Cannot create converter without type.");
        }

        IsValueType = Type.IsValueType;
    }
}

public abstract class XmlConverter<T> : XmlConverter
{
    public sealed override Type Type => typeof(T);

    public abstract void Serialize(T obj);
    public abstract T Deserialize();
}