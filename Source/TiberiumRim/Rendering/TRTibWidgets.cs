using System;
using TR.GameParts.Networks.AtmosphericNetwork;
using TR.TiberiumObjects;
using TR.Util;
using UnityEngine;

namespace TR.Rendering;

public static class TRTibWidgets
{
    public static Color ColorFor(Enum enumType)
    {
        if (enumType is TiberiumValueType tibType)
        {
            return tibType.GetColor();
        }

        if (enumType is AtmosphericValueType atmosType)
        {

        }
        return Color.white;
    }
}