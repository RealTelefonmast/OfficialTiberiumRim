using System;

namespace TeleCore.Unsorted;

public abstract class XmlConverter
{
    internal XmlConverter()
    {
        if (Type == null) throw new Exception("Cannot create converter without type.");

        IsValueType = Type.IsValueType;
    }

    public abstract Type Type { get; }
    public bool IsValueType { get; }
}

public abstract class XmlConverter<T> : XmlConverter
{
    public sealed override Type Type => typeof(T);

    public abstract void Serialize(T obj);
    public abstract T Deserialize();
}