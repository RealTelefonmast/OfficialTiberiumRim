using System;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim
{
    public struct DefValue<T> where T : Def
    {
        private T def;
        private float value;

        public T Def => def;

        public float Value
        {
            get => value;
            set => this.value = value;
        }

        public static explicit operator DefValue<T>(DefFloat<T> defFloat)
        {
            return new DefValue<T>(defFloat);
        }

        public DefValue(DefFloat<T> defFloat)
        {
            this.def = defFloat.def;
            this.value = defFloat.value;
        }

        public DefValue(T def, float value)
        {
            this.def = def;
            this.value = value;
        }
    }

    public class DefFloat<T> where T : Def
    {
        public T def;
        public float value = 1;



        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
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

}
