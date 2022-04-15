using System;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public struct DefValue<T> : IExposable where T : Def 
    {
        private Type type;

        private T def;
        private float value;

        public T Def => (T)def;

        public float Value
        {
            get => value;
            set => this.value = value;
        }
        public void ExposeData()
        {
            Scribe_Universal.Look(ref def, "def", LookMode.Def, ref type);
            Scribe_Values.Look(ref value, "value");
        }

        public static explicit operator DefValue<T>(DefFloat<T> defFloat)
        {
            return new DefValue<T>(defFloat);
        }

        public DefValue(DefFloat<T> defFloat)
        {
            type = typeof(T);
            this.def = defFloat.def;
            this.value = defFloat.value;
        }

        public DefValue(T def, float value)
        {
            type = typeof(T);
            this.def = def;
            this.value = value;
        }
    }

    public class DefFloat<T> where T : Def
    {
        public T def;
        public float value = 1;

        public DefFloat(){}

        public DefFloat(string def, float value)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", def);
            this.value = value;
        }

        public DefFloat(T def, float value)
        {
            this.def = def;
            this.value = value;
        }

        public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", array[0], null, null);
            if (array.Length > 1)
                this.value = (float) ParseHelper.FromString(array[1], typeof(float));
        }

        public override string ToString()
        {
            return def.LabelCap + "(" + value + ")";
        }

        public string ToStringPercent()
        {
            return def.LabelCap + "(" + value.ToStringPercent() + ")";
        }
    }

    public class TypeFloat<T>
    {
        public T type;
        public float value = 1;

        public TypeFloat(){}
        public TypeFloat(T type, float value)
        {
            this.type = type;
            this.value = value;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            type = (T) ParseHelper.FromString(array[0], typeof(T));
            if (array.Length > 1)
                this.value = (float)ParseHelper.FromString(array[1], typeof(float));
        }
    }

}
