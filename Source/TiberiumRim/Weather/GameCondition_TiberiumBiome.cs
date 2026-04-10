using System.Collections.Generic;
using RimWorld;
using TR.Components;
using UnityEngine;
using Verse;

namespace TR;

public class GameCondition_TiberiumBiome : GameCondition
{
    public static readonly Color skyColor = new ColorInt().ToColor;
    private MapComponent_Tiberium tiberium;

    private MapComponent_Tiberium Tiberium => tiberium ??= SingleMap.Tiberium();

    private Color SkyColor => Color.Lerp(Color.white, skyColor, tiberium.TiberiumInfo.Coverage);

    public override void PostMake()
    {
    }

    public override void GameConditionTick()
    {
        Log.Message("Ticking game con..");
        foreach (var value in TiberiumPollutionOverlay.Values) 
            value.TickOverlay(Find.CurrentMap);
    }

    public override void GameConditionDraw(Map map)
    {
        Log.Message("Drawing game con..");
        foreach (var value in TiberiumPollutionOverlay.Values) 
            value.DrawOverlay(Find.CurrentMap);
    }

    public override List<SkyOverlay> SkyOverlays(Map map)
    {
        return base.SkyOverlays(map);
        if (!SkyOverlayData.ContainsKey(map)) 
            SkyOverlayData.Add(map, new List<SkyOverlay>());
        return SkyOverlayData[map];
    }

    public void Notify_PollutionChange(Map onMap, float newVal)
    {
        if (TiberiumPollutionOverlay.ContainsKey(onMap)) return;
        TiberiumPollutionOverlay.Add(onMap, new WeatherOverlay_Fog());
        return;
        Log.Message("Changing pollution for skyoverlay...");
        //
        if (newVal <= 0f)
        {
            SkyOverlays(onMap).RemoveAll(t => t is WeatherOverlay_TiberiumPollution);
            Log.Message("removing skyoverlay");
            return;
        }

        var mainOverlay =
            (WeatherOverlay_TiberiumPollution)SkyOverlays(onMap).Find(t => t is WeatherOverlay_TiberiumPollution);
        if (mainOverlay == null)
        {
            mainOverlay = new WeatherOverlay_TiberiumPollution();
            SkyOverlays(onMap).Add(mainOverlay);
            Log.Message("Adding skyoverlay");
        }

        mainOverlay.UpdateMaterial(newVal);
        Log.Message($"adjusting skyoverlay - {newVal} ");
    }

    public override SkyTarget? SkyTarget(Map map)
    {
        return new SkyTarget
        {
            colors = new SkyColorSet(skyColor, new Color(), new Color(), 1f),
            glow = 1,
            lightsourceShineIntensity = 1,
            lightsourceShineSize = 1
        };
        return base.SkyTarget(map);
    }

    /*
    public override void GameConditionTick()
    {
        if (SkyOverlayData.TryGetValue(Find.CurrentMap, out var overlays))
        {
            foreach (var overlay in overlays)
            {
                overlay.TickOverlay(Find.CurrentMap);
            }
        }
        base.GameConditionTick();
    }

    public override void GameConditionDraw(Map map)
    {
        if (SkyOverlayData.TryGetValue(map, out var overlays))
        {
            foreach (var overlay in overlays)
            {
                overlay.DrawOverlay(map);
            }
        }
    }
    */

    public override float SkyTargetLerpFactor(Map map)
    {
        return 1f;
        return base.SkyTargetLerpFactor(map);
    }
}