using System.Collections.Generic;
using System.Linq;
using TeleCore.Types.Structs;
using Verse;

namespace TeleCore;

public enum AtmosphericVentMode
{
    Intake,
    Output,
    TwoWay
}

public class CompProperties_ANS_Vent : CompProperties_ANS
{
    [Unsaved] private List<AtmosphericValueDef> allowedValuesInt;

    //Filter
    private AtmosphericVentFilter filter;
    public int gasThroughPut = 1;
    public IntVec3 intakeOffset;

    //
    public bool passive = false;

    //
    public List<DefValue<AtmosphericValueDef, float>> upkeepLevels;
    public AtmosphericVentMode ventMode = AtmosphericVentMode.Intake;

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

    private class AtmosphericVentFilter
    {
        public List<AtmosphericValueDef> acceptedAtmospheres;
        public string acceptedTag;
    }
}