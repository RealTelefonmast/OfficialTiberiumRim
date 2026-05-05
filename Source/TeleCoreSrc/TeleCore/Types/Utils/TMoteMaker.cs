using TeleCore.Rendering;
using Verse;

namespace TeleCore.Types.Utils;

public static class TMoteMaker
{
    public static MoteBeam MakeBeamEffect(ThingDef moteDef, TargetInfo A, TargetInfo B, float width)
    {
        var beam = (MoteBeam)ThingMaker.MakeThing(moteDef);
        beam.Scale = 0.5f;
        beam.Attach(A, B);
        beam.UpdateWidth(width);
        GenSpawn.Spawn(beam, A.Cell, A.Map ?? B.Map);
        return beam;
    }
}