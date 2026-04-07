using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public static class PipeNetworkMaker
    {
        private static HashSet<INetworkComponent> closedSet = new();
        private static HashSet<INetworkComponent> openSet = new();
        private static HashSet<INetworkComponent> currentSet = new();

        public static IEnumerable<INetworkComponent> ConnectedComponents(INetworkComponent forRoot)
        {
            closedSet.Clear();
            openSet.Clear();
            currentSet.Clear();
            openSet.Add(forRoot);
            while (openSet.Count > 0)
            {
                foreach (var component in openSet)
                {
                    closedSet.Add(component);
                    yield return component;
                }

                (currentSet, openSet) = (openSet, currentSet);
                openSet.Clear();
                foreach (var component in currentSet)
                {
                    var map = component.Parent.Thing.Map;
                    for (var i = 0; i < component.Parent.ConnectionCells.Length; i++)
                    {
                        var c = component.Parent.ConnectionCells[i];
                        List<Thing> thingList = c.GetThingList(map);
                        for (var t = 0; t < thingList.Count; t++)
                        {
                            var thing = thingList[t];
                            if (!Fits(thing, component.NetworkDef, out var newComponent)) continue;
                            if (!closedSet.Contains(newComponent) && newComponent.ConnectsTo(component))
                            {
                                openSet.Add(newComponent);
                                break;
                            }
                        }
                    }
                }
            }
            closedSet.Clear();
            openSet.Clear();
            currentSet.Clear();
        }

        //Check whether or not a thing is part of a network
        private static bool Fits(Thing thing, NetworkDef forNetwork, out INetworkComponent component)
        {
            //component = thing as INetworkStructure;
            INetworkStructure structure = (thing as ThingWithComps)?.AllComps.Find(t => t is INetworkStructure) as INetworkStructure;
            component = structure?.NetworkParts.Find(c => c.NetworkDef == forNetwork);
            return component != null;
        }
    }
}
