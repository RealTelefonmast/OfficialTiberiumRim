using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /*TODO: Rebase on flecks
    public class TiberiumSpore : Particle
    {
        public TiberiumCrystalDef crystalDef = TiberiumDefOf.TiberiumPod;
        public TiberiumProducer parent;

        public void SporeSetup(TiberiumCrystalDef crystalDef, TiberiumProducer parent)
        {
            this.crystalDef = crystalDef;
            this.parent = parent;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "parent");
            Scribe_Defs.Look(ref crystalDef, "crystalDef");
        }

        public override void FinishAction()
        {
            base.FinishAction();
            if (crystalDef != null && !map.roofGrid.Roofed(Position))
            {
                GenTiberium.TrySpawnTiberium(endCell, map, crystalDef, parent);
            }
        }

        public override bool ShouldDestroy => base.ShouldDestroy || Position.Roofed(map);

        public override Color Color
        {
            get
            {
                Color color = base.Color;
                switch (crystalDef.TiberiumValueType)
                {
                    case TiberiumValueType.Green:
                        color *= MainTCD.Main.GreenColor;
                        break;
                    case TiberiumValueType.Blue:
                        color *= MainTCD.Main.BlueColor;
                        break;
                    case TiberiumValueType.Red:
                        color *= MainTCD.Main.RedColor;
                        break;
                }
                return color;
            }
        }
    }
    */
}
