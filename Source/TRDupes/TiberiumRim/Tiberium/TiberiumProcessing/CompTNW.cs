using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public enum TNWMode
    {
        GlobalStorage,
        LocalStorage,
        Transmitter
    }

    public class CompTNW : ThingComp
    {
        public new TiberiumNetworkBuilding parent;
        public MapComponent_TNWManager TiberiumNet;
        public CompPowerTrader compPower;
        public CompFlickable compFlick;

        private TiberiumContainer container;
        private IntVec3 lastPos;

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref container, "container");
            Scribe_Values.Look(ref lastPos, "lastPos");
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent = base.parent as TiberiumNetworkBuilding;
            TiberiumNet = parent.Map.GetComponent<MapComponent_TNWManager>();
            compPower = this.parent.TryGetComp<CompPowerTrader>();
            compFlick = this.parent.TryGetComp<CompFlickable>();
            if (!respawningAfterLoad)
            {
                lastPos += parent.Position;
                Container = new TiberiumContainer(Props.maxStorage, Props.storeMode, this.parent);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
        }

        public CompProperties_TNW Props => base.props as CompProperties_TNW;
        public TiberiumContainer Container { get => container; set => container = value; }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (DebugSettings.godMode)
            {
                if (Props.maxStorage > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Container Options",
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            list.Add(new FloatMenuOption("Add RGB", delegate ()
                            {
                                container.TryAddValue(TiberiumType.Red, 500, out int ex);
                                container.TryAddValue(TiberiumType.Blue, 500, out ex);
                                container.TryAddValue(TiberiumType.Green, 500, out ex);
                            }));
                            list.Add(new FloatMenuOption("Add Gas", delegate ()
                            {
                                Log.Message("Adding green");
                                container.TryAddValue(TiberiumType.Gas, 1000, out int ex);
                            }));
                            list.Add(new FloatMenuOption("Add Green", delegate ()
                            {
                                Log.Message("Adding green");
                                container.TryAddValue(TiberiumType.Green, 500, out int ex);
                            }));
                            list.Add(new FloatMenuOption("Add Blue", delegate ()
                            {
                                container.TryAddValue(TiberiumType.Blue, 500, out int ex);
                            }));
                            list.Add(new FloatMenuOption("Add Red", delegate ()
                            {
                                container.TryAddValue(TiberiumType.Red, 500, out int ex);
                            }));
                            list.Add(new FloatMenuOption("Clear", delegate ()
                            {
                                container.Clear();
                            }));
                            FloatMenu menu = new FloatMenu(list);
                            menu.vanishIfMouseDistant = true;
                            Find.WindowStack.Add(menu);
                        }
                    };
                }
            }
        }
    }

    public class CompProperties_TNW : CompProperties
    {
        public int maxStorage = 0;
        public bool dropsContents = true;
        public bool storage = true;
        public bool transmitter = true;

        public int consumePerDay = 0;
        public bool global = false;
        public StoreMode storeMode = StoreMode.RGB;
        public TNWMode tnwMode = TNWMode.Transmitter;

        public CompProperties_TNW()
        {
            this.compClass = typeof(CompTNW);
        }
    }
}
