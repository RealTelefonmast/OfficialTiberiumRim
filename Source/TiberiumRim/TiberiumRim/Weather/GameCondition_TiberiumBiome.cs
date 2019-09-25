using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class GameCondition_TiberiumBiome : GameCondition
    {
        public static readonly Color skyColor = new ColorInt().ToColor;
        private MapComponent_Tiberium tiberium;

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget
            {
                colors = new SkyColorSet(skyColor,new Color(), new Color(), 1f),
                glow = 1,
                lightsourceShineIntensity = 1,
                lightsourceShineSize = 1
            };
            return base.SkyTarget(map);
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return base.SkyTargetLerpFactor(map);
        }

        private MapComponent_Tiberium Tiberium => tiberium ?? (tiberium = this.SingleMap.GetComponent<MapComponent_Tiberium>());

        private Color SkyColor => Color.Lerp(Color.white, skyColor, tiberium.TiberiumInfo.Coverage);
    }
}
