using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class MechanicalPawn : FXPawn
    {
        private MechLink parentLink;
        protected Building parent;

        public MechLink ParentMechLink
        {
            get => parentLink;
            set => parentLink = value;
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "parent");
        }

        public virtual Building ParentBuilding
        {
            get => parent;
            set => parent = value;
        }

        public NetworkMapInfo NetworkInfo => TiberiumManager.NetworkInfo;

        public MapComponent_Tiberium TiberiumManager => Map.Tiberium();

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
