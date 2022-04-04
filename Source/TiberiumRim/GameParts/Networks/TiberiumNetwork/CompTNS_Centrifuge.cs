using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Centrifuge : Comp_TiberiumNetworkStructure
    {
        private FCAccelerator accelerator;
        private FC speedControl;

        private bool processingBatch;

        readonly SimpleCurve Curve = new SimpleCurve()
        {
            new (0, 0),
            new (0.5f, 3),
            new (0.8f, 6),
            new (1, 10),
        };

        private readonly SimpleCurve shaderCurve = new SimpleCurve()
        {
            new(0, 0),
            new(0.5f, 0),
            new(1,1),
        };

        private NetworkComponent ChemicalComponent => this[TiberiumDefOf.ChemicalNetwork];

        public override float?[] AnimationSpeeds => new float?[6] {null, null, speedControl.OutputValue, speedControl.OutputValue, null, null};
        public override Color[] ColorOverrides => new Color[6] { Color.white, Color.white, Color.white, Color.white, TiberiumComp.Container.Color, Color.white};
        public override float?[] RotationOverrides => new float?[6] {null, null, null, null, CompFX.Overlays[3].Rotation, null};

        //
        private float MaxValue()
        {
            return 25f;
        }

        private float AccValue()
        {
            if (FCState() == TiberiumRim.FCState.Decelerating)
                return 10f;
            return accelerator.FC.CurValue;
        }

        private FCState FCState()
        {
            var accState = accelerator.AcceleratorState();
            if (accState is TiberiumRim.FCState.Accelerating or TiberiumRim.FCState.Sustaining)
            {
                return TiberiumRim.FCState.Accelerating;
            }
            if (accState is TiberiumRim.FCState.Decelerating)
            {
                return TiberiumRim.FCState.Decelerating;
            }
            return TiberiumRim.FCState.Sustaining;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            accelerator = new FCAccelerator(MaxValue()/4f, 4f);
            speedControl = new FC(MaxValue, AccValue, FCState, Curve);
        }

        [TweakValue("CENTRIFUGE_BLEND", 0f, 1f)]
        public static float OverrideBlendValue = 0;

        private float BlendValue => OverrideBlendValue > 0 ? OverrideBlendValue : shaderCurve.Evaluate(speedControl.CurPct);

        private bool HasEnoughStored => TiberiumComp.Container.StoredPercent >= TiberiumComp.RequestedCapacityPercent && !Container.Empty;

        public override void CompTick()
        {
            base.CompTick();
            //_Samples
            //CompFX.Overlays[4].PropertyBlock.SetFloat("_Samples", Mathf.CeilToInt(Mathf.Lerp(1,360, BlendValue)));
            //CompFX.Overlays[2].Graphic.MatSingle.SetFloat("_BlendValue", BlendValue);


            /*
            if (currentSpeedUpTick > speedUpTicks || currentIdleTicks > 0)
            {
                if (currentIdleTicks < idleTime)
                {
                    currentIdleTicks++;
                }
                else
                {
                    currentSpeedUpTick--;
                    if (currentSpeedUpTick == 0)
                        currentIdleTicks = 0;
                }
            }
            else
                currentSpeedUpTick++;

            speedInt = Curve.Evaluate((float)currentSpeedUpTick / speedUpTicks);
            */
        }

        private void StartOrSustainCentrifuge(bool isPowered)
        {
            if (!processingBatch || !isPowered)
            {
                accelerator.Stop();
                processingBatch = false;
            }
            if (isPowered && HasEnoughStored && !processingBatch && !ChemicalComponent.Container.CapacityFull)
            {
                //Start
                accelerator.Start();
                processingBatch = true;
            }

            accelerator.Tick();
            speedControl.Tick();

            CompFX.Overlays[2].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            CompFX.Overlays[3].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            CompFX.Overlays[4].PropertyBlock.SetFloat("_BlendValue", BlendValue);
        }

        protected override void NetworkTickCustom(bool isPowered)
        {
            StartOrSustainCentrifuge(isPowered);
            if (!isPowered) return;
            if (TiberiumComp.RequesterMode == RequesterMode.Automatic)
            {
                //Resolve..
                var maxVal = TiberiumComp.RequestedCapacityPercent * Container.Capacity;

                foreach (var valType in TiberiumComp.Container.AcceptedTypes)
                {
                    var valTypeValue = Container.ValueForType(valType) + TiberiumComp.Network.NetworkValueFor(valType, NetworkRole.Storage);
                    if (valTypeValue > 0)
                    {
                        var setValue = Mathf.Min(maxVal, valTypeValue);
                        TiberiumComp.RequestedTypes[valType] = setValue;
                        maxVal = Mathf.Clamp(maxVal - setValue, 0, maxVal);
                    }
                }
            }

            if (speedControl.ReachedPeak && processingBatch)
            {
                var storedTypes = TiberiumComp.Container.AllStoredTypes;
                for (int i = storedTypes.Count() - 1; i >= 0; i--)
                {
                    var storedType = storedTypes.ElementAt(i);
                    var values = ValuesFor(storedType);
                    if(values.NullOrEmpty()) continue;
                    if (TiberiumComp.Container.TryRemoveValue(storedType, 1f, out float actualValue))
                    {
                        foreach (var type in values)
                        {
                            ChemicalComponent.Container.TryAddValue(type.valueDef, type.valueF * actualValue, out _);
                        }
                    }

                    if (TiberiumComp.Container.Empty || ChemicalComponent.Container.CapacityFull)
                    {
                        processingBatch = false;
                        break;
                    }
                }
            }
        }

        private List<NetworkValue> ValuesFor(NetworkValueDef def)
        {
            if (def == TiberiumDefOf.TibGreen)
                return GreenTibValues;
            if (def == TiberiumDefOf.TibBlue)
                return BlueTibValues;
            if (def == TiberiumDefOf.TibRed)
                return RedTibValues;

            TLog.Warning($"[TibExtractor] {def} does not have a value list!");
            return null;
        }

        private static readonly List<NetworkValue> GreenTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 0.275f),
            new NetworkValue(TiberiumDefOf.Carbon, 0.25f),
            new NetworkValue(TiberiumDefOf.Iron, 0.325f),
            new NetworkValue(TiberiumDefOf.Calcium, 0.1525f),
            new NetworkValue(TiberiumDefOf.Copper, 0.0575f),
            new NetworkValue(TiberiumDefOf.Silicon, 0.025f),
            new NetworkValue(TiberiumDefOf.Exotic, 0.015f)
        };

        private static readonly List<NetworkValue> BlueTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 0.275f),
            new NetworkValue(TiberiumDefOf.Carbon, 0.25f),
            new NetworkValue(TiberiumDefOf.Iron, 0.325f),
            new NetworkValue(TiberiumDefOf.Calcium, 0.1525f),
            new NetworkValue(TiberiumDefOf.Copper, 0.0575f),
            new NetworkValue(TiberiumDefOf.Silicon, 0.025f),
            new NetworkValue(TiberiumDefOf.Exotic, 0.015f),
            new NetworkValue(TiberiumDefOf.Silver, 0.0125f)
        };

        private static readonly List<NetworkValue> RedTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 0.275f),
            new NetworkValue(TiberiumDefOf.Carbon, 0.25f),
            new NetworkValue(TiberiumDefOf.Iron, 0.325f),
            new NetworkValue(TiberiumDefOf.Calcium, 0.1525f),
            new NetworkValue(TiberiumDefOf.Copper, 0.0575f),
            new NetworkValue(TiberiumDefOf.Silicon, 0.025f),
            new NetworkValue(TiberiumDefOf.Exotic, 0.015f),
            new NetworkValue(TiberiumDefOf.Gold, 0.025f),
            new NetworkValue(TiberiumDefOf.Uranium, 0.015f)
        };

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());
            sb.AppendLine();

            //Accelerator
            var accMaxVal = Math.Round(accelerator.FC.MaxVal, 2);
            var accCurState = accelerator.FC.CurState;
            var accAcceleration = Math.Round(accelerator.FC.Acceleration, 2); 
            var accCurValue = Math.Round(accelerator.FC.CurValue, 2); 
            var accCurPct = accelerator.FC.CurPct.ToStringPercent(); 
            var accOutputVal = Math.Round(accelerator.FC.OutputValue, 2);

            var spdMaxVal = Math.Round(speedControl.MaxVal, 2);
            var spdCurState = speedControl.CurState;
            var spdAcceleration = Math.Round(speedControl.Acceleration, 2);
            var spdCurValue = Math.Round(speedControl.CurValue, 2);
            var spdCurPct = speedControl.CurPct.ToStringPercent();
            var spdOutputVal = Math.Round(speedControl.OutputValue, 2);

            sb.AppendLine($"VALUE\t|ACCELERATOR\t|SPEEDER\t|\n" +
                          $"MaxVal\t|{accMaxVal}\t\t|{spdMaxVal}\t\t|\n" +
                          $"State\t|{accCurState}\t|{spdCurState}\t|\n" +
                          $"Accel.\t|{accAcceleration}\t\t|{spdAcceleration} \t\t|\n" +
                          $"CurValue\t|{accCurValue}\t\t|{spdCurValue}\t\t|\n" +
                          $"CurPct\t|{accCurPct}\t\t|{spdCurPct}\t\t|\n" +
                          $"Output\t|{accOutputVal}\t\t|{spdOutputVal}\t\t|\n");

            sb.Append($"Shader BlendValue: {BlendValue}");
            return sb.ToString().TrimEndNewlines();
        }
    }
}
