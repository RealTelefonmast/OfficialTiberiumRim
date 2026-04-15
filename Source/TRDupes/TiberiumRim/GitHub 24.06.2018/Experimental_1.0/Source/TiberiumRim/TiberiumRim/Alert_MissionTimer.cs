using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class Alert_MissionTimer : Alert
    {
        public Alert_MissionTimer()
        {
            this.defaultLabel = "MissionTimer".Translate();
            this.defaultExplanation = "MissionTimerDesc".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        protected override Color BGColor
        {
            get
            {
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * Color.cyan;
            }
        }

        public override string GetExplanation()
        {
            return base.GetExplanation();
        }

        public override AlertReport GetReport()
        {
            if (Find.World.GetComponent<WorldComponent_TiberiumMissions>().Missions.Any((Mission x) => x.objectiveTimer.Values.Any((int y) => y > 0)))
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
