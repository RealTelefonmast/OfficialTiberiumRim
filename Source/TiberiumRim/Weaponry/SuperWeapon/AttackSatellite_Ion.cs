using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class AttackSatellite_Ion : AttackSatellite, IMapWatcher
    {
        private GlobalTargetInfo target = GlobalTargetInfo.Invalid;

        public override void Draw()
        {
            base.Draw();
        }

        private bool ShouldAttack => target.IsMapTarget && (Tile == target.Tile && tileDest == target.Tile);
        private bool ShouldMove => target.IsWorldTarget;

        public bool IsSpyingNow => true;
        public Map MapTarget => Find.World.worldObjects.MapParentAt(Tile)?.Map;

        //TODO: Add Ion Cannon Beacon to target cell
        //Otherwise use comm satallite and console with pawn to use targeter
        public void SetAttackDest(Map map, IntVec3 cell)
        {
            target = new GlobalTargetInfo(cell, map);
        }

        public override void Tick()
        {
            base.Tick();
            if (!ShouldAttack) return;
            ActionComposition composition = new ActionComposition("Ion Cannon Satellite Action");
            composition.CacheMap(target);
            composition.AddPart(delegate
            {
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.IonCannonActivated, null);
                composition.target.Map.weatherManager.TransitionTo(WeatherDef.Named("Rain"));
            },0);
            composition.AddPart(delegate
            {
                TiberiumFX.DoFloatingEffectsInRadius(composition.target.Cell, composition.target.Map, IonCannon_Strike.radius, 8, false, new IntRange(5, 15), new IntRange(4, 8), new IntRange(3,6));
            },0.5f);
            composition.AddPart(delegate
            {
                //TODO: Reset cooldown, notify ion cannon center
                IonCannon_Strike strike = (IonCannon_Strike)ThingMaker.MakeThing(ThingDef.Named("IonCannonStrike"));
                strike.satellite = this;
                GenSpawn.Spawn(strike, composition.target.Cell, composition.target.Map);
            }, 2);
            /*
            composition.AddPart(delegate
            {
                composition.target.Map.weatherManager.TransitionTo(WeatherDef.Named("Fog"));
            },22);
            */
            composition.Init();
            target = GlobalTargetInfo.Invalid;
        }

        public bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if (target.IsMapTarget && target.Map.IsPlayerHome) return false;

            return true;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = "Set Target",
                //icon = MissileSilo.LaunchWorldTex,
                action = delegate ()
                {
                    CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
                    Find.WorldSelector.ClearSelection();
                    Find.WorldTargeter.BeginTargeting((this.ChoseWorldTarget), true);
                }
            };

            if (Find.World.worldObjects.AnySettlementAt(Tile))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Spy",
                    //icon = MissileSilo.LaunchWorldTex,
                    action = delegate()
                    {
                        Settlement settlement = Find.World.worldObjects.SettlementAt(Tile);
                        if (!settlement.HasMap)
                        {
                            LongEventHandler.QueueLongEvent(delegate ()
                            {
                                LoadMap(settlement);
                            }, "GeneratingMapForNewEncounter", false, null, true);
                            return;
                        }
                        LoadMap(settlement);
                    }
                };
            }
        }

        private void LoadMap(Settlement settlement)
        {
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(settlement));

            if (!CameraJumper.TryHideWorld() && Find.CurrentMap != orGenerateMap)
            {
                SoundDefOf.MapSelected.PlayOneShotOnCamera(null);
            }
            Current.Game.CurrentMap = orGenerateMap;
        }
    }
}
