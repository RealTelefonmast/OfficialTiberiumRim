using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_TurretHub : Building_TRTurret
    {
        public List<Building_HubTurret> hubTurrets = new List<Building_HubTurret>();

        public void Upgrade_AddTurret()
        {

        }


        public void AddHubTurret(Building_HubTurret t)
        {
            if (!hubTurrets.Contains(t))
            {
                hubTurrets.Add(t);
                t.parentHub = this;
            }
        }
        
        public Building_HubTurret DestroyedChild => hubTurrets.First(c => c.NeedsRepair);

        public bool NeedsTurrets => hubTurrets.Count < 3;

        public override void Draw()
        {
            base.Draw();
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
        }
    }
}
