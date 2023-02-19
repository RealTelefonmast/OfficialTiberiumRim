using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Turret : Comp_TiberiumNetworkStructure
    {
        public Building_TurretGun Turret => parent as Building_TurretGun;
        public TurretTop TurretTop => Turret.top;

        //CompFX
        public override bool FX_ProvidesForLayer(FXArgs args)
        {
            return args.index is >= 0 and < 3;
        }


        public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
        {
            return parent.DrawPos;
        }


        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return args.index switch
            {
                _ => Color.white
            };
        }
        
        public override float? FX_GetOpacity(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => Container.StoredPercent,
                2 => Container.StoredPercent,
                _ => 1f
            };
        }

        public override float? FX_GetRotation(FXLayerArgs args)
        {
            return args.index switch
            {
                _ => Rotation(parent.DrawPos)
            };
        }

        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                1 => HasConnection,
                2 => HasConnection,
                _ => true
            };
            return base.FX_ShouldDraw(args);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        //Turret
        private float Rotation(Vector3 fromPos)
        {
            LocalTargetInfo currentTarget = (parent as Building_TurretGun).CurrentTarget;
            if (currentTarget.IsValid)
            {
                return (currentTarget.Cell.ToVector3Shifted() - fromPos).AngleFlat();
            }
            return TurretTop.CurRotation;
        }

        //public override float[] DrawRotations => new float[] { ((Building_TurretGun)parent)..Rotation.AsAngle, parent.Rotation.AsAngle, parent.Rotation.AsAngle };
    }

    public class CompProperties_TNWTurret : CompProperties_NetworkStructure
    {
        public TurretProperties turret;
    }
}

