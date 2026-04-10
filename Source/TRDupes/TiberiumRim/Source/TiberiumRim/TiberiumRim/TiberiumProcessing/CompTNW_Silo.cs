using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class CompTNW_Silo : CompTNW
    {
        public override bool ShouldDoEffecters => Container.StoredPercent > 0.5f;

        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                var rect = parent.OccupiedRect();
                var cells = rect.Cells.ToList();
                rect.Corners.ToList().ForEach(x => cells.Remove(x));
                return cells;
            }
        }

        public override void Notify_ContainerFull()
        {
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SILOSNEEDED);
            /*
            ActionComposition composition = new ActionComposition();
            composition.AddPart(delegate ()
            {
                Messages.Message("Silos are needed.", MessageTypeDefOf.NeutralEvent, true);
            }, 2);
            composition.AddPart(delegate ()
            {
                Messages.Message("SILOS NEEDED!", MessageTypeDefOf.NegativeEvent, true);
            }, 4, 6);
            composition.AddPart(delegate ()
            {
                Find.WindowStack.Add(new Dialog_MessageBox("DID YOU KNOW THAT SILOS ARE NEEDED?!?!11?", null, null, null, null, "SILOS NEEDED", false, null, null));
            }, 6);
            composition.Init();
            */
        }
    }
}
