using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TiberiumRim.GameParts
{
    public class DefCount<T> where T : Def
    {
        public T def;
        public int value = 1;

        public T Def => def;
        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public DefCount()
        {

        }
        public DefCount(T def, int value)
        {
            this.def = def;
            this.value = value;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
            value = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
        }

        public override string ToString()
        {
            return $"{def.LabelCap} ({value})";
        }
    }
}
