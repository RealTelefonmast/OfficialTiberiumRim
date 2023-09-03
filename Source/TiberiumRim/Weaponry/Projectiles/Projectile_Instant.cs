﻿using RimWorld;
using Verse;

namespace TR
{
    public class Projectile_Instant : Projectile
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
        }

        protected virtual void Finish()
        {
            this.Destroy();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
