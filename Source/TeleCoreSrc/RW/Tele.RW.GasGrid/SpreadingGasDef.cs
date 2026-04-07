using Verse;

namespace TeleCore.RimWorld.GasGrid;

/// <summary>
/// Placeholder for the gas definition
/// </summary>
public class SpreadingGasDef : Def
{
    public int minSpreadDensity = 256;
    public ushort maxDensityPerCell = 65535;
    public float spreadViscosity = 0.35f;
    public int dissipationAmount = 1;
    // ... other properties
}