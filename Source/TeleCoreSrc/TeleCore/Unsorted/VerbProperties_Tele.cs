using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class VerbProperties_Tele : VerbProperties
{
    public List<VerbCompProperties> comps;
    
    //Functional
    public Vector3 shotStartOffset = Vector3.zero;
    public List<Vector3>? originOffsetPerShot;

    //Effects
    public MuzzleFlashProperties muzzleFlash;
    public EffecterDef originEffecter;
    
    //
    public NetworkCost networkCostPerShot;
    public float powerConsumptionPerShot = 0;
}