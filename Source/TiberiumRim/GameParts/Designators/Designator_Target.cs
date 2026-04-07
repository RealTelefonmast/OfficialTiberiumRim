using System.Linq;
using LudeonTK;
using RimWorld;
using TeleCore.ActionCompositions;
using TR.Defs;
using TR.GameParts.EVA;
using TR.Rendering.TextureContent;
using TR.Util;
using TR.Weaponry.SuperWeapon;
using UnityEngine;
using Verse;
using WorldComponent_TR = TR.World.WorldComponent_TR;

namespace TR.GameParts.Designators;

public abstract class Designator_Target : Designator_Extended
{
    protected FloatRange opacity = new(0.5f, 1f);
    protected float size;
    protected Material targeterMat;
    private Material tempMaterial;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return loc.InBounds(Map) && !loc.Fogged(Map);
    }

    public override void SelectedUpdate()
    {
        if (targeterMat == null)
            return;

        tempMaterial = new Material(targeterMat);
        var designateCheck = CanDesignateCell(UI.MouseCell()).Accepted;
        var color = !designateCheck ? Color.red : targeterMat.color;
        if (opacity.min != opacity.max)
            color.a = TRUtils.Cosine2(opacity.min, opacity.max, 3f, 0, Time.realtimeSinceStartup * 6.28318548f);
        tempMaterial.color = color;
        TRUtils.DrawTargeter(UI.MouseCell(), tempMaterial, size);
    }
}

public class Designator_ScrinLanding : Designator_Target
{
    public bool activated;

    public Designator_ScrinLanding()
    {
        defaultLabel = "DEBUG: Scrin Landing";
        defaultDesc = "Scrin lands here now";
        icon = TiberiumContent.ScrinIcon;
        useMouseIcon = false;
        soundSucceeded = SoundDefOf.Click;
        mustBeUsed = true;

        targeterMat = TiberiumContent.NodNukeTargeter;
        size = 6;
    }

    public override bool MustStaySelected => base.MustStaySelected && !activated;

    public override void Selected()
    {
        base.Selected();
        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectDestination);
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        var skyfaller =
            SkyfallerMaker.MakeSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, TiberiumDefOf.ScrinDronePlatform);
        var platform = (DronePlatform)ThingMaker.MakeThing(TiberiumDefOf.ScrinDronePlatform);
        platform.SetFactionDirect(Faction.OfPlayer);
        SkyfallerMaker.SpawnSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, platform, c, Map);
        activated = true;
    }

    public override bool CanRemainSelected()
    {
        return !activated;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return base.CanDesignateCell(loc).Accepted && loc.Standable(Map);
    }
}

public class Designator_IonCannonTargeter : Designator_Target
{
    public Designator_IonCannonTargeter()
    {
        defaultLabel = "Ion Cannon";
        defaultDesc = "Obliterate it all.";
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
        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectTarget);
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
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.IonCannonActivated);
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

        var sats = Find.World.GetComponent<WorldComponent_TR>().SatelliteInfo.AttackSatelliteNetwork.ASatsIon;
        sat = fromTile >= 0
            ? sats.MinBy(s => Find.WorldGrid.ApproxDistanceInTiles(fromTile, s.Tile))
            : sats.FirstOrDefault();
        return sat;
    }
}

public class Designator_NodNukeTargeter : Designator_Target
{
    [TweakValue("IonBubbleScale", 1f, 20f)]
    public static float IonScale = 20;

    [TweakValue("IonBubbleRotRate", 0f, 5f)]
    public static float RotRate = 1.5f;

    public Designator_NodNukeTargeter()
    {
        defaultLabel = "Nod Nuke";
        defaultDesc = "Cleanse.";
        icon = TiberiumContent.NodNukeIcon;
        useMouseIcon = false;
        soundSucceeded = SoundDefOf.Click;


        targeterMat = TiberiumContent.NodNukeTargeter;
        size = 16;
    }

    public override void Selected()
    {
        base.Selected();
        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectTarget);
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return true;
    }


        [TweakValue("IonBubbleScale", 1f, 20f)]
        public static float IonScale = 20;

        [TweakValue("IonBubbleRotRate", 0f, 5f)]
        public static float RotRate = 1.5f;

        public override void DesignateSingleCell(IntVec3 loc)
        {
            ActionComposition composition = new ActionComposition("Designator Bubble Test");
            Mote mote = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonBubble"), null);
            Mote distortion = (Mote)ThingMaker.MakeThing(ThingDef.Named("IonDistortionBubble"));
            composition.AddPart(delegate (ActionPart part)
            {
                mote.exactPosition = loc.ToVector3Shifted();
                mote.Scale = 20;
                mote.rotationRate = 1.2f;
                mote.instanceColor = new ColorInt(70, 90, 175).ToColor;
                GenSpawn.Spawn(mote, loc, Map, WipeMode.Vanish);
                distortion.exactPosition = loc.ToVector3Shifted();
                float scaleVal = 20 * (part.CurrentTick / (float)part.playTime);
                distortion.Scale = scaleVal;
                distortion.rotationRate = RotRate;
                GenSpawn.Spawn(distortion, loc, Map);
            },0);
            composition.AddPart(delegate (ActionPart part)
            {
                float scaleVal = 20 * (part.CurrentTick / (float)part.playTime);
                distortion.Scale = scaleVal;
                mote.Scale = scaleVal * 8;
            }, 0, 20);
            composition.Init();
        }
    }

public class TemporaryTargeter : TRBuilding
{
    public Material mat;
    public float size;

    public override void Draw()
    {
        TRUtils.DrawTargeter(Position, mat, size);
        base.Draw();
    }
}