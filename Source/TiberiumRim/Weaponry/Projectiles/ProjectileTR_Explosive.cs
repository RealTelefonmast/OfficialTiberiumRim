using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class ProjectileTR_Explosive : Projectile_Explosive
    {
        public TRThingDef TRDef => base.def as TRThingDef;
        public ProjectileProperties_Extended Props => TRDef?.projectileExtended;

        protected override void Impact(Thing hitThing)
        {
            if (Props != null)
            {
                Props.impactExplosion?.DoExplosion(Position, Map, this);
                Props.impactFilth?.SpawnFilth(Position, Map);
                Props.impactEffecter?.Spawn(Position, Map);
            }
            base.Impact(hitThing);
        }

        public override Graphic Graphic
        {
            get
            {
                if (base.Graphic is Graphic_Random Random) return Random.SubGraphicFor(this);
                return base.Graphic;
            }
        }
    }
}
