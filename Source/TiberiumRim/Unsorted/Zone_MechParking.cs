using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TR;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class Zone_MechParking : Zone
{
    private readonly Dictionary<IntVec3, MechanicalPawn> parkingSlots = new();
    private int maxSlots;
    public MechanicalPawnKindDef mechKindDef;

    private int slotsTaken;

    public Zone_MechParking(ZoneManager manager, MechanicalPawnKindDef kindDef) : base(
        "TR_MechParkingZone".Translate(kindDef.LabelCap), manager)
    {
        mechKindDef = kindDef;
    }

    protected override Color NextZoneColor
    {
        get
        {
            var color = Color.cyan;
            color.a = 0.09f;
            return color;
        }
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.GetInspectString());
        sb.AppendLine("TR_MechParkingSlots".Translate() + " " + slotsTaken + "/" + maxSlots);
        return sb.ToString().TrimEndNewlines();
    }

    public IntVec3 SlotFor(MechanicalPawn mech)
    {
        if (!parkingSlots.ContainsValue(mech))
            return IntVec3.Invalid;
        return parkingSlots.FirstOrDefault(k => k.Value == mech).Key;
    }

    public void AssignNextSlot(MechanicalPawn mech)
    {
        foreach (var cell in cells)
            if (parkingSlots[cell] == null)
            {
                parkingSlots[cell] = mech;
                slotsTaken++;
                return;
            }

        Messages.Message("TR_MechParkingZoneFull".Translate(), mech, MessageTypeDefOf.RejectInput);
    }

    public void DismissSlot(MechanicalPawn mech)
    {
        parkingSlots[parkingSlots.FirstOrDefault(k => k.Value == mech).Key] = null;
        slotsTaken--;
    }

    public override void AddCell(IntVec3 c)
    {
        base.AddCell(c);

        if (!parkingSlots.ContainsKey(c))
        {
            parkingSlots.Add(c, null);
            maxSlots++;
        }
    }

    public override void RemoveCell(IntVec3 c)
    {
        base.RemoveCell(c);
        if (parkingSlots.TryGetValue(c, out var mech))
            AssignNextSlot(mech);
        parkingSlots.Remove(c);
        maxSlots--;
    }

    public override IEnumerable<Gizmo> GetZoneAddGizmos()
    {
        yield return new Designator_ZoneAdd_MechParking(mechKindDef);
        yield break;
    }
}