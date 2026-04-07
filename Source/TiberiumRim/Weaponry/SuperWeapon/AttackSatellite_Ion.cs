using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using TeleCore.ActionCompositions;
using TR.Defs;
using TR.GameParts.EVA;
using TR.GameParts.MapWatchers;
using TR.Util.Effects;
using Verse;
using Verse.Sound;

namespace TR.Weaponry.SuperWeapon;

public class AttackSatellite_Ion : AttackSatellite, IMapWatcher
{
    private GlobalTargetInfo target = GlobalTargetInfo.Invalid;

    private bool ShouldAttack => target.IsMapTarget && Tile == target.Tile && tileDest == target.Tile;

    public override void Draw()
    {
        base.Draw();
    }

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
        var composition = new ActionComposition("Ion Cannon Satellite Action");
        composition.CacheMap(target);
        composition.AddPart(delegate
        {
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.IonCannonActivated, null);
            composition.target.Map.weatherManager.TransitionTo(WeatherDef.Named("Rain"));
            Notify_Fired();
        }, 0);
        composition.AddPart(
            delegate
            {
                TiberiumFX.DoFloatingEffectsInRadius(composition.target.Cell, composition.target.Map,
                    IonCannon_Strike.radius, 8, false, new IntRange(5, 15), new IntRange(4, 8), new IntRange(3, 6));
            }, 0.5f);
        composition.AddPart(delegate
        {
            //TODO: Reset cooldown, notify ion cannon center
            var strike = (IonCannon_Strike)ThingMaker.MakeThing(TRFDefOf.IonCannonStrike);
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
        if (target.IsWorldTarget)
        {
            Tile = target.Tile;
            SetDestination(Tile);
            return true;
        }

        return true;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            defaultLabel = "Set Target",
            //icon = MissileSilo.LaunchWorldTex,
            action = delegate
            {
                CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
                Find.WorldSelector.ClearSelection();
                Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, true);
            }
        };

        if (Find.World.worldObjects.AnySettlementAt(Tile))
            yield return new Command_Action
            {
                defaultLabel = "Spy",
                //icon = MissileSilo.LaunchWorldTex,
                action = delegate
                {
                    var settlement = Find.World.worldObjects.SettlementAt(Tile);
                    if (!settlement.HasMap)
                    {
                        LongEventHandler.QueueLongEvent(delegate { LoadMap(settlement); },
                            "GeneratingMapForNewEncounter", false, null);
                        return;
                    }

                    LoadMap(settlement);
                }
            };
    }

    private void LoadMap(Settlement settlement)
    {
        var orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
        CameraJumper.TryJump(CameraJumper.GetWorldTarget(settlement));

        if (!CameraJumper.TryHideWorld() && Find.CurrentMap != orGenerateMap)
            SoundDefOf.MapSelected.PlayOneShotOnCamera();
        Current.Game.CurrentMap = orGenerateMap;
    }
}