using TeleCore.Visual.VFX.Effecters;
using TeleCore.Visual.VFX.Particles.Motes;
using Verse;

namespace TeleCore.Utils;

public static class TMoteMaker
{
    public static MoteBeam MakeBeamEffect(ThingDef moteDef, TargetInfo A, TargetInfo B, float width)
    {
        MoteBeam beam = (MoteBeam)ThingMaker.MakeThing(moteDef, null);
        beam.Scale = 0.5f;
        beam.Attach(A, B);
        beam.UpdateWidth(width);
        GenSpawn.Spawn(beam, A.Cell, A.Map ?? B.Map, WipeMode.Vanish);
        return beam;
    }
}