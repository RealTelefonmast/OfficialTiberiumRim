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
        public Dictionary<IntVec3, Graphic> CurrentCracks = new Dictionary<IntVec3, Graphic>();
        [Unsaved]
        private List<IntVec3> positions = new List<IntVec3>();

        private int maxDepositValue = 0;
        private float depositValue = 0f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            SetupCracks();
            if (!respawningAfterLoad)
            {
                depositValue = TRUtils.Range(10000, 30000);
                maxDepositValue = TRUtils.Range((int)depositValue, 30000);
            }
        }

        private void SetupCracks()
        {
            if (!positions.Any())
            {
                var cells = GenRadial.RadialCellsAround(Position, 9.9f, false);
                for(int i = 0; i < 22; i++)
                    positions.Add(cells.RandomElement());
            }
            for(int k = 0; k < positions.Count; k++)
            {
                var cell = positions[k];
                if (!CurrentCracks.ContainsKey(cell) && !this.OccupiedRect().Contains(cell))
                {
                    CurrentCracks.Add(cell, (def.extraGraphicData.Graphic as Graphic_RandomSelection).GraphicAt(k));
                }
            }
        }

        public override void ExposeData()
        {
            Log.Message("Exposing");
            base.ExposeData();
            Scribe_Collections.Look(ref positions, "positions");
            Scribe_Values.Look(ref maxDepositValue, "maxDeposit");
            Scribe_Values.Look(ref depositValue, "depositValue");
            Log.Message("Finished saving and loading");
        }

        public override void Tick()
        {
            base.Tick();
            if(depositValue > 0)
            {
                if (!tiberiumSpike.DestroyedOrNull() && tiberiumSpike.CompTNW.CompPower.PowerOn)
                {
                    if (tiberiumSpike.CompTNW.Container.TryAddValue(TiberiumValueType.Gas, 0.25f, out float excess))
                    {
                        depositValue -= 1f - excess;
                    }
                }
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
            }
            if(Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
            {
                if (depositValue < maxDepositValue)
                {
                    depositValue += TRUtils.Range(100, 450);
                }
            }
        }

        public float ContentPercent
        {
            get
            {
                return depositValue / maxDepositValue;
            }
        }

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
            foreach (IntVec3 cell in CurrentCracks.Keys)
            {
                var graphic = CurrentCracks[cell];
                Printer_Mesh.PrintMesh(layer, cell.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), graphic.MeshAt(Rotation), graphic.MatAt(Rotation));// Graphics.DrawMesh(graphic.MeshAt(Rotation), cell.ToVector3ShiftedWithAltitude(AltitudeLayer.FloorEmplacement), Rotation.AsQuat, graphic.MatAt(Rotation), 0);
            }
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
