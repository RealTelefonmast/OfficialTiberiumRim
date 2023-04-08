﻿using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class Veinhole : TiberiumProducer
    {
        private VeinholeNetwork livingNetwork;
        private const int hubRadius = 70;
        private int ticksToHub = 0;
        private int ticksToEgg = 0;

        private int nutrients = 0;

        private List<Thing> boundHubs = new List<Thing>();
        private Comp_AnimationRenderer animationCompInt;

        public Comp_AnimationRenderer AnimationComp => animationCompInt;
        public VeinholeNetwork LivingNetwork => livingNetwork;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            //
            livingNetwork = new VeinholeNetwork(this);
            
            //Shake the camera!
            Find.CameraDriver.shaker.DoShake(0.2f);

            ResetEggTimer();
            ResetHubTimer();
            base.SpawnSetup(map, respawningAfterLoad);

            animationCompInt = this.GetComp<Comp_AnimationRenderer>();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToHub, "hubTicks");
            Scribe_Values.Look(ref ticksToEgg, "eggTicks");
            Scribe_Deep.Look(ref livingNetwork, "livingNetwork");
            base.ExposeData();
        }

        public override void Tick()
        {
            base.Tick();
            TrySpawnHub();
            TrySpawnEgg();
        }

        private void TryConsume(Pawn pawn)
        {

        }

        private void TrySpawnHub()
        {
            if (ticksToHub != 0) return;

            Action<IntVec3> Processor = delegate(IntVec3 c)
            {
                TerrainDef terrain = Ruleset.RandomOutcome(c.GetTerrain(Map));
                if (terrain != null)
                    Map.terrainGrid.SetTerrain(c, terrain);
            };

            IntVec3 end = GenRadial.RadialCellsAround(Position, 56, false).RandomElement();
            _ = TeleFlooder.TryMakeConnection(Position, end, Processor);
            var hub = GenSpawn.Spawn(ThingDef.Named("VeinHub"), end, Map);
            boundHubs.Add(hub);

            ResetHubTimer();
        }

        public void RemoveHub(VeinHub hub)
        {
            if (boundHubs.Contains(hub))
                boundHubs.Remove(hub);
        }

        private void TrySpawnEgg()
        {
            if (ticksToEgg != 0) return;

            var cell = FieldCells.RandomElement();
            GenSpawn.Spawn(ThingDef.Named("VeinEgg"), cell, Map);

            ResetEggTimer();
        }

        private void ResetHubTimer()
        {
            ticksToHub = (int)(GenDate.TicksPerDay * TRandom.Range(3f, 7f));
        }

        private void ResetEggTimer()
        {
            ticksToEgg = (int)(GenDate.TicksPerDay * TRandom.Range(1f, 3f));
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {

            foreach(Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            
            yield return new Command_Action{
                defaultLabel= "Spawn Hub",
                action = TrySpawnHub
            };

        }
    }
}