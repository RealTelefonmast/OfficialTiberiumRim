using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     Defines properties of any gas or fluid that can dissipate into air or ground.
/// </summary>
public class DissipationConfig : Editable
{
    public DissipationMode mode;

    //TODO: Add terrainfilter from TR
    public List<string> terrainFilter;
    public SpreadingGasTypeDef toGas;
}