﻿using System;
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
            ActionComposition composition = new ActionComposition("Ion Cannon Satellite Action");
            composition.CacheMap(target);
            composition.AddPart(delegate
            {
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.IonCannonActivated);
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

        private bool ShouldAttack => target.IsMapTarget && (Tile == target.Tile && tileDest == target.Tile);
    }
}
