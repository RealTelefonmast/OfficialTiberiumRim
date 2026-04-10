using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumChunk : ThingWithComps
    {
        public override void TickLong()
        {
            DamageOrCorruptChunks(this);
        }

        public void DamageOrCorruptChunks(Thing thing)
        {
            int amt = Rand.Range(0, 10);
            DamageInfo damage = new DamageInfo(DamageDefOf.Deterioration, amt);
            if (Rand.Chance(0.07f))
            {
                var c = thing.RandomAdjacentCell8Way();
                if (c.InBounds(thing.Map))
                {
                    Thing thing2 = c.GetFirstHaulable(thing.Map);

                    if (thing2 != null)
                    {
                        if (!thing2.def.defName.Contains("Tiberium"))
                        {
                            if (thing2.def.defName.Contains("Chunk") && Rand.Chance(0.05f))
                            {
                                ThingDef rock = null;

                                switch (thing.def.defName)
                                {
                                    case "GreenTiberiumChunk":
                                        rock = ThingDef.Named("GreenTiberiumChunk");
                                        break;

                                    case "BlueTiberiumChunk":
                                        rock = ThingDef.Named("BlueTiberiumChunk");
                                        break;

                                    case "RedTiberiumChunk":
                                        rock = ThingDef.Named("RedTiberiumChunk");
                                        break;
                                }

                                IntVec3 loc = thing2.Position;

                                if (loc.InBounds(Map))
                                {
                                    thing2.Destroy(DestroyMode.Vanish);
                                    GenSpawn.Spawn(rock, loc, Map);
                                    return;
                                }
                            }
                            thing2.TakeDamage(damage);
                        }
                    }
                }
            }
            else
            {
                thing.TakeDamage(damage);
            }
        }
    }
}
