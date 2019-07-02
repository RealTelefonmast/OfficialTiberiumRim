using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SuppressionTower : FXThing
    {

        public Comp_Suppression Comp => GetComp<Comp_Suppression>();

        private void SendWave()
        {
            foreach (var c in Comp.Cells)
            {
                
            }
        }
    }

}
