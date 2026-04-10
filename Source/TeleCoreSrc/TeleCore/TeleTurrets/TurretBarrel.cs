using LudeonTK;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.TeleTurrets;

public class TurretBarrel
{
    private static readonly float smoothTime = 0.01f;
    private static readonly float deltaTime = 0.01666f;

    [TweakValue("TurretGunTop_BarrelOffset", -5f, 5f)]
    private static float barrelOffset = 0f;

    private float currentRecoil;
    public float currentVelocity;
    private readonly TurretGunTop parent;
    private readonly TurretBarrelProperties props;
    private float speed = 100;
    private float wantedRecoil;

    public TurretBarrel(TurretGunTop parent, TurretBarrelProperties props)
    {
        this.parent = parent;
        this.props = props;
    }

    public Graphic Graphic => props.graphic.Graphic;

    public Vector3 DrawPos
    {
        get
        {
            var drawPos = parent.DrawPos;
            var offset = props.barrelOffset + new Vector3(0, 0, barrelOffset) + props.recoilOffset * currentRecoil;
            drawPos += Quaternion.Euler(0, parent.CurRotation, 0) * offset;
            drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + props.altitudeOffset;
            return drawPos;
        }
    }

    public void Notify_TurretShot()
    {
        wantedRecoil = 1;
        speed = parent.Props.recoilSpeed;
    }

    public void BarrelTick()
    {
        currentRecoil = Mathf.SmoothDamp(currentRecoil, wantedRecoil, ref currentVelocity, smoothTime, speed, deltaTime);
        if (wantedRecoil > 0 && wantedRecoil - currentRecoil <= 0.01)
        {
            wantedRecoil = 0;
            speed = parent.Props.resetSpeed;
        }
    }

    public Vector3 DrawPos { get; private set; }
    
    public void Draw(Vector3 drawPos)
    {
        var offset = props.barrelOffset + new Vector3(0, 0, barrelOffset) + props.recoilOffset * currentRecoil;
        drawPos += Quaternion.Euler(0, parent.CurRotation, 0) * offset;
        drawPos.y = AltitudeLayer.BuildingOnTop.AltitudeFor() + props.altitudeOffset;
        DrawPos = drawPos;
        TDrawing.Draw(Graphic, drawPos, Rot4.North, parent.CurRotation, null);
        //Overlays.DrawMesh(mesh, DrawPos, parent.CurRotation.ToQuat(), graphic.MatSingle, 0);
    }
}