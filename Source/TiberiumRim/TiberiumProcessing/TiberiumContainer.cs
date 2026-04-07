using System.Collections.Generic;
using System.Linq;
using TR.Data.ThingClasses.TibCrystals;
using TR.GameParts.Interfaces;
using TR.Rendering.TextureContent;
using TR.TiberiumObjects;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.TiberiumProcessing;

public class TiberiumContainer : IExposable
{
    //
    public List<TiberiumValueType> AcceptedTypes = new()
        { TiberiumValueType.Green, TiberiumValueType.Blue, TiberiumValueType.Red, TiberiumValueType.Sludge };

    public float capacity;
    public IContainerHolder holder;
    public object parent;
    public IntRange pressure = new(0, 100);
    private Dictionary<TiberiumValueType, float> StoredTiberium = new();
    public Dictionary<TiberiumValueType, bool> TypeFilter = new();

    //Constructors
    public TiberiumContainer()
    {
    }

    public TiberiumContainer(Thing parent)
    {
        this.parent = parent;
    }

    public TiberiumContainer(Thing parent, IContainerHolder holder)
    {
        this.parent = parent;
        this.holder = holder;
    }

    public TiberiumContainer(float capacity, List<TiberiumValueType> types, object parent = null,
        IContainerHolder holder = null)
    {
        this.parent = parent;
        this.holder = holder;
        this.capacity = capacity;
        if (!types.NullOrEmpty())
            AcceptedTypes = types;
    }

    public List<TiberiumValueType> AllStoredTypes => StoredTiberium.Keys.ToList();
    public Dictionary<TiberiumValueType, float> AllStoredTypeValues => StoredTiberium;

    public float TotalStorage => StoredTiberium.Sum(t => t.Value);
    public float StoredPercent => TotalStorage / capacity;

    public bool HasStorage => TotalStorage > 0;
    public bool Empty => !StoredTiberium.Any() || TotalStorage <= 0;
    public bool CapacityFull => TotalStorage >= capacity;
    public bool ContainsForbiddenType => Enumerable.Any(AllStoredTypes, t => !AcceptsType(t));

    public Color Color
    {
        get
        {
            var color = new Color();
            if (StoredTiberium.Count > 0)
                foreach (var type in StoredTiberium.Keys)
                    color += type.GetColor() * (StoredTiberium[type] / capacity);

            return color;
        }
    }

