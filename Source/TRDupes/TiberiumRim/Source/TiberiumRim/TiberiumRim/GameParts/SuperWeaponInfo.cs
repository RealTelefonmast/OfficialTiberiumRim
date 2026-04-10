using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class SuperWeaponProperties
    {
        public float chargeTime;
        public Type designator;
        public Type worker = typeof(SuperWeaponInfo);

        private Designator resolvedDesignator;

        public Designator ResolvedDesignator
        {
            get
            {
                if (resolvedDesignator == null)
                {
                    resolvedDesignator = (Designator)Activator.CreateInstance(designator);
                }
                return this.resolvedDesignator;
            }
        }
    }

    public class SuperWeaponInfo
    {
        public TRBuilding building;
        public int ticksUntilReady;

        public virtual bool Active => building.DestroyedOrNull() && IsPowered;

        public virtual bool CanFire => ticksUntilReady <= 0;

        public bool IsPowered => ((CompPowerTrader) building.PowerComp).PowerOn;
    }
}
