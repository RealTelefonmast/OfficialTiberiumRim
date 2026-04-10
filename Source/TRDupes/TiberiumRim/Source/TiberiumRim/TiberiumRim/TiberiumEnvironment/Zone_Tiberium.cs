using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Zone_Tiberium : Zone
    {
        public HarvestType ZoneType = HarvestType.Unharvestable;

        protected override Color NextZoneColor => Color.green;

        public override void PostRegister()
        {
            base.PostRegister();
        }
    }
}
