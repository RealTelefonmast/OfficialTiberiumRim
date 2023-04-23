using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class GameCondition_TiberiumBiome : GameCondition
    {
        private const float MaxSkyLerpFactor = 0.5f;
        private const float SkyGlow = 0.85f;
        private Color tibColor = new ColorInt(33, 104, 55).ToColor;
        private Color tibColorDark = new ColorInt(27, 86, 45).ToColor;
        //private SkyColorSet TiberiumSkyColors = new SkyColorSet(new ColorInt(33, 104, 55).ToColor, new ColorInt(27, 86, 45).ToColor, new Color(0.6f, 0.8f, 0.5f), SkyGlow);
        private SkyColorSet TiberiumSkyColors = new SkyColorSet(Color.white, Color.white, new ColorInt(75, 160, 92).ToColor, SkyGlow);
        private List<SkyOverlay> overlays = new List<SkyOverlay>
        {
            new WeatherOverlay_TiberiumPollution()
        };

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            for (int j = 0; j < this.overlays.Count; j++)
            {
                for (int k = 0; k < affectedMaps.Count; k++)
                {
                    this.overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }

        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(map);
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return 1f;
            //var val = map.Tiberium().AtmosphericInfo.OutsideContainer.Saturation;
            //return Mathf.Lerp(0,1, val);//GameConditionUtility.LerpInOutValue(this, , MaxSkyLerpFactor);
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return this.overlays;
        }

        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return false;
        }

        public override float PlantDensityFactor(Map map)
        {
            return 0f;
        }

        public override float AnimalDensityFactor(Map map)
        {
            return 0f;
        }

        public override WeatherDef ForcedWeather()
        {
            return WeatherDef.Named("TiberiumClear");
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(0.85f, this.TiberiumSkyColors, 1f, 1f);
        }

    }

    public class GameCondition_TiberiumBiome2 : GameCondition
    {
        public static readonly Color skyColor = new ColorInt().ToColor;
        private MapComponent_Tiberium tiberium;




        private Dictionary<Map, SkyOverlay> TiberiumPollutionOverlay = new Dictionary<Map, SkyOverlay>();

        private Dictionary<Map, List<SkyOverlay>> SkyOverlayData = new Dictionary<Map, List<SkyOverlay>>();

        private MapComponent_Tiberium Tiberium => tiberium ??= this.SingleMap.Tiberium();

        private Color SkyColor => Color.Lerp(Color.white, skyColor, tiberium.TiberiumInfo.InfestationPercent);

        public override void Init()
        {
            base.Init();
        }

        public override void PostMake()
        {

        }

        public override void GameConditionTick()
        {
            Log.Message("Ticking game con..");
            foreach (var value in TiberiumPollutionOverlay.Values)
            {
                value.TickOverlay(Find.CurrentMap);
            }
        }

        public override void GameConditionDraw(Map map)
        {
            Log.Message("Drawing game con..");
            foreach (var value in TiberiumPollutionOverlay.Values)
            {
                value.DrawOverlay(Find.CurrentMap);
            }
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return base.SkyOverlays(map);
            if (!SkyOverlayData.ContainsKey(map))
            {
                SkyOverlayData.Add(map, new List<SkyOverlay>());
            }
            return SkyOverlayData[map];
        }

        public void Notify_PollutionChange(Map onMap, float newVal)
        {
            if (TiberiumPollutionOverlay.ContainsKey(onMap))
            {
                return;
            }
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
            WeatherOverlay_TiberiumPollution mainOverlay = (WeatherOverlay_TiberiumPollution)SkyOverlays(onMap).Find(t => t is WeatherOverlay_TiberiumPollution);
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
            return null;
            return new SkyTarget
            {
                colors = new SkyColorSet(new Color(0.1f, 0.85f, 0.12f), new Color(0.1f, 0.85f, 0.12f), new Color(0.1f, 0.85f, 0.12f), 1f),
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
}
