using UnityEngine;
using Verse;

namespace TR.Components;

public class MapComponent_Pollution : MapComponent
{
    private BoolGrid pollutionGrid;
    private float pollutionPct;


    public MapComponent_Pollution(Map map) : base(map)
    {
    }

    public float CurrentPollution
    {
        get => pollutionPct;
        set
        {
            pollutionPct += value;
            pollutionPct = Mathf.Clamp01(pollutionPct);
        }
    }
}