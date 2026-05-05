using System.Collections.Generic;
using TeleCore.UI;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class VerbProperties_Tele : VerbProperties
{
    public List<VerbCompProperties> comps;

    //Effects
    public MuzzleFlashProperties muzzleFlash;

    //
    public NetworkCost networkCostPerShot;
    public EffecterDef originEffecter;
    public List<Vector3>? originOffsetPerShot;
    public float powerConsumptionPerShot = 0;

    //Functional
    public Vector3 shotStartOffset = Vector3.zero;
}