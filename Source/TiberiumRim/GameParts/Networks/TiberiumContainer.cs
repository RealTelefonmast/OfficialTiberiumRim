using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumContainer : NetworkContainer
    {
        public TiberiumContainer(IContainerHolder parent, float capacity, List<Enum> acceptedTypes, Type containedType) : base(parent, capacity, acceptedTypes, containedType)
        {
        }

        public override List<Thing> PotentialItemDrops()
        {
            //TODO: Add Gas Leak
            List<Thing> list = new List<Thing>();
            foreach (TiberiumValueType type in AllStoredTypes)
            {
                if (StoredValuesByType.ContainsKey(type))
                {
                    ThingDef def = TRUtils.CrystalDefFromType(type, out bool isGas);
                    if (def != null)
                    {
                        if (!isGas)
                        {
                            TiberiumCrystalDef crystalDef = def as TiberiumCrystalDef;
                            int count = (int)(StoredValuesByType[type] / crystalDef.tiberium.harvestValue);
                            for (int i = 0; i < count; i++)
                            {
                                TiberiumCrystal crystal = ThingMaker.MakeThing(crystalDef) as TiberiumCrystal;
                                list.Add(crystal);
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            return list;
        }

        public override Color Color
        {
            get
            {
                Color color = new Color();
                if (StoredValuesByType.Count > 0)
                {
                    foreach (TiberiumValueType type in AllStoredTypes)
                    {
                        color += type.GetColor() * (StoredValuesByType[type] / TotalCapacity);
                    }
                }
                return color;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (TotalCapacity > 0)
            {
                if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(Parent.Thing))
                {
                    yield return new Gizmo_TiberiumStorage
                    {
                        container = this
                    };
                }

                /*
                if (!AcceptedTypes.NullOrEmpty())
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "ContainerMode_TR".Translate(),
                        icon = TiberiumContent.ContainMode_TripleSwitch,
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            
                            foreach (TiberiumCategory category in Props.supportedCats)
                            {
                                list.Add(new FloatMenuOption("SiloCat" + category + "_TR", delegate ()
                                {
                                    container.Role = category;
                                }));
                            }
                            
                            FloatMenu menu = new FloatMenu(list);
                            menu.vanishIfMouseDistant = true;
                            Find.WindowStack.Add(menu);
                        }
                    };
                }
                */

                if (DebugSettings.godMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Container Options",
                        icon = TiberiumContent.ContainMode_TripleSwitch,
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            list.Add(new FloatMenuOption("Add RGB", delegate
                            {
                                TryAddValue(TiberiumValueType.Red, 500, out var ex);
                                TryAddValue(TiberiumValueType.Blue, 500, out ex);
                                TryAddValue(TiberiumValueType.Green, 500, out ex);
                            }));
                            list.Add(new FloatMenuOption("Add Gas",
                                delegate { TryAddValue(TiberiumValueType.Gas, 1000, out var ex); }));
                            list.Add(new FloatMenuOption("Add Green",
                                delegate { TryAddValue(TiberiumValueType.Green, 500, out var ex); }));
                            list.Add(new FloatMenuOption("Add Blue",
                                delegate { TryAddValue(TiberiumValueType.Blue, 500, out var ex); }));
                            list.Add(new FloatMenuOption("Add Red",
                                delegate { TryAddValue(TiberiumValueType.Red, 500, out var ex); }));
                            list.Add(new FloatMenuOption("Add Sludge",
                                delegate { TryAddValue(TiberiumValueType.Sludge, 500, out var ex); }));
                            list.Add(new FloatMenuOption("Clear", delegate { Clear(); }));
                            FloatMenu menu = new FloatMenu(list);
                            menu.vanishIfMouseDistant = true;
                            Find.WindowStack.Add(menu);
                        }
                    };
                }
            }
        }
    }
}
