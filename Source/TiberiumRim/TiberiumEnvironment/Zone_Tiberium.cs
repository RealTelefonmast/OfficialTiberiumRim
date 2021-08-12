using UnityEngine;
using Verse;

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
