using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Alert_TiberiumInHomeArea : Alert_Critical
    {
        public Alert_TiberiumInHomeArea()
        {
            this.defaultLabel = "TiberiumHomeAlert".Translate();
            this.defaultExplanation = "TiberiumHomeAlertExp".Translate();
        }

        protected override Color BGColor
        {
            get
            {
                return new ColorInt(0, 100, 0).ToColor;
            }
        }

        private TiberiumCrystal TiberiumInHomeArea
        {
            get
            {
                List<Map> maps = Find.Maps;
                for(int i = 0; i < maps.Count; i++)
                {
                    HashSet<TiberiumCrystal> hashset = new HashSet<TiberiumCrystal>();
                    hashset.AddRange(maps[i].GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals);
                    foreach(TiberiumCrystal tiberium in hashset)
                    {
                        if(maps[i].areaManager.Home[tiberium.Position] && !tiberium.Position.Fogged(tiberium.Map))
                        {
                            return tiberium;
                        }
                    }
                }
                return null;
            }
        }

        public override AlertReport GetReport()
        {
            if (TiberiumRimSettings.settings.BuildingDamage)
            {
                TiberiumCrystal tiberium = this.TiberiumInHomeArea;
                if (tiberium != null)
                {
                    return AlertReport.CulpritIs(tiberium);
                }
                return false;
            }
            return false;
        }
    }
}
