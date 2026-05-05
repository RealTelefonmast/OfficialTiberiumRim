// Preserved from TeleCore/SpreadingGas/GasCellValue.cs

using System.Runtime.InteropServices;

namespace TeleCore.Types.Structs;

[StructLayout(LayoutKind.Explicit, Size = 6)]
public struct GasCellValue_TAE
{
    [FieldOffset(0)] public ushort defID = 0;

    [FieldOffset(2)] public readonly uint totalBitVal = 0;
    [FieldOffset(2)] public ushort value = 0;
    [FieldOffset(4)] public ushort overflow = 0;

    public GasCellValue_TAE(ushort defID, ushort value)
    {
        this.defID = defID;
        this.value = value;
    }

    public GasCellValue_TAE(ushort defID, ushort value, ushort overflow)
    {
        this.defID = defID;
        this.value = value;
        this.overflow = overflow;
    }

    public static GasCellValue_TAE operator +(GasCellValue_TAE self, GasCellValue_TAE value)
    {
        self.value += value.value;
        self.overflow += value.overflow;
        return self;
    }

    public static GasCellValue_TAE operator -(GasCellValue_TAE self, GasCellValue_TAE value)
    {
        self.value -= value.value;
        self.overflow -= value.overflow;
        return self;
    }

    public static GasCellValue_TAE operator +(GasCellValue_TAE self, ushort value)
    {
        self.value += value;
        return self;
    }

    public static GasCellValue_TAE operator -(GasCellValue_TAE self, ushort value)
    {
        self.value -= value;
        return self;
    }

    public static bool operator ==(GasCellValue_TAE self, int value)
    {
        return self.totalBitVal == value;
    }

    public static bool operator !=(GasCellValue_TAE self, int value)
    {
        return self.totalBitVal != value;
    }

    public static GasCellValue_TAE Invalid { get; }
    public static GasCellValue_TAE Empty { get; }

    public override string ToString()
    {
        return $"[{(TAE.SpreadingGasTypeDef)defID}]: ({value}, {overflow})";
    }
}