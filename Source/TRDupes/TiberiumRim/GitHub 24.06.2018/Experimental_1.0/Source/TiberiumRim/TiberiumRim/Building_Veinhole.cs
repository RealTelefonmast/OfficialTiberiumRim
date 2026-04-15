using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class Building_Veinhole : Building_TiberiumProducer
    {
        private int hubsInt = 0;

        private int hubsRadius = 50;

        private int ticksToSpawn = 0;

        private Lord defenseLord;

        private List<Thing> hubList = new List<Thing>();

        private static readonly Material WireMat = MaterialPool.MatFrom("Building/Natural/Veinhole/VeinConnection");

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if(this.defenseLord == null)
                {
                    if(!CellFinder.TryFindRandomCellNear(this.Position, this.Map, this.def.terrainRadius, (IntVec3 c) => c.Standable(this.Map) && this.Map.reachability.CanReach(this.Position, this, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out IntVec3 cell))
                    {
                        Log.Error("No cell found for veinhub defense lord.");
                        cell = IntVec3.Invalid;
                    }
                    LordJob_MechanoidsDefendShip lordJob = new LordJob_MechanoidsDefendShip(this, null, this.def.terrainRadius, cell);
                    this.defenseLord = LordMaker.MakeNewLord(null, lordJob, this.Map, null);
                }
                ResetTimer();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            KillHubs();
            base.KillBoundCrystals();
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref ticksToSpawn, "ticksTopSpawn");
            Scribe_Values.Look<int>(ref this.hubsInt, "progressTicks", 0, false);
            Scribe_References.Look<Lord>(ref this.defenseLord, "defenseLord", false);
            Scribe_Collections.Look<Thing>(ref this.hubList, "hubs", LookMode.Reference);         
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!base.IsResearchBound)
            {
                if (ticksToSpawn <= 0)
                {
                    GrowHub();
                    ResetTimer();
                }
                ticksToSpawn -= GenTicks.TickRareInterval;
                if (Rand.Chance(0.001f))
                {
                    SpawnVeinmonster();
                }              
            }
            else
            {
                KillHubs();
            }
        }

        public void ResetTimer()
        {
            IntRange range = new IntRange(GenDate.TicksPerDay, GenDate.TicksPerQuadrum);
            this.ticksToSpawn = range.RandomInRange;
        }

        public void KillHubs()
        {
            foreach (Thing t in hubList)
            {
                if (t != null)
                {
                    t.Destroy();
                }
            }
        }

        public void SpawnVeinmonster()
        {       
            IntVec3 pos = this.RandomAdjacentCell8Way();

            int maximum = Map.listerThings.AllThings.FindAll((Thing x) => x.def == this.def).Count * 12;
            int Veinmonsters = Map.listerThings.AllThings.FindAll((Thing x) => x.def.defName.Contains("Veinmonster_TBI")).Count;

            if (Veinmonsters < maximum)
            {
                PawnKindDef Veinmonster = PawnKindDef.Named("Veinmonster_TBI");
                PawnGenerationRequest request = new PawnGenerationRequest(Veinmonster);
                Pawn pawn = PawnGenerator.GeneratePawn(request);
                GenSpawn.Spawn(pawn, pos, Map);
                this.defenseLord.AddPawn(pawn);
            }
        }

        private void GrowHub()
        {
            if (hubsInt < 6)
            {
                ThingDef hubDef = DefDatabase<ThingDef>.GetNamed("VeinholeHub_TBNS");
                int num = GenRadial.NumCellsInRadius(hubsRadius);
                IntVec3 spawnCell = IntVec3.Invalid ;
                bool flag = true;
                while (flag)
                {
                    int rand = Rand.Range(1, num);
                    IntVec3 pos = this.Position + GenRadial.RadialPattern[rand];
                    if (pos.InBounds(Map))
                    {
                        if (pos.IsFarAwayEnoughFromAny(hubDef, Map, 12))
                        {
                            List<Thing> thinglist = pos.GetThingList(Map);
                            foreach (Thing t in thinglist)
                            {
                                if (t.def != hubDef)
                                {
                                    if (t.def.passability == Traversability.Standable)
                                    {
                                        if (!pos.GetTerrain(Map).defName.Contains("Water") || !pos.GetTerrain(Map).defName.Contains("Marsh"))
                                        {
                                            spawnCell = pos;
                                            flag = !flag;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (spawnCell.IsValid)
                {
                    Thing hub = ThingMaker.MakeThing(hubDef, null);
                    GenSpawn.Spawn(hub, spawnCell, Map);
                    hubsInt += 1;
                    hubList.Add(hub);
                    Messages.Message("VeinHubSpawned".Translate(), new TargetInfo(hub.Position, Map, false), MessageTypeDefOf.CautionInput);
                }
            }
        }

        //TODO: Add connection between veinhole and hubs
        public void MakeTerrainConnection()
        {

        }

        public override void Print(SectionLayer layer)
        {
            Material mat = WireMat; 
            float y = Altitudes.AltitudeFor(AltitudeLayer.SmallWire);
            foreach (Building_Veinhub hub in hubList)
            {
                Vector3 center = (this.TrueCenter() + hub.TrueCenter()) / 2f;
                center.y = y;
                Vector3 v = hub.TrueCenter() - this.TrueCenter();
                Vector2 size = new Vector2(1f, v.MagnitudeHorizontal());
                float rot = v.AngleFlat();
                Printer_Plane.PrintPlane(layer, center, size, mat, rot, false, null, null, 0.01f);
            }
            base.Print(layer);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            base.GetGizmos();
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn Hub",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        GrowHub();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn Veiny",
                    icon = TexCommand.Attack,
                    action = delegate
                    {
                        Log.Message("TryingToSpawnVeinmonster");
                        this.SpawnVeinmonster();
                    }
                };
            }
            yield break;
        }
    }
}
