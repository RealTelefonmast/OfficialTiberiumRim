﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class Building_IonCannon : TRBuilding
    {
        public override bool[] DrawBools => new bool[2]{true, CentralLight};

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            AttackSatellite_Ion asat = (AttackSatellite_Ion) WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("ASat_Ion"));
            asat.Tile = Tile;
            Find.WorldObjects.Add(asat);
        }

        public bool CentralLight => true;

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("Current ASATS: " + TiberiumRimComp.SatelliteInfo.AttackSatelliteNetwork.ASatsIon.Count);
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.godMode)
            {

            }
        }
    }
}
