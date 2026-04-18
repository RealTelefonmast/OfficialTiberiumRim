using System.Collections.Generic;
using System.Linq;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.CompProperties;

public enum AtmosphericVentMode
{
    Intake,
    Output,
    TwoWay
}

public class CompProperties_ANS_Vent : CompProperties_ANS
{
    [Unsaved] private List<AtmosphericValueDef> allowedValuesInt;

    //
    public bool passive = false;
    public IntVec3 intakeOffset;
    public AtmosphericVentMode ventMode = AtmosphericVentMode.Intake;
    public int gasThroughPut = 1;

    //Filter
    private AtmosphericVentFilter filter;

    //
    public List<DefValue<AtmosphericValueDef, float>> upkeepLevels;

    private class AtmosphericVentFilter
    {
        public string acceptedTag;
        public List<AtmosphericValueDef> acceptedAtmospheres;
    }

    public List<AtmosphericValueDef> AllowedValues
    {
        get
        {
            if (allowedValuesInt == null)
            {
                var list = new List<AtmosphericValueDef>();
                if (filter.acceptedTag != null)
                    list.AddRange(AtmosphericReferenceCache.AtmospheresOfTag(filter.acceptedTag));
                if (!filter.acceptedAtmospheres.NullOrEmpty()) list.AddRange(filter.acceptedAtmospheres);
                allowedValuesInt = list.Distinct().ToList();
            }

            return allowedValuesInt;
        }
    }

    public IntVec3 GetIntakePos(IntVec3 basePos, Rot4 rotation)
    {
        return basePos + intakeOffset.RotatedBy(rotation);
    }

}
