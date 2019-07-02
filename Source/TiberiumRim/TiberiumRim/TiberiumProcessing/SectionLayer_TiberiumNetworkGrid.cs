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
            Designator_PlaceThing designatorPipe = (Find.DesignatorManager.SelectedDesignator as Designator_PlaceThing);
            if(designatorPipe != null)
            {
                base.DrawLayer();
                return;
            }
            Designator_Build designator = (Find.DesignatorManager.SelectedDesignator as Designator_Build);
            if (designator != null && ((designator.PlacingDef as ThingDef)?.comps.Any(c => c is CompProperties_TNW) ?? false))
            {
                base.DrawLayer();
                return;
            }
            Designator_RemoveTiberiumPipe designator2 = (Find.DesignatorManager.SelectedDesignator as Designator_RemoveTiberiumPipe);
            if (designator2 != null)
            {
                base.DrawLayer();
            }
        }

        protected override void TakePrintFrom(Thing t)
        {
            var comp = t.TryGetComp<CompTNW>();
            if (comp != null)
            {
                comp.PrintForGrid(this);
            }
        }
    }
}
