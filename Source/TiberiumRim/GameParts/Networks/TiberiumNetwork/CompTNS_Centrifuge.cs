using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Centrifuge : Comp_TiberiumNetworkStructure
    {
        private FloatControl speedControl;
        private bool processingBatch;

        readonly SimpleCurve AccCurve = new()
        {
            new (0, 0),
            new (0.25f, 0.125f),
            new (0.5f, 0.35f),
            new (0.65f, 0.5f),
            new (1, 1),
        };

        readonly SimpleCurve DecCurve = new()
        {
            new (0, 0),
            new (0.75f, 0.5f),
            new (1, 0.75f),
        };

        readonly SimpleCurve OutCurve = new()
        {
            new (0, 0),
            new (0.5f, 3),
            new (0.8f, 6),
            new (1, 10),
        };

        readonly SimpleCurve shaderCurve = new()
        {
            new(0, 0),
            new(0.5f, 0),
            new(1,1),
        };

        private NetworkSubPart ChemicalComponent => this[TiberiumDefOf.ChemicalNetwork];

        public override float? FX_GetRotation(FXLayerArgs args)
        {
            return args.index switch
            {
                4 => CompFX.FXLayers[3].TrueRotation,
                _ => null
            };
        }

        public override float? FX_GetRotationSpeedOverride(FXLayerArgs args)
        {
            return args.index switch
            {
                2 => speedControl.OutputValue,
                3 => speedControl.OutputValue,
                _ => null
            };
        }

        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return args.index switch
            {
                4 => TiberiumComp.Container.Color,
                _ => null,
            };
        }

        //
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            speedControl = new FloatControl(25, 4, AccCurve, DecCurve, OutCurve);
        }

        [TweakValue("CENTRIFUGE_BLEND", 0f, 1f)]
        public static float OverrideBlendValue = 0;

        private float BlendValue => OverrideBlendValue > 0 ? OverrideBlendValue : shaderCurve.Evaluate(speedControl.CurPct);

        private bool HasEnoughStored => TiberiumComp.Container.StoredPercent >= TiberiumComp.RequestedCapacityPercent && !Container.Empty;

        private bool ShouldWork
        {
            get
            {
                if (!HasEnoughStored) return false;
                foreach (var valueDef in TiberiumComp.Props.AllowedValuesByRole[NetworkRole.Requester])
                {
                    if (Container.TotalStoredOf(valueDef) > 0)
                        return true;
                }
                return false;
            }
        }


        public override void CompTick()
        {
            base.CompTick();
        }

        private void StartOrSustainCentrifuge(bool isPowered)
        {
            if (!processingBatch || !isPowered)
            {
                speedControl.Stop();
                processingBatch = false;
            }
            if (isPowered && HasEnoughStored && !processingBatch && !ChemicalComponent.Container.Full)
            {
                //Start
                speedControl.Start();
                processingBatch = true;
            }
            speedControl.Tick();

            CompFX.FXLayers[2].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            CompFX.FXLayers[3].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            CompFX.FXLayers[4].PropertyBlock.SetFloat("_BlendValue", BlendValue);
        }

        public override void NetworkPostTick(NetworkSubPart networkSubPart, bool isPowered)
        {
            StartOrSustainCentrifuge(isPowered);
            if (!isPowered) return;

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
                            ChemicalComponent.Container.TryAddValue(type.valueDef, type.valueF * actualValue * 2, out _);
                            TiberiumComp.Container.TryAddValue(TiberiumDefOf.TibSludge, 0.125f, out _);
                        }
                    }

                    if (TiberiumComp.Container.Empty || ChemicalComponent.Container.Full)
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

            TRLog.Warning($"[TibExtractor] {def} does not have a value list!");
            return null;
        }

        private static readonly List<NetworkValue> GreenTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 1f/8),
            new NetworkValue(TiberiumDefOf.Carbon    , 1f/4),
            new NetworkValue(TiberiumDefOf.Iron      , 1f/4),
            new NetworkValue(TiberiumDefOf.Calcium   , 1f/8),
            new NetworkValue(TiberiumDefOf.Copper    , 1f/4),
            new NetworkValue(TiberiumDefOf.Silicon   , 1f/8),
            new NetworkValue(TiberiumDefOf.Exotic    , 1f/16)
        };

        private static readonly List<NetworkValue> BlueTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 1f / 8),
            new NetworkValue(TiberiumDefOf.Carbon    , 1f / 4),
            new NetworkValue(TiberiumDefOf.Iron      , 1f / 4),
            new NetworkValue(TiberiumDefOf.Calcium   , 1f / 8),
            new NetworkValue(TiberiumDefOf.Copper    , 1f / 4),
            new NetworkValue(TiberiumDefOf.Silicon   , 1f / 8),
            new NetworkValue(TiberiumDefOf.Exotic    , 1f / 16),
            new NetworkValue(TiberiumDefOf.Silver    , 1f / 8)
        };

        private static readonly List<NetworkValue> RedTibValues = new()
        {
            new NetworkValue(TiberiumDefOf.Phosphorus, 1f / 8),
            new NetworkValue(TiberiumDefOf.Carbon    , 1f / 4),
            new NetworkValue(TiberiumDefOf.Iron      , 1f / 4),
            new NetworkValue(TiberiumDefOf.Calcium   , 1f / 8),
            new NetworkValue(TiberiumDefOf.Copper    , 1f / 4),
            new NetworkValue(TiberiumDefOf.Silicon   , 1f / 8),
            new NetworkValue(TiberiumDefOf.Exotic    , 1f / 16),
            new NetworkValue(TiberiumDefOf.Gold      , 1f / 8),
            new NetworkValue(TiberiumDefOf.Uranium   , 1f / 8)
        };

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder(base.CompInspectStringExtra());
            sb.AppendLine();

            var spdMaxVal = Math.Round(25f, 2);
            var spdCurState = speedControl.CurState;
            var spdAcceleration = Math.Round(speedControl.Acceleration, 2);
            var spdCurValue = Math.Round(speedControl.CurValue, 2);
            var spdCurPct = speedControl.CurPct.ToStringPercent();
            var spdOutputVal = Math.Round(speedControl.OutputValue, 2);

            sb.AppendLine($"VALUE\t|ACCELERATOR\n" +
                          $"MaxVal\t|{spdMaxVal}\n" +
                          $"State\t|{spdCurState}\n" +
                          $"Accel.\t|{spdAcceleration}\n" +
                          $"CurValue\t|{spdCurValue}\n" +
                          $"CurPct\t|{spdCurPct}\n" +
                          $"Output\t|{spdOutputVal}\n");

            sb.Append($"Shader BlendValue: {BlendValue}");
            return sb.ToString().TrimStart().TrimEndNewlines();
        }
    }
}
