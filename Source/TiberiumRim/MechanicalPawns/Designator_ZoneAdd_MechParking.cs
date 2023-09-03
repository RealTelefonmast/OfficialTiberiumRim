using RimWorld;
using UnityEngine;
using Verse;

namespace TR
{
    public class Designator_ZoneAdd_MechParking : Designator_ZoneAdd
    {
        public MechanicalPawnKindDef mechKindDef;

        public Designator_ZoneAdd_MechParking(MechanicalPawnKindDef mechKindDef)
        {
            this.mechKindDef = mechKindDef;
            this.zoneTypeToPlace = typeof(Zone_MechParking);
            this.defaultLabel = "TR_MechParkingZone".Translate();
            this.defaultDesc = "TR_MechParkingZoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true);
        }

        public override string NewZoneLabel => mechKindDef + "";

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.Standable(Find.CurrentMap);
        }

        public override Zone MakeNewZone()
        {
            if(mechKindDef == null)
                Log.Error("Designator MechParking has missing mechKindDef!");
            return new Zone_MechParking(Find.CurrentMap.zoneManager, mechKindDef);
        }
    }
}
