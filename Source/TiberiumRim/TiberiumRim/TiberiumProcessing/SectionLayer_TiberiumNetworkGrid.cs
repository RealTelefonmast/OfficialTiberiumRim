using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class SectionLayer_TiberiumNetworkGrid : SectionLayer_Things
    {
        public SectionLayer_TiberiumNetworkGrid(Section section) : base(section)
        {
            this.requireAddToMapMesh = false;
            this.relevantChangeTypes = MapMeshFlag.Buildings;
        }

        public override void DrawLayer()
        {
            if(Find.DesignatorManager.SelectedDesignator is Designator_PlaceThing)
            {
                base.DrawLayer();
                return;
            }
            if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator && ((designator.PlacingDef as ThingDef)?.comps.Any(c => c is CompProperties_TNW) ?? false))
            {
                base.DrawLayer();
                return;
            }
            if (Find.DesignatorManager.SelectedDesignator is Designator_RemoveTiberiumPipe)
            {
                base.DrawLayer();
            }

        }

        protected override void TakePrintFrom(Thing t)
        {
            var comp = t.TryGetComp<CompTNW>();
            comp?.PrintForGrid(this);
        }
    }
}
