using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Alert_TiberiumNeed : Alert_Critical
    {
        public Alert_TiberiumNeed()
        {
            this.defaultLabel = "TiberiumNeed".Translate();
            this.defaultExplanation = "CannotReachTiberium".Translate();
        }

        private Pawn NeedyPawn
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    foreach (Pawn pawn in maps[i].mapPawns.AllPawns.Where((Pawn x) => x.IsColonistPlayerControlled))
                    {
                        if (pawn != null && (pawn.needs.AllNeeds.Find((Need x) => x != null && x is Need_Tiberium) as Need_Tiberium)?.CurCategory == TiberiumNeedCategory.Urgent)
                        {
                            return pawn;
                        }
                    }
                }
                return null;
            }
        }

        public override AlertReport GetReport()
        {
            if (NeedyPawn != null)
            {
                return AlertReport.CulpritIs(NeedyPawn);
            }
            return false;
        }
    }
}
