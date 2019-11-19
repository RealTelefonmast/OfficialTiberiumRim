using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class GlowerSource : ThingWithComps
    {
        private CompGlower glower;

        public void InitGlower(CompProperties props)
        {
            if (glower != null) return;
            glower = (CompGlower) Activator.CreateInstance(typeof(CompGlower));
            glower.parent = this;
            Traverse.Create(this).Field("comps").GetValue<List<ThingComp>>().Add(glower);
            glower.Initialize(props);
        }

        public void UpdateGlower(CompProperties newProps)
        {

        }
    } 

    public class CompGlowerOffset : ThingComp
    {
        public GlowerSource glower;
        public CompProperties_GlowerOffset Props => (CompProperties_GlowerOffset) base.props;

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            GlowerSource source = (GlowerSource)GenSpawn.Spawn(ThingDef.Named("GlowerSource"), parent.Position + parent.Rotation.FacingCell, parent.Map);
            source.InitGlower(Props.glower);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

        }

        public override void ReceiveCompSignal(string signal)
        {
            glower.BroadcastCompSignal(signal);
        }
    }

    public class CompProperties_GlowerOffset : CompProperties
    {
        public IntVec3 offset = IntVec3.Zero;
        public CompProperties_Glower glower;

        public CompProperties_GlowerOffset()
        {
            compClass = typeof(CompGlowerOffset);
        }
    }
}
