using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public struct NetworkValueStack
    {
        public NetworkValue[] networkValues;

        public bool Empty => networkValues.NullOrEmpty();

        public float TotalValue => networkValues.Sum(t => t.valueF);
        public IEnumerable<NetworkValueDef> AllTypes => networkValues.Select(t => t.valueDef);

        public NetworkValue this[NetworkValueDef def]
        {
            get { return networkValues.First(v => v.valueDef == def); }
            set { networkValues.SetValue(value, networkValues.FirstIndexOf(v => v.valueDef == def)); }
        }

        public NetworkValueStack(Dictionary<NetworkValueDef, float> values)
        {
            networkValues = new NetworkValue[values.Count];
            //color = Color.clear;
            int i = 0;
            foreach (var value in values)
            {
                networkValues[i] = new NetworkValue(value.Key, value.Value);
                i++;
                //color += value.Key.valueColor/values.Count;
            }
        }

        public NetworkValueStack(NetworkValueDef def, int val)
        {
            networkValues = new NetworkValue[1];
            networkValues[0] = new NetworkValue(def, val);
            //color = Color.white;
        }

        public NetworkValueStack(int valueCount)
        {
            networkValues = new NetworkValue[valueCount];
            //color = Color.white;
        }

        public void Add(NetworkValueDef def, float value)
        {
            var old = networkValues;
            if (networkValues == null)
            {
                networkValues = new NetworkValue[1] { new(def, value) };
                return;
            }
            networkValues = new NetworkValue[networkValues.Length + 1];
            networkValues.Populate(old);
            networkValues[networkValues.Length - 1] = new NetworkValue(def, value);
        }

        public void Reset()
        {
            networkValues = null;
        }

        public bool HasValue(NetworkValueDef def)
        {
            return networkValues.Any(t => t.valueDef == def);
        }

        public static NetworkValueStack operator +(NetworkValueStack a, NetworkValueStack b)
        {
            if (a.networkValues == null)
            {
                return b;
            }
            else if (b.networkValues == null)
            {
                return a;
            }

            for (var i = 0; i < a.networkValues.Length; i++)
            {
                var valueA = a.networkValues[i];
                for (var k = 0; k < b.networkValues.Length; k++)
                {
                    var valueB = b.networkValues[k];
                    if (valueA.valueDef.Equals(valueB.valueDef))
                    {
                        a.networkValues[i] += valueB;
                    }
                }
            }

            return a;
        }

        public static NetworkValueStack operator -(NetworkValueStack a, NetworkValue b)
        {
            if (!a.HasValue(b.valueDef)) return a;

            a[b.valueDef] = a[b.valueDef] - b;
            return a;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (!networkValues.NullOrEmpty())
            {
                foreach (var value in networkValues)
                {
                    sb.Append($"{value.valueDef}: {value.value}|");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
