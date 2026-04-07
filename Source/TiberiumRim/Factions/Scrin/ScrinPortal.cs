using TR.Util;
using UnityEngine;
using Verse;

namespace TR.Factions.Scrin;

public class ScrinPortal : PortalSpawner
{
    protected override int ParticleTick => 20;

    protected override void DoParticleEffect()
    {
        float angleFromCenter = TRUtils.Range(0, 360);
        var rand = DrawPos + Quaternion.Euler(0, angleFromCenter, 0) * new Vector3(TRUtils.Range(4f, 4.25f), 0, 0);
        var angleToCenter = (angleFromCenter + 270).AngleWrapped();

        var mote = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("PortalParticle"));
        mote.Scale = TRUtils.Range(0.45f, 0.65f);
        mote.exactPosition = rand;
        mote.SetVelocity(angleToCenter, Rand.Range(1f, 1.25f));
        GenSpawn.Spawn(mote, rand.ToIntVec3(), Map);
    }
}