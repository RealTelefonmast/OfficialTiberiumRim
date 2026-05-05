using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Buildings;

public class Building_TurretHubCore : Building_TeleTurret
{
    //Anticipated Connections
    private readonly List<IntVec3> anticipatingPositions = new();

    //
    public List<Building_HubTurret> hubTurrets = new();

    public Building_HubTurret DestroyedChild => hubTurrets.First(c => c.NeedsRepair);
    public bool AcceptsTurrets => hubTurrets.Count + AnticipatedBlueprintsOrFrames.Count < Extension.hub.maxTurrets;

    public List<Thing> AnticipatedBlueprintsOrFrames
    {
        get
        {
            var things = new List<Thing>();
            for (var i = anticipatingPositions.Count - 1; i >= 0; i--)
            {
                var position = anticipatingPositions[i];
                var thing = position.GetThingList(Verse.Map).Find(t =>
                    (t is Blueprint_Build b && b.def.entityDefToBuild.HasTurretExtension(out var extension) &&
                     extension.hub.hubDef == def)
                    || (t is Frame f && f.def.entityDefToBuild.HasTurretExtension(out var extension2) &&
                        extension2.hub.hubDef == def));
                if (thing != null)
                    things.Add(thing);
                else
                    anticipatingPositions.Remove(position);
            }

            return things;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    protected override void OnOrderAttack(LocalTargetInfo targ)
    {
        foreach (var hubTurret in hubTurrets) hubTurret.OrderAttack(targ);
    }

    public void Upgrade_AddTurret()
    {
    }

    public void AnticipateTurretAt(IntVec3 pos)
    {
        anticipatingPositions.Add(pos);
    }

    public void AddHubTurret(Building_HubTurret t)
    {
        if (hubTurrets.Contains(t)) return;
        hubTurrets.Add(t);
        t.parentHub = this;
        anticipatingPositions.Remove(t.Position);
    }


    public void RemoveHubTurret(Building_HubTurret turret)
    {
        hubTurrets.Remove(turret);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
    }

    public override void Print(SectionLayer layer)
    {
        base.Print(layer);
        foreach (var turret in hubTurrets)
            PrintTurretCable(layer, this, turret);
    }

    private void PrintTurretCable(SectionLayer layer, Thing A, Thing B)
    {
        var mat = MaterialPool.MatFrom(Extension.hub.cableTexturePath);
        var y = AltitudeLayer.SmallWire.AltitudeFor();
        var center = (A.TrueCenter() + B.TrueCenter()) / 2f;
        center.y = y;
        var v = B.TrueCenter() - A.TrueCenter();
        var size = new Vector2(1.5f, v.MagnitudeHorizontal());
        var rot = v.AngleFlat();
        Printer_Plane.PrintPlane(layer, center, size, mat, rot);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder(base.GetInspectString());
        sb.AppendLine();
        sb.AppendLine(Translations.Buildings.TurretHubAnticipated(AnticipatedBlueprintsOrFrames.Count));
        return sb.ToString().TrimEnd();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        yield return GenData.GetDesignatorFor<Designator_Build>(def); //new Designator_BuildFixed(def);
    }
}