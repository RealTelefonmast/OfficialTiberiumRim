using UnityEngine;
using Verse;

namespace TeleCore.Systems.Turrets;

public class TurretBarrelProperties
{
    public float altitudeOffset = 0;
    public Vector3 barrelOffset = Vector3.zero;
    public GraphicData graphic;
    public Vector3 recoilOffset;
}