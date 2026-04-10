using System.Collections.Generic;
using RimWorld;
using TeleCore.Utils;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TeleCore.TeleTurrets;

public class TurretGunTop
{
    //Barrels
    private readonly TurretGun parent;
    private bool rotateClockwise = true;

    //
    private float rotation;
    private float rotationSpeed;
    private bool targetAcquired;
    private float targetRot = 20;

    private int ticksUntilTurn;
    private int turnTicks;

    public TurretGunTop(TurretGun parent, TurretTopProperties topProps)
    {
        this.parent = parent;
        Props = topProps;
        if (HasBarrels)
        {
            Barrels = new List<TurretBarrel>(Props.barrels.Count);
            foreach (var barrel in Props.barrels) Barrels.Add(new TurretBarrel(this, barrel));
        }
    }

    public TurretTopProperties Props { get; }
    public List<TurretBarrel> Barrels { get; }
    public bool HasBarrels => Props.barrels != null;

    public bool OnTarget
    {
        get
        {
            if (parent.CurrentTarget.IsValid)
            {
                targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
                return Quaternion.Angle(rotation.ToQuat(), targetRot.ToQuat()) < 1.5f;
            }

            return false;
        }
    }

    public float CurRotation
    {
        get => rotation;
        set
        {
            if (value > 360) rotation = value - 360;
            if (value < 0) rotation = value + 360;
            rotation = value;
        }
    }

    //
    public void Notify_TurretShot(int index)
    {
        if (HasBarrels && Barrels.Count > index) 
            Barrels[index].Notify_TurretShot();
    }

    public void Notify_AimAngleChanged(float? angle)
    {
        if (angle.HasValue)
            rotation = angle.Value;
    }

    //
    public void TurretTopTick()
    {
        //Tick Barrels
        if (HasBarrels) Barrels.ForEach(b => b.BarrelTick());

        //Rotate Turret To Target Or Idle
        var currentTarget = parent.CurrentTarget;
        if (!currentTarget.IsValid)
            if (targetAcquired)
                targetAcquired = false;
        if (currentTarget.IsValid)
        {
            targetRot = (parent.CurrentTarget.CenterVector3 - parent.DrawPos).AngleFlat();
            turnTicks = 0;
        }
        else if (ticksUntilTurn > 0)
        {
            ticksUntilTurn--;
            if (ticksUntilTurn == 0)
            {
                rotateClockwise = !(Rand.Value > 0.5);
                turnTicks = Props.idleDuration.RandomInRange;
            }
        }
        else
        {
            targetRot += rotateClockwise ? 0.26f : -0.26f;
            turnTicks--;
            if (turnTicks <= 0)
                ticksUntilTurn = Props.idleInterval.RandomInRange;
        }

        rotation = Mathf.SmoothDampAngle(rotation, targetRot, ref rotationSpeed, 0.01f, Props.aimSpeed, 0.01666f);
        if (OnTarget && !targetAcquired)
        {
            targetAcquired = true;
            SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(parent.ParentThing.Position,
                parent.ParentThing.Map));
        }
    }

    public void DrawTurret(Vector3 drawPos)
    {
        drawPos = new Vector3(drawPos.x, AltitudeLayer.BuildingOnTop.AltitudeFor(), drawPos.z);
        TDrawing.Draw(Props.topGraphic.Graphic, drawPos, Rot4.North, CurRotation, null);
        if (HasBarrels)
            Barrels.ForEach(b => b.Draw(drawPos));
    }
}