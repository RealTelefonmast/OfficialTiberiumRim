using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Designator_RemoveTiberiumPipe : Designator
    {
        public override int DraggableDimensions => 2;
        public override bool DragDrawMeasurements => true;

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.InBounds(Map) || !DebugSettings.godMode && loc.Fogged(Map) || loc.GetThingList(Map).Any(t => CanDesignateThing(t).Accepted))
            {
                return false;
            }
            return true;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            var building = t as Building_NetworkStructureTransmitter;
            if(building == null || building.def.category != ThingCategory.Building || !DebugSettings.godMode && building.Faction != Faction.OfPlayer)
            {
                return false;
            }
            if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (base.Map.designationManager.DesignationOn(t, DesignationDefOf.Uninstall) != null)
            {
                return false;
            }
            return true;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
