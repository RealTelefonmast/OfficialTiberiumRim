using System.ComponentModel;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class CompTNW_Turret : CompTNW
{
    public override Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
    public override Color[] ColorOverrides => new[] { Color.white, Color.white, Color.white };
    public override float[] OpacityFloats => new[] { Container.StoredPercent, 1f, Container.StoredPercent };

    public override float?[] RotationOverrides => new float?[]
        { Rotation(DrawPositions[0]), Rotation(DrawPositions[1]), Rotation(DrawPositions[2]) };

    public override bool[] DrawBools => new[] { true, StructureSet.Pipes.Any(), StructureSet.Pipes.Any() };

    private float Rotation(Vector3 fromPos)
    {
        var par = parent as Building_TurretGun;
        var currentTarget = par.CurrentTarget;
        if (currentTarget.IsValid)
        {
            var curRotation = (currentTarget.Cell.ToVector3Shifted() - fromPos).AngleFlat();
            return curRotation;
        }

        var top = Traverse.Create(par).Field("top").GetValue<TurretTop>();
        var rot = Traverse.Create(top).Field("curRotationInt").GetValue<float>();
        return rot;
    }

    //public override float[] DrawRotations => new float[] { ((Building_TurretGun)parent)..Rotation.AsAngle, parent.Rotation.AsAngle, parent.Rotation.AsAngle };
}

public class CompProperties_TNWTurret : CompProperties_TNW
{
    public TurretProperties turret;
}