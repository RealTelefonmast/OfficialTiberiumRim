using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    ///TODO: Implement with underground resource layer
    public class Building_TiberiumGeyser : TRBuilding
    {
        private Building_TiberiumSpike tiberiumSpike;
        private Sustainer spraySustainer;
        private IntermittenFleckSprayer tibSprayer;

        private int sustainerStartTick = -1;

        //Values
        private uint maxDepositValue = 0;
        private float depositValue = 0f;

        //
        private int burstTicksLeft = -1;
        private bool startEnum;

        //
        public static bool makePollutionGas = false;

        public float ContentPercent => depositValue / maxDepositValue;
        public bool Bursting => burstTicksLeft > 0;
        public override bool ShouldDoEffecters => tiberiumSpike.Spawned;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref maxDepositValue, "maxDeposit");
            Scribe_Values.Look(ref depositValue, "depositValue");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            tibSprayer = new IntermittenFleckSprayer(this, delegate
            {
                TR_FleckMaker.ThrowTiberiumAirPuff(this.TrueCenter(), Map);
                if (Find.TickManager.TicksGame % 20 == 0)
                {
                    depositValue--;
                    GenTemperature.PushHeat(this, 40f);
                    var cell = this.OccupiedRect().RandomCell;
                    if (cell.GetGas(Map) is SpreadingGas spreadGas)
                    {
                        spreadGas.AdjustSaturation(Rand.Range(500, 1000), out _);
                        return;
                    }
                    var gas = (SpreadingGas)GenSpawn.Spawn(ThingDef.Named("Gas_TiberiumGas"), cell, Map);
                    gas.AdjustSaturation(Rand.Range(500, 1000), out _);
                }
            }, StartSpray, EndSpray);
            if (respawningAfterLoad) return;

            depositValue = TRUtils.Range(10000, 30000);
            maxDepositValue = TRUtils.Range((uint)depositValue, 30000U);
        }

        private void StartSpray()
        {
            SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, Map, 4f, -0.06f);
            spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(Position, Map));
            sustainerStartTick = Find.TickManager.TicksGame;
        }

        private void EndSpray()
        {
            if (spraySustainer != null)
            {
                spraySustainer.End();
                spraySustainer = null;
            }
        }

        public override void Tick()
        {
            if (depositValue <= 0) return;

            if (tiberiumSpike == null)
            {
                tibSprayer.SprayerTick();

                //Refill
                if (Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
                {
                    if (depositValue < maxDepositValue)
                    {
                        depositValue += TRUtils.Range(100, 450);
                    }
                }
            }
            else if (tiberiumSpike?.Spawned ?? false)
            {
                if (tiberiumSpike.IsPoweredOn())
                {
                    if (tiberiumSpike.TibComponent.Container.TryAddValue(TiberiumDefOf.TibGas, 0.25f, out float actualValue))
                    {
                        depositValue -= actualValue;
                    }
                }
                return;
            }

            if (spraySustainer != null && Find.TickManager.TicksGame > sustainerStartTick + 1000)
            {
                Log.Message("Tiberium Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
                spraySustainer.End();
                spraySustainer = null;
            }
        }

        public void Notify_SpikeSpawned(Building_TiberiumSpike tibSpike)
        {
            tiberiumSpike = tibSpike;
        }

        public void Notify_SpikeDespawned()
        {
            tiberiumSpike = null;
        }
        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine($"{"TR_GasDeposit".Translate()}: {depositValue}l");
            sb.AppendLine($"Making Gas: {makePollutionGas}");
            return sb.ToString().TrimEndNewlines();

        }

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
                    makePollutionGas = !makePollutionGas;
                    //Map.Tiberium().AtmosphericInfo.AddDirect(Position.RandomAdjacentCell8Way(), 100);
                }
            };
        }

    }
}
