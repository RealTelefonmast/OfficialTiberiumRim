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

        SimpleCurve Curve = new SimpleCurve()
        {
            new (0, 0),
            new (0.5f, 3),
            new (0.8f, 6),
            new (1, 10),
        };

        private SimpleCurve shaderCurve = new SimpleCurve()
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

        public override void CompTick()
        {
            base.CompTick();
            if (!processingBatch)
            {
                accelerator.Stop();
            }
            if (TiberiumComp.Container.StoredPercent >= 0.5f && !processingBatch && !ChemicalComponent.Container.CapacityFull)
            {
                //Start
                accelerator.Start();
                processingBatch = true;
            }

            accelerator.Tick();
            speedControl.Tick();

            CompFX.Overlays[2].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            CompFX.Overlays[3].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            //_Samples
            CompFX.Overlays[4].PropertyBlock.SetFloat("_Samples", Mathf.CeilToInt(Mathf.Lerp(1,360, BlendValue)));
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

        protected override void NetworkTickCustom(bool isPowered)
        {
            if (speedControl.ReachedPeak && processingBatch && isPowered)
            {
                var storedTypes = TiberiumComp.Container.AllStoredTypes;
                //foreach (var storedType in TiberiumComp.Container.AllStoredTypes)
                for (int i = storedTypes.Count() - 1; i >= 0; i--)
                {
                    var storedType = storedTypes.ElementAt(i);
                    if (TiberiumComp.Container.TryRemoveValue(storedType, 1f, out float actualValue))
                    {
                        var types = ValuesFor(new NetworkValue(storedType, actualValue));
                        foreach (var type in types)
                        {
                            ChemicalComponent.Container.TryAddValue(type.valueDef, type.valueF, out _);
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

        private List<NetworkValue> ValuesFor(NetworkValue value)
        {

            if (value.valueDef == TiberiumDefOf.TibBlue)
            {
                return new List<NetworkValue>()
                {
                   
                    new NetworkValue(TiberiumDefOf.Phosphorus, 0.425f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Iron,       0.325f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Calcium,    0.1525f * value.valueF),
                    new NetworkValue(TiberiumDefOf.Copper,     0.0575f * value.valueF),
                    new NetworkValue(TiberiumDefOf.Silicon,    0.025f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Exotic,     0.015f  * value.valueF),
                };
            }
            if (value.valueDef == TiberiumDefOf.TibRed)
            {
                return new List<NetworkValue>()
                {
                    new NetworkValue(TiberiumDefOf.Phosphorus, 0.425f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Iron,       0.325f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Calcium,    0.1525f * value.valueF),
                    new NetworkValue(TiberiumDefOf.Copper,     0.0575f * value.valueF),
                    new NetworkValue(TiberiumDefOf.Silicon,    0.025f  * value.valueF),
                    new NetworkValue(TiberiumDefOf.Exotic,     0.015f  * value.valueF),
                };
            }

            return new List<NetworkValue>()
            {
                new NetworkValue(TiberiumDefOf.Phosphorus, 0.425f  * value.valueF),
                new NetworkValue(TiberiumDefOf.Iron,       0.325f  * value.valueF),
                new NetworkValue(TiberiumDefOf.Calcium,    0.1525f * value.valueF),
                new NetworkValue(TiberiumDefOf.Copper,     0.0575f * value.valueF),
                new NetworkValue(TiberiumDefOf.Silicon,    0.025f  * value.valueF),
                new NetworkValue(TiberiumDefOf.Exotic,     0.015f  * value.valueF),
            };

        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());
            sb.AppendLine();

            //Accelerator

            sb.AppendLine($"{"VALUE",5}|{"ACCELERATOR",5}|{"SPEEDER",5}|\n" +
                          $"{"MaxVal",5}|{accelerator.FC.MaxVal,5}|{speedControl.MaxVal,5}|\n" +
                          $"{"State",5}|{accelerator.FC.CurState,5}|{speedControl.CurState,5}|\n" +
                          $"{"Accel.",5}|{accelerator.FC.Acceleration,5}|{speedControl.Acceleration,5}|\n" +
                          $"{"CurValue",5}|{accelerator.FC.CurValue,5}|{speedControl.CurValue,5}|\n" +
                          $"{"CurPct",5}|{accelerator.FC.CurPct,5}|{speedControl.CurPct,5}|\n" +
                          $"{"Output",5}|{accelerator.FC.OutputValue,5}|{speedControl.OutputValue,5}|\n");

            sb.Append($"Shader BlendValue: {BlendValue}");
            return sb.ToString().TrimEndNewlines();
        }
    }
}
