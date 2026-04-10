using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Graphic_LinkedWithSame : Graphic_Linked
    {
        public Graphic_LinkedWithSame() { }

        public Graphic_LinkedWithSame(Graphic subGraphic)
        {
            this.subGraphic = subGraphic;
        }

        public override void Init(GraphicRequest req)
        {
            subGraphic = GraphicDatabase.Get<Graphic_Single>(req.path, req.shader, Vector2.one, Color.white, Color.white, data);
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && c.GetFirstThing(parent.Map, parent.def) != null;
        }
    }
}
