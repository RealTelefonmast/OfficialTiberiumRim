using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class CompTNW_Turret : CompTNW
    {
        public override Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { Container.StoredPercent, 1f, Container.StoredPercent };
        public override float?[] RotationOverrides => new float?[] { Rotation(DrawPositions[0]), Rotation(DrawPositions[1]), Rotation(DrawPositions[2])};
        public override bool[] DrawBools => new bool[] { true, StructureSet.Pipes.Any(), StructureSet.Pipes.Any() };

        private float Rotation(Vector3 fromPos)
        {
            var par = parent as Building_TurretGun;
            LocalTargetInfo currentTarget = par.CurrentTarget;
            if (currentTarget.IsValid)
            {
                float curRotation = (currentTarget.Cell.ToVector3Shifted() - fromPos).AngleFlat();
                return curRotation;
            }
            TurretTop top = Traverse.Create(par).Field("top").GetValue<TurretTop>();
            float rot = Traverse.Create(top).Field("curRotationInt").GetValue<float>();
            return rot;
        }

        //public override float[] DrawRotations => new float[] { ((Building_TurretGun)parent)..Rotation.AsAngle, parent.Rotation.AsAngle, parent.Rotation.AsAngle };
    }

    public class CompProperties_TNWTurret : CompProperties_TNW
    {
        public TurretProperties turret;
    }
}
