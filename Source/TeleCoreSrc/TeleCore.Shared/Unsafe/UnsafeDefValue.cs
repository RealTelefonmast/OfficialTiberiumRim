using System.Runtime.InteropServices;
using TeleCore.Lib;
using TeleCore.Loader;
using TeleCore.Math;
using Verse;

namespace TeleCore.Shared.Unsafe;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct UnsafeDefValue<TDef, TValue>
    where TDef : Def
    where TValue : unmanaged
{
    [FieldOffset(0)]
    public int defID; //4 bytes
    [FieldOffset(4)]
    public Numeric<TValue> _value; //Up to 8 bytes

    public UnsafeDefValue(Def def, Numeric<TValue> value)
    {
        defID = DefIDStack.ToID(def);
        _value = value;
    }

    public TDef Def => new DefID<TDef>(defID).Def;
}