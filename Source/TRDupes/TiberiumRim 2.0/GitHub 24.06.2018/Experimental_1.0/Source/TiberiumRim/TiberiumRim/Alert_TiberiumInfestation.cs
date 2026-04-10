using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class Alert_TiberiumInfestation : Alert
    {
        public override string GetLabel()
        {
            return "InfestationPct".Translate(new object[] {
                Math.Round(InfestationPct, 4) * 100 + "%"
            });
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(Map map in Find.Maps)
            {
                if (World.TiberiumTiles.ContainsKey(map.Tile))
                {
                    float pct = World.GetPct(map.Tile);
                    if (pct > 0f)
                    {
                        stringBuilder.AppendLine("     " + map.info.parent.LabelCap + ": " + Math.Round(pct, 4) * 100 + "%"); 
                    }
                }
            }
            return string.Format("InfestationPctExp".Translate(), stringBuilder.ToString());
        }

        private WorldComponent_TiberiumSpread World
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_TiberiumSpread>();
            }
        }

        private float InfestationPct
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    Map map = maps[i];
                    if (World.TiberiumTiles.ContainsKey(map.Tile))
                    {
                        float pct = World.GetPct(map.Tile);
                        if (pct > 0f)
                        {
                            return pct;
                        }
                    }
                }
                return 0f;
            }
        }

        public override AlertReport GetReport()
        {
            if(InfestationPct > 0f)
            {
                return AlertReport.Active;
            }
            return false;
        }

    }
}
