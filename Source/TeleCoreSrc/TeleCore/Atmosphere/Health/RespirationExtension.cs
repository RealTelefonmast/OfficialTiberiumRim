using System.Collections.Generic;
using RimWorld;
using TeleCore.Atmosphere.Defs;
using Verse;

namespace TeleCore.Atmosphere.Health;

public class RespirationExtension : DefModExtension
{
    public int conditionTicks = GenTicks.TickRareInterval;

    public float desiredNeedLevel = 0.21f;
    public AtmosphericValueDef exhaledGas;
    public AtmosphericValueDef inhaledGas;
    public int intervalTicks = GenDate.TicksPerHour;
    public List<RespirationStage> stages;

    public int
        unitsInPerInterval =
            101; //The average person has a respiratory minute volume of 8L(Air)/min, 480L(Air)/h -> 21% oxygen = 101L(Oxygen)/h

    public int unitsOutPerInterval = 101; //

    //Health
    public RespirationWorker worker;

    public bool UsesStages => !stages.NullOrEmpty();

    public RespirationStage StageAt(float level)
    {
        return stages[StageIndexAt(level)];
    }

    public int StageIndexAt(float level)
    {
        if (stages == null) return 0;
        for (var i = stages.Count - 1; i >= 0; i--)
            if (level >= stages[i].minNeedLevel)
                return i;
        return 0;
    }
}