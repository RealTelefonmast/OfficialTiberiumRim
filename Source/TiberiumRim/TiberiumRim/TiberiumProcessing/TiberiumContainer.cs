using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumContainer : IExposable
    {
        public object parent;
        public float capacity;
        public IntRange pressure = new IntRange(0, 100);

        //
        public List<TiberiumValueType> AcceptedTypes = new List<TiberiumValueType>() { TiberiumValueType.Green, TiberiumValueType.Blue, TiberiumValueType.Red, TiberiumValueType.Sludge };
        public  Dictionary<TiberiumValueType, bool> TypeFilter = new Dictionary<TiberiumValueType, bool>();
        private Dictionary<TiberiumValueType, float> StoredTiberium = new Dictionary<TiberiumValueType, float>();

        //
        public TiberiumContainer() { }

        public TiberiumContainer(Thing parent)
        {
            this.parent = parent;
        }

        public TiberiumContainer(float capacity, List<TiberiumValueType> types, object parent = null)
        {
            this.parent = parent;
            this.capacity = capacity;
            if (!types.NullOrEmpty())
                AcceptedTypes = types;
        }

        public TiberiumContainer MakeCopy(Thing thing)
        {
            TiberiumContainer newContainer = new TiberiumContainer(thing);
            newContainer.capacity = capacity;
            newContainer.AcceptedTypes = new List<TiberiumValueType>(AcceptedTypes);
            newContainer.TypeFilter = TypeFilter.Copy();
            foreach (TiberiumValueType type in AllStoredTypes)
            {
                newContainer.TryAddValue(type, StoredTiberium[type], out float e);
            }
            return newContainer;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref StoredTiberium, "StoredTiberium");
            Scribe_Collections.Look(ref AcceptedTypes, "types");
            Scribe_Values.Look(ref capacity, "capacity");
        }

        public void Clear()
        {
            StoredTiberium.RemoveAll(s => s.Value > 0);
        }

        public bool AcceptsType(TiberiumValueType valueType)
        {
            return AcceptedTypes.Contains(valueType);
        }

        public bool CanEvenOutWith(TiberiumContainer other, float value)
        {
            float totalCapacity = capacity + other.capacity;
            float totalValue = TotalStorage + other.TotalStorage;
            return totalValue + Mathf.Abs(value) <= totalCapacity;
        }

        public void FillWith(float wantedValue)
        {
            float val = wantedValue / AcceptedTypes.Count;
            foreach(TiberiumValueType type in AcceptedTypes)
            {
                TryAddValue(type, val, out float e);
            }
        }

        public bool TryAddValue(TiberiumValueType valueType, float wantedValue, out float excessValue)
        {
            var potentialTotal = TotalStorage + wantedValue;
            excessValue = Mathf.Clamp(potentialTotal - capacity, 0, float.PositiveInfinity);
            var actualValue = wantedValue;
            if (CapacityFull || !AcceptsType(valueType))
                return false;

            if (excessValue > 0)
                actualValue = wantedValue - excessValue;

            if (StoredTiberium.TryGetValue(valueType, out float value))
                StoredTiberium[valueType] += actualValue;
            else
                StoredTiberium.Add(valueType, actualValue);

            return true;
        }

        public bool TryRemoveValue(TiberiumValueType valueType, float wantedValue, out float leftover)
        {
            leftover = wantedValue;
            if (StoredTiberium.TryGetValue(valueType, out float value) && value > 0)
            {
                leftover = Math.Abs(Mathf.Clamp(value - wantedValue, float.NegativeInfinity, 0));
                if (leftover > 0)
                    StoredTiberium[valueType] -= wantedValue;
                else
                    StoredTiberium[valueType] -= (wantedValue - leftover);
            }
            return leftover != wantedValue;
        }

        public bool TryTransferTo(TiberiumContainer other, TiberiumValueType valueType, float value)
        {
            if (StoredTiberium.TryGetValue(valueType) > 0 && CanEvenOutWith(other, value) && other.TryAddValue(valueType, value, out float excess))
            {
                TryRemoveValue(valueType, value - excess, out float leftOver);              
                return true;
            }
            return false;
        }
        
        public bool TryConsume(float wantedValue)
        {
            if(TotalStorage >= wantedValue)
            {
                float value = wantedValue;
                foreach (TiberiumValueType type in AllStoredTypes)
                {
                    if (value > 0f && TryRemoveValue(type, value, out float leftOver))
                    {
                        value -= (value - leftOver);
                    }
                }
                return true;
            }
            return false;
        }

        public bool TryConsume(TiberiumValueType valueType, float wantedValue)
        {
            if (ValueForType(valueType) >= wantedValue)
            {
               return TryRemoveValue(valueType, wantedValue, out float leftOver);
            }
            return false;
        }

        public float TotalStorage => StoredTiberium.Sum(t => t.Value);
        public float StoredPercent => TotalStorage/capacity;

        public Color Color
        {
            get
            {
                Color color = new Color();
                if(StoredTiberium.Count > 0)
                {
                    foreach(TiberiumValueType type in StoredTiberium.Keys)
                    {
                        color += TRUtils.ColorForType(type) * (StoredTiberium[type] / capacity);
                    }
                }
                return color;
            }
        }     

        public TiberiumValueType MainValueType
        {
            get
            {
                return StoredTiberium.MaxBy(x => x.Value).Key;
            }
        }

        public List<TiberiumValueType> AllStoredTypes
        {
            get
            {
                return StoredTiberium.Keys.ToList();
            }
        }

        public List<TiberiumCrystal> PotentialCrystals()
        {
            //TODO: Add Gas Leak
            List<TiberiumCrystal> list = new List<TiberiumCrystal>();
            foreach(TiberiumValueType type in AllStoredTypes)
            {
                if (StoredTiberium.ContainsKey(type))
                {
                    ThingDef def = TRUtils.CrystalDefFromType(type, out bool isGas);
                    if (def != null)
                    {
                        if (!isGas)
                        {
                            TiberiumCrystalDef crystalDef = def as TiberiumCrystalDef;
                            int count = (int)(StoredTiberium[type] / crystalDef.tiberium.harvestValue);
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

        public float ValueForTypes(List<TiberiumValueType> types)
        {
            float value = 0;
            foreach(TiberiumValueType type in types)
            {
                if (StoredTiberium.ContainsKey(type))
                {
                    value += StoredTiberium[type];
                }
            }
            return value;
        }

        public float ValueForType(TiberiumValueType valueType)
        {
            if (StoredTiberium.ContainsKey(valueType))
            {
                return StoredTiberium[valueType];
            }
            return 0;
        }

        public bool PotentialCapactiyFull(TiberiumValueType valueType, float potentialVal, out bool overfilled)
        {
            float val = potentialVal;
            overfilled = false;
            foreach(TiberiumValueType type2 in AllStoredTypes)
            {
                if(type2 != valueType)
                {
                    val += StoredTiberium[type2];
                }
            }
            if(val > capacity)
            {
                overfilled = true;
            }
            return val >= capacity;
        }

        public bool HasStorage => StoredPercent > 0;
        public bool Empty => !StoredTiberium.Any() || TotalStorage == 0;
        public bool CapacityFull => TotalStorage == capacity;
        public bool ContainsForbiddenType => AllStoredTypes.Any(t => !AcceptsType(t));

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (capacity > 0)
            {
                if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(parent))
                {
                    yield return new Gizmo_TiberiumStorage
                    {
                        container = this
                    };
                }

                if (!AcceptedTypes.NullOrEmpty())
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "ContainerMode_TR".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();
                            /*
                            foreach (TiberiumCategory category in Props.supportedCats)
                            {
                                list.Add(new FloatMenuOption("SiloCat" + category + "_TR", delegate ()
                                {
                                    container.mode = category;
                                }));
                            }
                            */
                            FloatMenu menu = new FloatMenu(list);
                            menu.vanishIfMouseDistant = true;
                            Find.WindowStack.Add(menu);
                        }
                    };
                }

                if (DebugSettings.godMode)
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
                                TryAddValue(TiberiumValueType.Red, 500, out float ex);
                                TryAddValue(TiberiumValueType.Blue, 500, out ex);
                                TryAddValue(TiberiumValueType.Green, 500, out ex);
                            }));
                            list.Add(new FloatMenuOption("Add Gas", delegate ()
                            {
                                TryAddValue(TiberiumValueType.Gas, 1000, out float ex);
                            }));
                            list.Add(new FloatMenuOption("Add Green", delegate ()
                            {
                                TryAddValue(TiberiumValueType.Green, 500, out float ex);
                            }));
                            list.Add(new FloatMenuOption("Add Blue", delegate ()
                            {
                                TryAddValue(TiberiumValueType.Blue, 500, out float ex);
                            }));
                            list.Add(new FloatMenuOption("Add Red", delegate ()
                            {
                                TryAddValue(TiberiumValueType.Red, 500, out float ex);
                            }));
                            list.Add(new FloatMenuOption("Add Sludge", delegate ()
                            {
                                TryAddValue(TiberiumValueType.Sludge, 500, out float ex);
                            }));
                            list.Add(new FloatMenuOption("Clear", delegate ()
                            {
                                Clear();
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
}
