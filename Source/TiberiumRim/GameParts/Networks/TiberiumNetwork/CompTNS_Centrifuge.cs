using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SpeedController
    {
        private float curSpeed;
        private float targetSpeed;
        private float friction;

        public float CurSpeed => curSpeed;

        public SpeedController(float initialSpeed, float friction)
        {
            this.curSpeed = initialSpeed;
            this.targetSpeed = initialSpeed;
            this.friction = friction;
        }

        public void SetTargetSpeed(float speed)
        {
            this.targetSpeed = speed;
        }

        public void Update()
        {
            float acceleration = (targetSpeed - curSpeed) * Mathf.Lerp(friction, 1, Mathf.Lerp(0, targetSpeed, curSpeed));
            curSpeed += acceleration * Mathf.Pow(1/60f, 2);
        }
    }

    public class SecondOrderSpeed
    {
        // Private variables
        private Vector3? xp; // The previous input value
        private Vector3? y, yd; // The current output value and its derivative
        private float _w, _z, _d, k1, k2, k3; // Constants used in the algorithm

        // Constructor
        public SecondOrderSpeed(float f, float z, float r, Vector3 x0)
        {
            // Compute constants
            _w = 2 * Mathf.PI * f; // Natural frequency
            _z = z; // Damping ratio
            _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1)); // Damping frequency
            k1 = z / (Mathf.PI * f); // Constant used in the algorithm
            k2 = 1 / (_w * _w); // Constant used in the algorithm
            k3 = r * z / _w; // Constant used in the algorithm

            // Initialize variables
            xp = x0; // Set the previous input value to the initial value
            y = x0; // Set the output value to the initial value
            yd = Vector3.zero; // Set the derivative of the output value to zero
        }

        // Update method
        public Vector3? Update(float T, Vector3 x, Vector3? xd = null)
        {
            // Compute the input derivative if it is not provided
            if (xd == null)
            {
                xd = (x - xp) / T; // Approximate the derivative with finite differences
                xp = x; // Store the current input value for the next iteration
            }

            // Compute stable values of k1 and k2
            float k1_stable, k2_stable;
            if (_w * T < _z)
            {
                // Underdamped case
                k1_stable = k1;
                k2_stable = Mathf.Max(k2, T * T / 2 + T * k1 / 2, T * k1);
            }
            else
            {
                // Overdamped or critically damped case
                float t1 = Mathf.Exp(-_z * _w * T);
                float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(T * _d) : (float) Math.Cosh(T * _d));
                float beta = t1 * t1;
                float t2 = T / (1 + beta - alpha);
                k1_stable = (1 - beta) * t2;
                k2_stable = T * t2;
            }

            // Update the output value and its derivative
            y = y + T * yd; // Integrate the output value
            yd = yd + T * (x + k3 * xd - y - k1_stable * yd) / k2_stable; // Update the derivative
            return y; // Return the current output value
        }
    }

    public class CompTNS_Centrifuge : Comp_TiberiumNetworkStructure
    {
        private float speedVal;
        private float desiredSpeed;
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
            new (0.5f, 0.3f),
            new (0.8f, 0.6f),
            new (1, 1),
        };

        readonly SimpleCurve shaderCurve = new()
        {
            new(0, 0),
            new(0.5f, 0),
            new(1,1),
        };

        //
        private SpeedController speedController;
        private SecondOrderSpeed secondOrderSpeed;
        
        private NetworkSubPart ChemicalComponent => this[TiberiumDefOf.ChemicalNetwork];

        public override string FX_GetHolderTag => "Centrifuge";
        
        public override float? FX_GetRotation(FXLayerArgs args)
        {
            return args.index switch
            {
                4 => CompFX.FXLayers[3].TrueRotation,
                _ => null
            };
        }

        public override float? FX_GetAnimationSpeedFactor(FXLayerArgs args)
        {
            return args.index switch
            {
                2 => speedVal,
                3 => speedVal,
                _ => null
            };
        }

        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return args.index switch
            {
                4 => TiberiumNetPart.Container.Color,
                _ => null,
            };
        }

        //
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            speedControl = new FloatControl(25, 4, AccCurve, DecCurve, OutCurve);
            speedController = new SpeedController(0, 0.5f); //new AngularSpeedController(initialSpeed: 0.0f, acceleration: 2.0f, friction: 0.2f, mass: 5.0f);
            secondOrderSpeed = new SecondOrderSpeed(5, 1, 1, Vector3.zero);
        }

        [TweakValue("CENTRIFUGE_BLEND", 0f, 1f)]
        public static float OverrideBlendValue = 0;

        private float BlendValue => OverrideBlendValue > 0 ? OverrideBlendValue : shaderCurve.Evaluate(speedControl.CurPct);

        //TiberiumComp.Container.StoredPercent >=
        private bool HasEnoughStored => TiberiumNetPart.RequestWorker.ShouldRequest && !Container.Empty;

        private bool ShouldWork
        {
            get
            {
                if (!HasEnoughStored) return false;
                foreach (var valueDef in TiberiumNetPart.Props.AllowedValuesByRole[NetworkRole.Requester])
                {
                    if (Container.StoredValueOf(valueDef) > 0)
                        return true;
                }
                return false;
            }
        }


        public override void CompTick()
        {
            base.CompTick();
        }

        private Vector3 speedVec;

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

            if (speedControl.CurState is FloatControl.FCState.Accelerating or FloatControl.FCState.Sustaining)
            {
                speedController.SetTargetSpeed(10);
            }
            else
            {
                speedController.SetTargetSpeed(0);
            }

            speedController.Update();
            speedVal = speedController.CurSpeed;

            //CompFX.FXLayers[2].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            //CompFX.FXLayers[3].PropertyBlock.SetFloat("_BlendValue", BlendValue);
            //CompFX.FXLayers[4].PropertyBlock.SetFloat("_BlendValue", BlendValue);
        }

        public bool useHarmonics = true;
        
        public override void NetworkPostTick(NetworkSubPart networkSubPart, bool isPowered)
        {
            StartOrSustainCentrifuge(isPowered);
            if (!isPowered) return;

            if (speedControl.ReachedPeak && processingBatch)
            {
                var storedTypes = TiberiumNetPart.Container.StoredDefs;
                for (int i = storedTypes.Count() - 1; i >= 0; i--)
                {
                    var storedType = storedTypes.ElementAt(i);
                    var values = ValuesFor(storedType);
                    if(values.NullOrEmpty()) continue;
                    if (TiberiumNetPart.Container.TryRemoveValue(storedType, 1f, out var result))
                    {
                        foreach (var type in values)
                        {
                            ChemicalComponent.Container.TryAddValue(type.valueDef, type.valueF * result.ActualAmount * 2, out _);
                            WasteNetPart.Container.TryAddValue(TiberiumDefOf.TibSludge, 0.125f, out _);
                        }
                    }

                    if (TiberiumNetPart.Container.Empty || ChemicalComponent.Container.Full)
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


            sb.AppendLine($"Speed {Math.Round(speedVal,1)} -> {Math.Round(desiredSpeed,1)}");
            
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "ToggleHarmonics",
                action = delegate { useHarmonics = !useHarmonics; }
            };
            
            yield return new Command_Action()
            {
                defaultLabel = $"Boost Speed",
                action = delegate { desiredSpeed += 1f; }
            };
            
            yield return new Command_Action()
            {
                defaultLabel = $"DeBoost Speed",
                action = delegate { desiredSpeed -= 1f;  }
                
            };
        }
    }
}
