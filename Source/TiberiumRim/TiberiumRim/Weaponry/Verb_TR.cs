using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Verb_TR : Verb_LaunchProjectile
    {
        public CompTNW_Turret TiberiumComp => caster.TryGetComp<CompTNW_Turret>();

        public VerbProperties_TR Props => (VerbProperties_TR)base.verbProps;

        public bool IsBeam => Props.laser != null;

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
            if (base.CasterIsPawn)
            {
                Pawn casterPawn = base.CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return IsBeam ? true : this.Projectile != null;
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
            if (IsBeam)
            {
                return TryCastBeam();
            }
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

        public bool TryCastProjectile()
        {
            return base.TryCastShot();
        }

        public virtual bool TryCastBeam()
        {
            Log.Error("Trying to cast beam without using Verb_Beam");
            return false;
        }

        public bool TryCastTiberium()
        {
            return true;
        }
        
    }

    public class VerbProperties_TR : VerbProperties
    {
        public List<Vector3> originOffsets;
        public TiberiumCost tiberiumCostPerBurst;
        public TiberiumCost tiberiumCostPerShot;
        public float powerConsumption = 0;

        public LaserProperties laser;
    }
}
