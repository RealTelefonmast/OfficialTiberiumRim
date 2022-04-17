using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public abstract class Designator_Target : Designator_Extended
    {
        protected Material targeterMat;
        private Material tempMaterial;
        protected float size;
        protected FloatRange opacity = new FloatRange(0.5f, 1f);

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return loc.InBounds(Map) && !loc.Fogged(Map);
        }

        public override void SelectedUpdate()
        {
            if (targeterMat == null)
                return;

            tempMaterial = new Material(targeterMat);
            bool designateCheck = CanDesignateCell(UI.MouseCell()).Accepted;
            Color color = !designateCheck ? Color.red : targeterMat.color;
            if (opacity.min != opacity.max)
                color.a = TMath.Cosine2(opacity.min, opacity.max, 3f, 0, Time.realtimeSinceStartup * 6.28318548f);
            tempMaterial.color = color;
            TRUtils.DrawTargeter(UI.MouseCell(), tempMaterial, size);
        }
    }

    public class Designator_ScrinLanding : Designator_Target
    {
        public bool activated = false;

        public Designator_ScrinLanding()
        {
            this.defaultLabel = "DEBUG: Scrin Landing";
            this.defaultDesc = "Scrin lands here now";
            this.icon = TiberiumContent.ScrinIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;
            this.mustBeUsed = true;

            targeterMat = TiberiumContent.NodNukeTargeter;
            size = 6;
        }

        public override void Selected()
        {
            base.Selected();
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectDestination, null);
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, TiberiumDefOf.ScrinDronePlatform);
            DronePlatform platform = (DronePlatform) ThingMaker.MakeThing(TiberiumDefOf.ScrinDronePlatform);
            platform.SetFactionDirect(Faction.OfPlayer);
            SkyfallerMaker.SpawnSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, platform, c, Map);
            activated = true;
        }

        public override bool MustStaySelected => base.MustStaySelected && !activated;

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
            this.defaultLabel = "Ion Cannon";
            this.defaultDesc = "Obliterate it all.";
            this.icon = TiberiumContent.IonCannonIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;


            targeterMat = TiberiumContent.IonCannonTargeter;
            size = IonCannon_Strike.radius * 2;
        }

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

            var sats = Find.World.GetComponent<WorldComponent_TR>().SatelliteInfo.AttackSatelliteNetwork.ASatsIon;
            sat = fromTile >= 0 ? sats.MinBy(s => Find.WorldGrid.ApproxDistanceInTiles(fromTile, s.Tile)) : sats.FirstOrDefault();
            return sat;
        }

        public override bool Visible
        {
            get { return true; } 
        }
    }

    public class Designator_NodNukeTargeter: Designator_Target
    {
        public Designator_NodNukeTargeter()
        {
            this.defaultLabel = "Nod Nuke";
            this.defaultDesc = "Cleanse.";
            this.icon = TiberiumContent.NodNukeIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;


            targeterMat = TiberiumContent.NodNukeTargeter;
            size = 16;
        }

        public override void Selected()
        {
            base.Selected();
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectTarget, null);
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
}
