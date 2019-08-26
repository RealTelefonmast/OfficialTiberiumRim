using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public abstract class Designator_Target : Designator
    {
        protected Material targeterMat;
        protected float size;

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return loc.InBounds(Map) && !loc.Fogged(Map);
        }

        public override void SelectedUpdate()
        {
            var cell = UI.MouseCell();
            TRUtils.DrawTargeter(UI.MouseCell(), targeterMat, size);
        }
    }

    public class Designator_ScrinLanding : Designator_Target
    {
        public Designator_ScrinLanding()
        {
            this.defaultLabel = "Scrin Landing";
            this.defaultDesc = "Scrin lands here now";
            this.icon = TiberiumContent.ScrinIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;


            targeterMat = TiberiumContent.NodNukeTargeter;
            size = 6;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, TiberiumDefOf.DronePlatform);
            SkyfallerMaker.SpawnSkyfaller(TiberiumDefOf.ScrinDronePlatformIncoming, TiberiumDefOf.DronePlatform, c, Map);
            this.Finalize(true);
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
            this.icon = TiberiumContent.GDIIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;


            targeterMat = TiberiumContent.IonCannonTargeter;
            size = 16;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return base.CanDesignateCell(loc).Accepted;
        }
    }

    public class Designator_NodNukeTargeter: Designator_Target
    {
        public Designator_NodNukeTargeter()
        {
            this.defaultLabel = "Aurora Bomb";
            this.defaultDesc = "Cleanse.";
            this.icon = TiberiumContent.NodIcon;
            this.useMouseIcon = false;
            this.soundSucceeded = SoundDefOf.Click;


            targeterMat = TiberiumContent.NodNukeTargeter;
            size = 16;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return base.CanDesignateCell(loc).Accepted;
        }
    }
}
