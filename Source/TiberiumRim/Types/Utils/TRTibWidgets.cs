using System;
using TiberiumRim;
using TR.Networks.AtmosphericNetwork;
using UnityEngine;

namespace TR;

public static class TRTibWidgets
{
    public static Color ColorFor(Enum enumType)
    {
        if (enumType is TiberiumValueType tibType) return tibType.GetColor();

        if (enumType is AtmosphericValueType atmosType)
        {
        }

        return Color.white;
    }
}