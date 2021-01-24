using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_Draw : HediffComp
    {
        public bool MirrorImage => parent.Part.customLabel.Contains("right");

        public Comp_PawnExtraDrawer Drawer => Pawn.GetComp<Comp_PawnExtraDrawer>();

        public HediffCompProperties_Draw Props => (HediffCompProperties_Draw) base.props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
        }

        public string Identifier => MirrorImage ? Props.identifier + "_Mirror" : Props.identifier;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Drawer.RegisterParts(Identifier, Props.headGraphic, Props.bodyGraphic);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Drawer.DeregisterParts(Identifier);
        }
    }

    public class HediffCompProperties_Draw : HediffCompProperties
    {
        public HediffCompProperties_Draw()
        {
            compClass = typeof(HediffComp_Draw);
        }
        public GraphicData headGraphic;
        public GraphicData bodyGraphic;
        public string identifier;
    }
}
