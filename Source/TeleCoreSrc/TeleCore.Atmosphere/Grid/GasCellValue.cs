using System.Runtime.InteropServices;

namespace TeleCore.Atmosphere.Grid;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct GasCellValue
{
    public ushort defID = 0;
    public ushort value = 0;
    public ushort overflow = 0;

    public uint TotalBitVal
    {
        get { return ((uint)overflow << 16) | value; }
    }

    public static GasCellValue Invalid => new GasCellValue(0, 0, 0);
    public static GasCellValue Empty => new GasCellValue(ushort.MaxValue, 0, 0);

    public GasCellValue(ushort defID, ushort value)
    {
        this.defID = defID;
        this.value = value;
    }

    public GasCellValue(ushort defID, ushort value, ushort overflow)
    {
        this.defID = defID;
        this.value = value;
        this.overflow = overflow;
    }

    public static GasCellValue operator +(GasCellValue self, GasCellValue value)
    {
        self.value += value.value;
        self.overflow += value.overflow;
        return self;
    }

    public static GasCellValue operator -(GasCellValue self, GasCellValue value)
    {
        self.value -= value.value;
        self.overflow -= value.overflow;
        return self;
    }

    public static GasCellValue operator +(GasCellValue self, ushort value)
    {
        self.value += value;
        return self;
    }

    public static GasCellValue operator -(GasCellValue self, ushort value)
    {
        self.value -= value;
        return self;
    }

    //
    public static bool operator ==(GasCellValue left, GasCellValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GasCellValue left, GasCellValue right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if(obj is not GasCellValue other)
            return false;
        return defID == other.defID && value == other.value && overflow == other.overflow;
    }

    public override int GetHashCode()
    {
        unchecked // allows overflow without exception
        {
            return (TotalBitVal.GetHashCode() * 397) ^ defID.GetHashCode();
        }
    }

    public override string ToString()
    {
        return $"[{(SpreadingGasTypeDef)defID}]: ({value}, {overflow})";
    }
}
