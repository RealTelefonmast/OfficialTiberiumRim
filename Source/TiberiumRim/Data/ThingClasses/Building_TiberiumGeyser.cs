using System.Collections.Generic;
using System.Text;
using RimWorld;
using TAE;
using TeleCore;
using TeleCore.Data.Events;
using Verse;
using Verse.Sound;

namespace TR
{
    ///TODO: Implement with underground resource layer
    public class Building_TiberiumGeyser : TRBuilding
    {
        private Building_TiberiumSpike tiberiumSpike;
        private Sustainer spraySustainer;
        private IntermittenFleckSprayer tibSprayer;

        private int sustainerStartTick = -1;

        //Values
        private float maxDepositValue = 0;
        private double depositValue = 0f;

        //
        private int burstTicksLeft = -1;
        private bool startEnum;

        //
        public static bool makePollutionGas = false;

        public float ContentPercent => (float)depositValue / maxDepositValue;
        public bool IsEmpty => depositValue <= 0;
        public bool Bursting => burstTicksLeft > 0;
        
        //FX
        public override bool? FX_ShouldThrowEffects(FXEffecterArgs args)
        {
            return tiberiumSpike.Spawned;
        }
        
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
                TRFleckMaker.ThrowTiberiumAirPuff(this.TrueCenter(), Map);
                
                if (Find.TickManager.TicksGame % 20 == 0)
                {
                    depositValue--;
                    GenTemperature.PushHeat(this, 40f);
                    var cell = this.OccupiedRect().ExpandedBy(1).RandomCell;
                    map.GetMapInfo<AtmosphericMapInfo>().TrySpawnGasAt(cell, SpreadingGasDefOf.TiberiumGas, Rand.Range(500, 1000));
                }
                
            }, StartSpray, EndSpray);
            if (respawningAfterLoad) return;

            depositValue = TRandom.Range(10000, 30000);
            maxDepositValue = TRandom.Range((uint)depositValue, 30000U);
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
            if (IsEmpty) return;

            if (tiberiumSpike == null)
            {
                tibSprayer.SprayerTick();

                //Refill
                if (Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
                {
                    if (depositValue < maxDepositValue)
                    {
                        depositValue += TRandom.Range(100, 450);
                    }
                }
            }
            else if (tiberiumSpike?.Spawned ?? false)
            {
                if (tiberiumSpike.IsPoweredOn())
                {
                    if (tiberiumSpike.TibComponent.Volume.TryAdd(TiberiumDefOf.TibGas, 0.25f, out var result))
                    {
                        depositValue -= result.Actual;
                    }
                }
                return;
            }

            //Do spray when not covered by a spike
            if (spraySustainer != null && Find.TickManager.TicksGame > sustainerStartTick + 1000)
            {
                TRLog.Warning("Tiberium Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
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

            /*
            yield return new Command_Action()
            {
                defaultLabel = "Make Gas",
                action = delegate
                {
                    foreach (var cel in this.OccupiedRect().ExpandedBy(1))
                    {
                        Map.Tiberium().GasGridInfo.SetValue(cel);
                    }
                    //makePollutionGas = !makePollutionGas;
                    //Map.Tiberium().AtmosphericInfo.AddDirect(Position.RandomAdjacentCell8Way(), 100);
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "Do Spread",
                action = delegate
                {
                    Map.Tiberium().GasGridInfo.DoSpreadOnce();
                }
            };
            */
        }
        

    }
}
