using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TR.Data;

public class TypeFloat<T>
{
    public T type;
    public float value = 1;

    public TypeFloat()
    {
    }

    public TypeFloat(T type, float value)
    {
        this.type = type;
        this.value = value;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
        var array = s.Split(',');
        type = (T)ParseHelper.FromString(array[0], typeof(T));
        if (array.Length > 1)
            value = (float)ParseHelper.FromString(array[1], typeof(float));
    }
}