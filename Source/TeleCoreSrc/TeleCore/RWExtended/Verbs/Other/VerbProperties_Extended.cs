using System.Collections.Generic;
using TeleCore.Network.Bills;
using UnityEngine;
using Verse;

namespace TeleCore.RWExtended.Verbs.Other;

public class VerbProperties_Extended : VerbProperties
{
    public bool avoidFriendlyFire;

    public BeamProperties beamProps;

    //
    public SoundDef chargeSound;

    //Information
    public string description;

    //Functional
    public bool isProjectile = true;
    public bool playShotOnceOnBurst = false;
    public float minHitHeight = 0;

    //Graphical
    public MuzzleFlashProperties muzzleFlash;
    public FleckDef muzzleFlashFleck;
    public NetworkCost networkCostPerShot;

    //Effects
    public EffecterDef originEffecter;
    public List<Vector3> originOffsetPerShot;

    //Costs
    public float powerConsumptionPerShot = 0;
    public ThingDef secondaryProjectile;

    //
    public float shootHeightOffset = 0;
    public int shotIntervalTicks = 10;
    public Vector3 shotStartOffset = Vector3.zero;

    public void PostLoad()
    {
        beamProps?.SetParent(this);
    }

    public new IEnumerable<string> ConfigErrors(ThingDef parent)
    {
        if (parent.race != null && linkedBodyPartsGroup != null &&
            !parent.race.body.AllParts.Any(part => part.groups.Contains(linkedBodyPartsGroup)))
            yield return string.Concat("has verb with linkedBodyPartsGroup ", linkedBodyPartsGroup, " but body ",
                parent.race.body, " has no parts with that group.");
        if (LaunchesProjectile && defaultProjectile != null && forcedMissRadius > 0f != CausesExplosion)
            yield return
                "has incorrect forcedMiss settings; explosive projectiles and only explosive projectiles should have forced miss enabled";
        if (beamMoteDef != null && beamMoteDef.thingClass != typeof(MoteBeam))
            yield return "has beamMoteDef that is not a MoteBeam";
    }
}