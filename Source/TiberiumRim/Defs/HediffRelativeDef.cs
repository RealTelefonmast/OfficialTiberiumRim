using System.Collections.Generic;
using Verse;

namespace TR.Defs;

public class HediffRelativeDef : HediffDef
{
    public int capacityInterval = 750;
    public List<PawnCapacityModifier> relativeCapMods = new();
    public float relativePainFactor = 0;
    public float relativePartEfficiency = 0;
}