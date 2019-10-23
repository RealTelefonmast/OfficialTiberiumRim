using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AttackSatellite_Ion : AttackSatellite
    {
        private GlobalTargetInfo target = GlobalTargetInfo.Invalid;

        public override void Draw()
        {
            base.Draw();
        }

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
            ActionComposition composition = new ActionComposition();
            composition.CacheMap(target);
            composition.AddPart(delegate
            {
                composition.target.Map.weatherManager.TransitionTo(WeatherDef.Named("Rain"));
            },0);
            composition.AddPart(delegate
            {
                //TODO: yes but actually no
                IonCannon_Strike strike = (IonCannon_Strike)ThingMaker.MakeThing(ThingDef.Named("IonCannonStrike"));
                strike.satellite = this;
                GenSpawn.Spawn(strike, composition.target.Cell, composition.target.Map);
            }, 10);
            composition.AddPart(delegate
            {
                composition.target.Map.weatherManager.TransitionTo(WeatherDef.Named("Fog"));
            },22);
            composition.Init();
            target = GlobalTargetInfo.Invalid;
        }

        private bool ShouldAttack => target.IsMapTarget && (Tile == target.Tile && tileDest == target.Tile);
    }
}
