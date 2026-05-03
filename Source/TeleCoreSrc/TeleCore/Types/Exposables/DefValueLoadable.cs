using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TeleCore.Unsorted;

public class DefValueLoadable<TDef, TValue> : IExposable
    where TDef : Def
    where TValue : unmanaged
{
    private TDef def;
    private Numeric<TValue> value;

    public TDef Def => def;
    public Numeric<TValue> Value => value;
    public bool IsValid => def != null!;

    public void ExposeData()
    {
        //Scribe_Defs.Look(ref _def, "def");
        Look(ref def, "def");
        Scribe_Values.Look(ref value, "value");

        return;

        static void Look<T>(ref T value, string label) where T : Def
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                string text;
                text = value == null ? "null" : value.defName;
                Scribe_Values.Look<string>(ref text, label, "null");
                return;
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                value = ScribeExtractor.DefFromNodeUnsafe<T>(Scribe.loader.curXmlParent[label]);
        }
    }

    public static implicit operator DefValue<TDef, TValue>(DefValueLoadable<TDef, TValue> refValue)
    {
        return new DefValue<TDef, TValue>(refValue.Def, refValue.Value);
    }

    public static implicit operator DefValueLoadable<TDef, TValue>(DefValue<TDef, TValue> structValue)
    {
        return new DefValueLoadable<TDef, TValue>
        {
            def = structValue.Def,
            value = structValue.Value
        };
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //Listing
        var fi = typeof(DefValueLoadable<TDef, TValue>).GetField("def", BindingFlags.NonPublic | BindingFlags.Instance);
        if (xmlRoot.Name == "li")
        {
            var innerValue = xmlRoot.InnerText;
            var s = Regex.Replace(innerValue, @"\s+", "");
            var array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, fi, array[0]);
            value = ParseHelper.FromString<TValue>(array.Length > 1 ? array[1] : "0");
        }

        //InLined
        else
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, fi, xmlRoot.Name);
            value = ParseHelper.FromString<TValue>(xmlRoot.FirstChild.Value);
        }
    }
}