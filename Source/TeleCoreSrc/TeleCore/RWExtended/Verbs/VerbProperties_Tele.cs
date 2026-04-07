using System.Collections.Generic;
using TeleCore.Network.Bills;
using UnityEngine;
using Verse;

namespace TeleCore.RWExtended.Verbs;

public class VerbProperties_Tele : VerbProperties
{
    //Functional
    public Vector3 shotStartOffset = Vector3.zero;
    public List<Vector3>? originOffsetPerShot;

    //Effects
    public MuzzleFlashProperties? muzzleFlash;
    public EffecterDef? originEffecter;

    //
    public NetworkCost networkCostPerShot;
    public float powerConsumptionPerShot = 0;
}