    public TiberiumValueType MainValueType
    {
        get { return StoredTiberium.MaxBy(x => x.Value).Key; }
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref StoredTiberium, "StoredTiberium");
        Scribe_Collections.Look(ref AcceptedTypes, "types");
        Scribe_Values.Look(ref capacity, "capacity");
    }

    //
    public TiberiumContainer MakeCopy(Thing thing)
    {
        var newContainer = new TiberiumContainer(thing);
        newContainer.capacity = capacity;
        newContainer.AcceptedTypes = new List<TiberiumValueType>(AcceptedTypes);
        newContainer.TypeFilter = TypeFilter.Copy();
        foreach (var type in AllStoredTypes) newContainer.TryAddValue(type, StoredTiberium[type], out var e);
        return newContainer;
    }

    //Helper Functions
    public void Notify_Full()
    {
        holder?.Notify_ContainerFull();
    }

    public void Clear()
    {
        StoredTiberium.RemoveAll(s => s.Value > 0);
    }

    public void FillWith(float wantedValue)
    {
        var val = wantedValue / AcceptedTypes.Count;
        foreach (var type in AcceptedTypes) TryAddValue(type, val, out var e);
    }

    //Transfer Functions
    public bool AcceptsType(TiberiumValueType valueType)
    {
        return AcceptedTypes.Contains(valueType);
    }

    public bool CanFullyTransferTo(TiberiumContainer other, float value)
    {
        return other.TotalStorage + value <= other.capacity;
    }

    // Value Functions
    public bool TryAddValue(TiberiumValueType valueType, float wantedValue, out float actualValue)
    {
        //If we add more than we can contain, we have an excess weight
        var excessValue = Mathf.Clamp(TotalStorage + wantedValue - capacity, 0, float.MaxValue);
        //The actual added weight is the wanted weight minus the excess
        actualValue = wantedValue - excessValue;

        //If the container is full, or doesnt accept the type, we dont add anything
        if (CapacityFull)
        {
            Notify_Full();
            return false;
        }

        if (!AcceptsType(valueType))
            return false;

        //If the weight type is already stored, add to it, if not, make a new entry
        if (StoredTiberium.TryGetValue(valueType, out var value))
            StoredTiberium[valueType] += actualValue;
        else
            StoredTiberium.Add(valueType, actualValue);

        //If this adds the last drop, notify full
        if (CapacityFull)
            Notify_Full();

        return true;
    }

    public bool TryRemoveValue(TiberiumValueType valueType, float wantedValue, out float actualValue)
    {
        //Attempt to remove a certain weight from the container
        actualValue = wantedValue;
        if (StoredTiberium.TryGetValue(valueType, out var value) && value > 0)
        {
            if (value >= wantedValue)
                //If we have stored more than we need to pay, remove the wanted weight
            {
                StoredTiberium[valueType] -= wantedValue;
            }
            else if (value > 0)
            {
                //If not enough stored to "pay" the wanted weight, remove the existing weight and set actual removed weight to removed weight 
                StoredTiberium[valueType] = 0;
                actualValue = value;
            }
        }

        if (StoredTiberium[valueType] <= 0) StoredTiberium.Remove(valueType);
        return actualValue == wantedValue;
    }

    public bool TryTransferTo(TiberiumContainer other, TiberiumValueType valueType, float value)
    {
        //Attempt to transfer a weight to another container
        //Check if anything of that type is stored, check if transfer of weight is possible without loss, try remove the weight from this container
        if (!other.AcceptsType(valueType)) return false;
        if (StoredTiberium.TryGetValue(valueType) >= value && CanFullyTransferTo(other, value) &&
            TryRemoveValue(valueType, value, out var actualValue))
        {
            //If passed, try to add the actual weight removed from this container, to the other.
            other.TryAddValue(valueType, actualValue, out var actualAddedValue);
            return true;
        }

        return false;
    }

    public bool TryConsume(float wantedValue)
    {
        if (TotalStorage >= wantedValue)
        {
            var value = wantedValue;
            foreach (var type in AllStoredTypes)
                if (value > 0f && TryRemoveValue(type, value, out var leftOver))
                    value = leftOver;

            return true;
        }

        return false;
    }

    public bool TryConsume(TiberiumValueType valueType, float wantedValue)
    {
        if (ValueForType(valueType) >= wantedValue) return TryRemoveValue(valueType, wantedValue, out var leftOver);
        return false;
    }

    public List<TiberiumCrystal> PotentialCrystals()
    {
        //TODO: Add Gas Leak
        var list = new List<TiberiumCrystal>();
        foreach (var type in AllStoredTypes)
            if (StoredTiberium.ContainsKey(type))
            {
                ThingDef def = TRUtils.CrystalDefFromType(type, out var isGas);
                if (def != null)
                    if (!isGas)
                    {
                        var crystalDef = def as TiberiumCrystalDef;
                        var count = (int)(StoredTiberium[type] / crystalDef.tiberium.harvestValue);
                        for (var i = 0; i < count; i++)
                        {
                            var crystal = ThingMaker.MakeThing(crystalDef) as TiberiumCrystal;
                            list.Add(crystal);
                        }
                    }
            }

        return list;
    }

    //Value
    public float ValueForTypes(List<TiberiumValueType> types)
    {
        float value = 0;
        foreach (var type in types)
            if (StoredTiberium.ContainsKey(type))
                value += StoredTiberium[type];

        return value;
    }

    public float ValueForType(TiberiumValueType valueType)
    {
        if (StoredTiberium.ContainsKey(valueType)) return StoredTiberium[valueType];
        return 0;
    }

    public bool PotentialCapactiyFull(TiberiumValueType valueType, float potentialVal, out bool overfilled)
    {
        var val = potentialVal;
        foreach (var type2 in AllStoredTypes)
            if (type2 != valueType)
                val += StoredTiberium[type2];

        overfilled = val > capacity;
        return val >= capacity;
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (capacity > 0)
        {
            if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(parent))
                yield return new Gizmo_TiberiumStorage
                {
                    container = this
                };

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
                                container.mode = category;
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
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Container Options",
                    icon = TiberiumContent.ContainMode_TripleSwitch,
                    action = delegate
                    {
                        var list = new List<FloatMenuOption>();
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
                        var menu = new FloatMenu(list);
                        menu.vanishIfMouseDistant = true;
                        Find.WindowStack.Add(menu);
                    }
                };
        }
    }
}