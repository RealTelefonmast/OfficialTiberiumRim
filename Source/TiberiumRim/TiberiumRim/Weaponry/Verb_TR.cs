using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Verb_TR : Verb_LaunchProjectile
    {
        public CompTNW_Turret TiberiumComp => caster.TryGetComp<CompTNW_Turret>();

        public VerbProperties_TR Props => (VerbProperties_TR)base.verbProps;

        public bool IsLaser => Props.defaultProjectile.thingClass == typeof(Projectile_Beam);

        protected override int ShotsPerBurst => this.verbProps.burstShotCount;

        public override bool Available()
        {
            if(Props.powerConsumption > 0)
            {

            }
            if (Props.tiberiumCostPerBurst != null)
            {
                return Props.tiberiumCostPerBurst.CanPay(TiberiumComp.Container);
            }
            if(Props.tiberiumCostPerShot != null)
            {
                return Props.tiberiumCostPerShot.CanPay(TiberiumComp.Container);
            }
            return base.Available();
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            if (Props.tiberiumCostPerBurst != null)
            {
                Props.tiberiumCostPerBurst.Pay(TiberiumComp.Container);
            }
        }     

        protected override bool TryCastShot()
        {
            bool flag = base.TryCastShot();
            if(flag && Props.tiberiumCostPerShot != null)
            {
                if (Props.tiberiumCostPerShot.CanPay(TiberiumComp.Container))
                    Props.tiberiumCostPerShot.Pay(TiberiumComp.Container);
                else
                    return false;
            }
            if (flag && base.CasterIsPawn)
            {
                base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }
            return flag;
        }

        public bool TryCastLaser()
        {
            return true;
        }

        public bool TryCastTiberium()
        {
            return true;
        }
        
    }

    public class VerbProperties_TR : VerbProperties
    {
        public TiberiumCost tiberiumCostPerBurst;
        public TiberiumCost tiberiumCostPerShot;
        public ThingDef beamMote = TiberiumDefOf.BeamMote;
        public float powerConsumption = 0;
    }
}
