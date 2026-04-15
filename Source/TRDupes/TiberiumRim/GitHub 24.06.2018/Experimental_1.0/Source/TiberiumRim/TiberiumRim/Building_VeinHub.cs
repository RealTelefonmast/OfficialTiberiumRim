using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Building_Veinhub : Building
    {
        private int RadialNum = GenRadial.NumCellsInRadius(7);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                for (int i = 0; i < RadialNum; i++)
                {
                    IntVec3 positionToCheck = this.Position + GenRadial.RadialPattern[i];
                    if (positionToCheck.InBounds(Map))
                    {
                        if (!positionToCheck.GetTerrain(Map).Removable)
                        {
                            this.TryAttack(positionToCheck);

                            TerrainDef t = TiberiumDefOf.TiberiumSoilVein;
                            TerrainDef t2 = TiberiumDefOf.TiberiumSoilRed;
                            TerrainDef tt = positionToCheck.GetTerrain(Map);
                            if (tt.defName.Contains("Water") || tt.defName.Contains("Ice") || tt.defName.Contains("Marsh"))
                            {
                                Map.terrainGrid.SetTerrain(positionToCheck, tt);
                            }
                            else if (tt.defName.Contains("Sand"))
                            {
                                Map.terrainGrid.SetTerrain(positionToCheck, t2);
                            }
                            else
                            {
                                Map.terrainGrid.SetTerrain(positionToCheck, t);
                            }
                        }
                    }
                }
            }
        }

        public override void TickRare()
        {
            for (int i = 0; i < RadialNum; i++)
            {
                IntVec3 positionToCheck = this.Position + GenRadial.RadialPattern[i];
                if (positionToCheck.InBounds(Map))
                {
                    if (!positionToCheck.GetTerrain(Map).Removable)
                    {
                        this.TryAttack(positionToCheck);
                    }
                }
            }           
        }

        private void TryAttack(IntVec3 c)
        {
            if (c.InBounds(Map))
            {
                List<Thing> thinglist = c.GetThingList(this.Map);
                for (int i = 0; i < thinglist.Count; i++)
                {
                    if (thinglist[i] is Pawn)
                    {
                        ThingDef tentacle = ThingDef.Named("VeinTentacle");
                        if (c.GetFirstThing(Map, tentacle) == null)
                        {
                            GenSpawn.Spawn(tentacle, c, Map);
                        }
                    }
                }
            }
        }
    }
}
