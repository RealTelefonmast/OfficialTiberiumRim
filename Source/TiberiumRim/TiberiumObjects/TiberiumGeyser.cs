using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{ 
    public class TiberiumGeyser : TRBuilding
    {
        public CompTNS_TiberiumSpike tiberiumSpike;
        //public List<TiberiumGeyserCrack> currentCracks = new List<TiberiumGeyserCrack>();


        private int burstTicksLeft = -1;

        private int maxDepositValue = 0;
        private float depositValue = 0f;

        private bool startEnum;

        //TODO: Make cracks instances and let pawns check for cracks instead of ticking each crack

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

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
            base.ExposeData();
            Scribe_Values.Look(ref maxDepositValue, "maxDeposit");
            Scribe_Values.Look(ref depositValue, "depositValue");
        }

        public override void Tick()
        {
            base.Tick();
            if (!(depositValue > 0)) return;

            if (tiberiumSpike.Thing.Spawned)
            {
                if (tiberiumSpike.IsPowered)
                {
                    if (tiberiumSpike.Container.TryAddValue(TiberiumValueType.Gas, 0.25f, out float actualValue))
                    {
                        depositValue -= actualValue;
                    }
                }
                return;
            }


            if (!startEnum)
                startEnum = this.IsHashIntervalTick(3000);
            if (startEnum && !Bursting)
            {
                StartBursting();
            }

            if (Bursting)
            {
                ThrowTiberiumGas(DrawPos, Map);
                burstTicksLeft--;

                depositValue--;
                //Map.Tiberium().PollutionInfo.GenerateGasAt(Position.RandomAdjacentCell8Way(), 0.01f);

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
                        HediffUtils.TryInfectPawn(pawn, null, true, 1);
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
            burstTicksLeft = TRUtils.Range(300, 600);
        }

        private void ThrowTiberiumGas(Vector3 loc, Map map)
        {
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

        public override bool ShouldDoEffecters => tiberiumSpike.Thing.Spawned;

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
            sb.AppendLine("Making Gas: " + makeGas);
            return sb.ToString().TrimEndNewlines();

        }

        public static bool makeGas = false;
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Make Gas",
                action = delegate
                {
                    makeGas = !makeGas;
                    //Map.Tiberium().PollutionInfo.AddDirect(Position.RandomAdjacentCell8Way(), 100);
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Get Shader Tex",
                action = delegate
                {
                    var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TestFolder/";
                    var texture1 = Shader.GetGlobalTexture(ShaderPropertyIDs.WaterOutputTex);

                    RenderTexture rt = texture1 as RenderTexture;

                    RenderTexture.active = rt;
                    Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    RenderTexture.active = null;

                    byte[] bytes1 = tex.EncodeToPNG();

                    File.WriteAllBytes(dirPath + "WATEROUTPUT" + ".png", bytes1);

                    var texture2 = Shader.GetGlobalTexture(ShaderPropertyIDs.WaterOffsetTex) as Texture2D;
                    byte[] bytes2 = texture2.EncodeToPNG();
                    File.WriteAllBytes(dirPath + "WATERFLOWMAPRW" + ".png", bytes2);
                }
            };
        }
    }
}
