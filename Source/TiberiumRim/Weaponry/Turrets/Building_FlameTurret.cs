using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using TeleCore.Data.Events;
using UnityEngine;
using Verse;

namespace TR
{
    public class Building_FlameTurret : Building_HubTurret
    {
        private IntVec3 target;
        private bool[] directions = new bool[2] {false, false};
        private bool settingFireWall = false;

        private int swayTicksDone = 0;
        private static int swayTicks = 200;

        private int growthTicks = 0;
        //private int growthDuration = 1500;


        public float curDegreeOff;
        private Vector3 distanceVector;

        private LocalTargetInfo fireWallPos = LocalTargetInfo.Invalid;
        private float Range => MainGun.AttackVerb.EffectiveRange;

        //FX
        public override bool FX_ProvidesForLayer(FXArgs args)
        {
            if (args.layerTag == "FXFlameTurret")
                return true;
            return base.FX_ProvidesForLayer(args);
        }

        public override float? FX_GetRotation(FXLayerArgs args)
        {
            return args.index switch
            {
                2 => MainGun?.TurretRotation,
                3 => MainGun?.TurretRotation,
                _ => null
            };
        }

        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                1 => false,
                3 => HoldingFire,
                _ => true
            };
        }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            target = IntVec3.Invalid;

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref fireWallPos, "fireWallPos");
        }

        [TweakValue("[TR]FlameTurretGrowth", 100, 2000)]
        public static int growthDuration = 1000;

        //TODO: Add simple firewall, no growth
        public void TargetLocTick()
        {
            if (!fireWallPos.IsValid) return;

            curDegreeOff = ((swayTicksDone / (float) swayTicks) - 0.5f) * 90f;
            target = (DrawPos + (Quaternion.Euler(0, curDegreeOff, 0) * (distanceVector * Mathf.Clamp01((growthTicks / (float) growthDuration) + 0.4f)))).ToIntVec3();
            
            if (directions[0] && swayTicksDone >= swayTicks)
                directions[0] = false;

            if (!directions[0] && swayTicksDone <= 0)
                directions[0] = true;

            swayTicksDone += directions[0] ? 1 : -1;


            if (directions[1] && growthTicks >= growthDuration)
                directions[1] = false;

            if (!directions[1] && growthTicks <= 0)
                directions[1] = true;

            growthTicks += directions[1] ? 1 : -1;
        }

        public override void Tick()
        {
            base.Tick();
            if (HoldingFire || !fireWallPos.IsValid) return;
            TargetLocTick();
            OrderAttack(target);
            //this.AttackVerb.TryStartCastOn(CurrentTarget, false, true);

            //OnAttackedTarget(CurrentTarget);
            //OrderAttack(target);
        }

        public override string GetInspectString()
        {
            return "Direction: " + directions[0] + "\n" + "Tick: " + swayTicksDone + " / " + swayTicks + "\nPct: " +
                   ((float) swayTicksDone / (float) swayTicks).ToStringPercent() + "\nRadOff: " + curDegreeOff;
            //return base.GetInspectString();
        }

        protected override void OnResetOrderedAttack()
        {
            fireWallPos = LocalTargetInfo.Invalid;
        }

        public override void Draw()
        {
            base.Draw();
            if (settingFireWall)
            {
                GenDraw.DrawFieldEdges(CellGen.SectorCells(Position, Map, Range, 90f, (DrawPos.AngleToFlat(UI.MouseMapPosition()) + 90).AngleWrapped(), false).ToList());
            }

            if (Find.Selector.IsSelected(this))
            {
                GenDraw.DrawTargetHighlight(fireWallPos);
                GenDraw.DrawLineBetween(DrawPos, target.ToVector3Shifted());
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = "FireWall",
                action = delegate
                {
                    settingFireWall = true;
                    swayTicksDone = 0;
                    growthTicks = 0;
                    Find.Targeter.BeginTargeting(new TargetingParameters
                        {
                            canTargetBuildings = true,
                            canTargetFires = false,
                            canTargetItems = true,
                            canTargetLocations = true,
                            canTargetPawns = true,
                            canTargetSelf = false,
                        },
                        delegate(LocalTargetInfo target)
                        {
                            settingFireWall = false;
                            var from = Position;
                            var to = target.Cell;
                            var distance = from.DistanceTo(to);

                            var range = Range - 1.49f;
                            if (distance < range)
                            {
                                var normed = (to - from).ToVector3().normalized;
                                IntVec3 newTo = from + (normed * range).ToIntVec3();
                                to = newTo;
                            }
                            fireWallPos = to;
                            distanceVector = (fireWallPos.Cell - Position).ToVector3Shifted();

                        }, null, null, null);
                }
            };
        }
    }
}
