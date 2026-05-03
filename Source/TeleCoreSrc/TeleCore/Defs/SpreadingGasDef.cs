using Verse;

namespace TeleCore.Defs;

/// <summary>
///     Placeholder for the gas definition
/// </summary>
public class SpreadingGasDef : Def
{
    public int dissipationAmount = 1;
    public ushort maxDensityPerCell = 65535;
    public int minSpreadDensity = 256;

    public float spreadViscosity = 0.35f;
    // ... other properties
}