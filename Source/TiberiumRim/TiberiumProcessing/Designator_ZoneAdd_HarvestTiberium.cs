using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Designator_ZoneAdd_HarvestTiberium : Designator_ZoneAdd
    {
        private CompTNS_Refinery parentRefinery;
        public override string NewZoneLabel => "TR_HarvestTiberiumZone".Translate();

        public Designator_ZoneAdd_HarvestTiberium(CompTNS_Refinery parentRefinery)
        {
            this.parentRefinery = parentRefinery;
            this.zoneTypeToPlace = typeof(Zone_HarvestTiberium);
            this.defaultLabel = "TR_HarvestTiberiumZone".Translate();
            this.defaultDesc = "TR_HarvestTiberiumZoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Icons/ZoneCreate_HarvestTiberium", true);
            this.hotKey = KeyBindingDefOf.Misc2;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!base.CanDesignateCell(c).Accepted) return false;

            if (c.GetTerrain(base.Map).passability == Traversability.Impassable)
            {
                return false;
            }
            List<Thing> list = base.Map.thingGrid.ThingsListAt(c);
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].def.CanOverlapZones)
                {
                    return false;
                }
            }
            return true;
            //return Map.Tiberium().TiberiumProducerInfo.HasProducerAt(c, out _);
        }

        public override void Deselected()
        {
            base.Deselected();
        }

        public override Zone MakeNewZone()
        {
            var newZone = new Zone_HarvestTiberium(Find.CurrentMap.zoneManager);
            newZone.ParentRefinery = parentRefinery;
            parentRefinery.HarvestTiberiumZone = newZone;
            return newZone;
        }
    }
}
