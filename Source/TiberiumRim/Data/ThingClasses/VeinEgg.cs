using RimWorld;
using Verse;

namespace TR
{
    public class VeinEgg : TRBuilding
    {
        public Veinhole parent;
        private int ticksToHatch = TRandom.Range(GenDate.TicksPerDay, GenDate.TicksPerDay * 3);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToHatch, "ticksToHatch");
            Scribe_References.Look(ref parent, "parent");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Hatch(true);
            base.Destroy(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
            ticksToHatch -= 250;
            if (ticksToHatch <= 0)
            {
                Hatch();
            }
        }

        public void Hatch(bool premature = false)
        {
            int num = TRandom.Range(1, 4);
            for(int i = 0; i < num; i++)
            {
                PawnKindDef Veinmonster = PawnKindDef.Named("Veinmonster");
                VeinRoamer veiny = TRUtils.NewBorn(Veinmonster) as VeinRoamer;
                veiny.boundProducer = parent;
                GenSpawn.Spawn(veiny, this.RandomAdjacentCell8Way(), Map);
                if (parent.DestroyedOrNull() || premature)
                {
                    veiny.Kill(new DamageInfo(DamageDefOf.Deterioration, 100));
                }
            }
        }
    }
}
