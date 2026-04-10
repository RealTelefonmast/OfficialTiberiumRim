using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class IncidentWorker_IonStorm : IncidentWorker_MakeGameCondition
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            float pct = (map.Size.x * map.Size.z) / map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals.Count;
            bool result;
            if (pct > 0.45f)
            {
                int duration = Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f);
                GameCondition cond = GameConditionMaker.MakeCondition(this.def.gameCondition, duration, 1);
                GameCondition cond2 = GameConditionMaker.MakeCondition(GameConditionDefOf.SolarFlare, duration, 1);
                map.gameConditionManager.RegisterCondition(cond);
                if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
                {
                    map.gameConditionManager.ActiveConditions.Find((GameCondition x) => x.def == GameConditionDefOf.SolarFlare).End();
                }
                map.gameConditionManager.RegisterCondition(cond2);
                map.weatherManager.TransitionTo(WeatherDef.Named("IonStormWeather"));
                base.SendStandardLetter();
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}