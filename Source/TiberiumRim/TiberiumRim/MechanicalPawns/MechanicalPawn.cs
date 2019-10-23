using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class MechanicalPawn : FXPawn
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.story == null)
            {
                story = new Pawn_StoryTracker(this);
            }
            if (this.Faction == Faction.OfPlayer)
            {
                if (this.playerSettings == null)
                {
                    this.playerSettings = new Pawn_PlayerSettings(this);
                }
                if (this.drafter == null)
                {
                    this.drafter = new Pawn_DraftController(this);
                }
            }
        }

        public MapComponent_TNWManager TNWManager
        {
            get
            {
                return Map.GetComponent<MapComponent_TNWManager>();
            }
        }

        public MapComponent_Tiberium TiberiumManager
        {
            get
            {
                return Map.GetComponent<MapComponent_Tiberium>();
            }
        }

        public bool IsDamaged()
        {
            return Damage().Any();
        }

        public IEnumerable<Hediff> Damage()
        {
            return from x in health?.hediffSet?.GetHediffs<Hediff>() where x is Hediff_Injury || x is Hediff_MissingPart select x;
        }
     
    }
}
