using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{ 
    public class TiberiumGeyser : TRBuilding
    {
        public TNW_TiberiumSpike tiberiumSpike;
        //public List<TiberiumGeyserCrack> currentCracks = new List<TiberiumGeyserCrack>();

        private List<IntVec3> area = new List<IntVec3>();
        private IEnumerator<IntVec3> areaCells;

        private int burstTicksLeft = -1;
        private int cellAmounts;

        private int maxDepositValue = 0;
        private float depositValue = 0f;

        private bool startEnum;

        //TODO: Make cracks instances and let pawns check for cracks instead of ticking each crack

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var cells = GenRadial.RadialCellsAround(Position, 9.9f, false).ToList();
            cellAmounts = cells.Count;
            areaCells = cells.GetEnumerator();
            
            if (!respawningAfterLoad)
            {
                depositValue = TRUtils.Range(10000, 30000);
                maxDepositValue = TRUtils.Range((int)depositValue, 30000);
            }
        }

        private void SetupCracks()
        {
            var cells = GenRadial.RadialCellsAround(Position, 9.9f, false);
            /*
            for (int i = 0; i < 22;)
            {
                IntVec3 cell = cells.RandomElement();
                if (cell.InBounds(Map) && !cell.GetThingList(Map).Any(t => t is TiberiumGeyserCrack || t.def.IsEdifice() || t.def == ThingDefOf.SteamGeyser))
                {
                    currentCracks.Add((TiberiumGeyserCrack) GenSpawn.Spawn(TiberiumDefOf.TiberiumGeyserCrack, cell, Map));
                    i++;
                }
            }
            */
        }

        public override void ExposeData()
        {
            Log.Message("Exposing");
            base.ExposeData();
            Scribe_Values.Look(ref maxDepositValue, "maxDeposit");
            Scribe_Values.Look(ref depositValue, "depositValue");
            Log.Message("Finished saving and loading");
        }

        public override void Tick()
        {
            base.Tick();
            if (!(depositValue > 0)) return;

            if (!tiberiumSpike.DestroyedOrNull() && tiberiumSpike.CompTNW.CompPower.PowerOn)
            {
                if (tiberiumSpike.CompTNW.Container.TryAddValue(TiberiumValueType.Gas, 0.25f, out float actualValue))
                {
                    depositValue -= actualValue;
                }
            }


            if (!startEnum)
                startEnum = this.IsHashIntervalTick(3000);
            if (startEnum && !Bursting)
            {
                Log.Message("Starting burst");
                StartBursting();
            }

            if (Bursting)
            {
                ThrowTiberiumGas(DrawPos, Map);
                GenSpawn.Spawn(TiberiumDefOf.TiberiumGas, areaCells.Current, Map);
                if (!areaCells.MoveNext())
                {
                    areaCells.Reset();
                }

                burstTicksLeft--;
                if (burstTicksLeft <= 0)
                {
                    startEnum = false;
                    return;
                }
            }

            /*
                foreach (IntVec3 pos in CurrentCracks.Keys)
                {
                    var pawn = pos.GetFirstPawn(Map);
                    if (pawn != null)
                    {
                        GenSpawn.Spawn(ThingDef.Named("Mote_TiberiumGeyser"), pos, Map);
                        HediffUtils.TryAffectPawn(pawn, null, true, 1);
                        depositValue -= Mathf.Clamp(TRUtils.Range(1, 4), 0, depositValue);
                    }
                }
            */

            if (Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
            {
                if (depositValue < maxDepositValue)
                {
                    depositValue += TRUtils.Range(100, 450);
                }
            }
        }

        private void StartBursting()
        {
            burstTicksLeft = TRUtils.Range(cellAmounts/2, cellAmounts);
        }

        private void ThrowTiberiumGas(Vector3 loc, Map map)
        {
            if (!loc.ToIntVec3().ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }

            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(TiberiumDefOf.Mote_TiberiumGeyser, null);
            moteThrown.Scale = 1.5f;
            moteThrown.rotationRate = (float)Rand.RangeInclusive(-240, 240);

            moteThrown.exactPosition = loc;
            moteThrown.exactPosition += new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
            moteThrown.SetVelocity((float)Rand.Range(-45, 45), Rand.Range(1.2f, 1.5f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public float ContentPercent => depositValue / maxDepositValue;

        public bool Bursting => burstTicksLeft > 0;

        public override bool ShouldDoEffecters => tiberiumSpike.DestroyedOrNull();

        public override void Draw()
        {
            base.Draw();
            if (Find.Selector.IsSelected(this))
            {
                //GenDraw.DrawFieldEdges(potentialCracks, Color.cyan);
            }
            /*
            foreach (IntVec3 cell in CurrentCracks.Keys)
            {
                var graphic = CurrentCracks[cell];
                Graphics.DrawMesh(graphic.MeshAt(Rotation), cell.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Rotation.AsQuat, graphic.MatAt(Rotation), 0);
            }
            */
           // Graphics.DrawMesh(CrackGraphic.MeshAt(),) CrackGraphic. .Draw(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Rot4.Random, null);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("TR_GasDeposit".Translate() + ": " + depositValue + "l");
            return sb.ToString().TrimEndNewlines();

        }
    }
}
