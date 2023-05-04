using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using TeleCore.Data.Network;
using Verse;

namespace TiberiumRim
{
    public class MechanicalPawn : FXPawn
    {
        protected MechLink parentLink;
        protected Building parent;

        public MapComponent_Tiberium TiberiumManager => Map.Tiberium();
        public NetworkMapInfo NetworkInfo => TiberiumManager.NetworkInfo;

        public MechLink ParentMechLink
        {
            get => parentLink;
            set => parentLink = value;
        }

        public virtual Building ParentBuilding
        {
            get => parent;
            set => parent = value;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.story == null)
            {
                story = new Pawn_StoryTracker(this);
                skills = new Pawn_SkillTracker(this);
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

        public override void Tick()
        {
            base.Tick();
        }

        public bool IsDamaged()
        {
            return Damage().Any();
        }

        public IEnumerable<Hediff> Damage()
        {
            return from x in health?.hediffSet?.hediffs where x is Hediff_Injury || x is Hediff_MissingPart select x;
        }
    }
}
