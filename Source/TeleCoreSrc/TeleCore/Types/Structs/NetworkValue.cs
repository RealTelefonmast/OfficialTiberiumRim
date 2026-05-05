using TeleCore.Defs;
using UnityEngine;

namespace TeleCore.Types.Structs;

public struct NetworkValue
{
    public NetworkValueDef valueDef;
    public int value;
    public float valueF;

    public NetworkValue(NetworkValueDef def, float value)
    {
        valueDef = def;
        valueF = value;
        this.value = Mathf.RoundToInt(valueF);
    }

    public void AdjustValue(float diff)
    {
        valueF += diff;
        value = Mathf.RoundToInt(valueF);
    }

    public static NetworkValue operator +(NetworkValue a, NetworkValue b)
    {
        a.valueF += b.valueF;
        a.value = Mathf.RoundToInt(a.valueF);
        return a;
    }

    public static NetworkValue operator -(NetworkValue a, NetworkValue b)
    {
        a.valueF -= b.valueF;
        a.value = Mathf.RoundToInt(a.valueF);
        return a;
    }

    public static NetworkValue operator +(NetworkValue a, int b)
    {
        a.valueF += b;
        a.value = Mathf.RoundToInt(a.valueF);
        return a;
    }

    public static NetworkValue operator -(NetworkValue a, int b)
    {
        a.valueF -= b;
        a.value = Mathf.RoundToInt(a.valueF);
        return a;
    }
}