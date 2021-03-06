﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public enum PortalType
    {
        Drones,
        Reinforcement
    }

    public static class GenPortal
    {
        public static ScrinPortal MakePortal()
        {
            return (ScrinPortal) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ScrinPortal"));
        }

        public static ScrinPortal SpawnPortal(IntVec3 cell, Map map)
        {
           return (ScrinPortal)GenSpawn.Spawn(MakePortal(), cell, map);
        }

        public static ScrinPortal SpawnDronePortal(IntVec3 cell, Map map)
        {
            //TODO: make portal with stuff
            ScrinPortal portal = MakePortal();
            portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
            portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
            portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
            portal.PortalSetup(1000, 400);
            return (ScrinPortal) GenSpawn.Spawn(portal, cell, map);
        }
    }
}
