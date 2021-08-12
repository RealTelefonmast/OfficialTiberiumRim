using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Turret : Comp_TiberiumNetworkStructure
    {
        //Cached
        private TurretTop turretTop;
        private Traverse floatValue;

        //CompFX
        public override Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { Container.StoredPercent, 1f, Container.StoredPercent };
        public override float?[] RotationOverrides => new float?[] { Rotation(DrawPositions[0]), Rotation(DrawPositions[1]), Rotation(DrawPositions[2]) };
        public override bool[] DrawBools => new bool[] { true, HasConnection, HasConnection };

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var par = parent as Building_TurretGun;
            turretTop = Traverse.Create(par).Field("top").GetValue<TurretTop>();
            floatValue = Traverse.Create(turretTop).Field("curRotationInt");
        }

        //Turret
        private float Rotation(Vector3 fromPos)
        {
            LocalTargetInfo currentTarget = (parent as Building_TurretGun).CurrentTarget;
            if (currentTarget.IsValid)
            {
                return (currentTarget.Cell.ToVector3Shifted() - fromPos).AngleFlat();
            }
            return floatValue.GetValue<float>();
        }

        //public override float[] DrawRotations => new float[] { ((Building_TurretGun)parent)..Rotation.AsAngle, parent.Rotation.AsAngle, parent.Rotation.AsAngle };
    }

    public class CompProperties_TNWTurret : CompProperties_NetworkStructure
    {
        public TurretProperties turret;
    }
}

