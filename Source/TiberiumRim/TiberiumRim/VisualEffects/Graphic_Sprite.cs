using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Graphic_Sprite : Graphic_NumberedCollection
    {
        private static Dictionary<Thing, int> indices = new Dictionary<Thing, int>();

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            /*
            var extendedData = (thingDef as TRThingDef)?.extraData;
            if (extendedData != null && extendedData.repeatSprite)
            {
                int tick = extendedData.spriteTicks;
                if (thing.IsHashIntervalTick(tick))
                {
                    index++;
                    if (index > subGraphics.Length)
                        index = 0;
                }
            }
            */
            subGraphics[GetIndex(thing)].DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public void AddIndex(Thing thing)
        {
            if (!indices.ContainsKey(thing))
                indices.Add(thing, 0);
        }

        public int GetIndex(Thing thing)
        {
            if (indices.TryGetValue(thing, out int i))
                return i;
            return i;
        }

        public Graphic CurrentGraphic(Thing thing)
        {
            return subGraphics[GetIndex(thing)];
        }

        public void Next(Thing thing)
        {
            if (GetIndex(thing) < Count-1)
                indices[thing]++;
        }

        public void Notify_Remove(Thing thing)
        {
            indices.Remove(thing);
        }
    }
}
