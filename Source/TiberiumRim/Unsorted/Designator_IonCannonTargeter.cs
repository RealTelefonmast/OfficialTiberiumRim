using System.Linq;
using RimWorld;
using TiberiumRim;
using TR.Designators;
using Verse;
using EVASignal = TR.EVA.EVASignal;
using GameComponent_EVA = TR.EVA.GameComponent_EVA;

namespace TR.SuperWeapon;

public class Designator_IonCannonTargeter : Designator_Target
{
    public Designator_IonCannonTargeter()
    {
        defaultLabel = "TR.IonCannon.Order".Translate();
        defaultDesc = "TR.IonCannon.OrderDesc".Translate();
        icon = TiberiumContent.IonCannonIcon;
        useMouseIcon = false;
        soundSucceeded = SoundDefOf.Click;

        targeterMat = TiberiumContent.IonCannonTargeter;
        size = IonCannon_Strike.radius * 2;
    }

    public override bool Visible => true;

    public override void Selected()
    {
        base.Selected();
        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectTarget, null);
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return base.CanDesignateCell(loc).Accepted;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        //base.DesignateSingleCell(c);
        var sat = NearestSatellite(Map);
        if (sat != null)
        {
            sat.SetAttackDest(Map, c);
            sat.SetDestination(Map.Tile);
            sat.SetCallback(superWeapon);
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.IonCannonActivated, null);
        }

        Find.DesignatorManager.Deselect();
    }

    private AttackSatellite_Ion NearestSatellite(Map fromMap = null, int fromTile = -1)
    {
        AttackSatellite_Ion sat = null;
        if (fromMap != null)
        {
            var map = Find.CurrentMap;
            fromTile = map.Tile;
        }

        var sats = TRUtils.Tiberium().GetWorldInfo<SatelliteInfo>().AttackSatelliteNetwork.ASatsIon;
        sat = fromTile >= 0
            ? sats.MinBy(s => Find.WorldGrid.ApproxDistanceInTiles(fromTile, s.Tile))
            : sats.FirstOrDefault();
        return sat;
    }
}