using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.UI;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class NetworkContainer : IExposable
{
    private List<NetworkValueDef> acceptedTypes;

    //Dynamic Data

    //
    private Gizmo_NetworkStorage containerGizmoInt;

    //Local Set Data
    private IContainerHolder parentHolder;
    private HashSet<NetworkValueDef> storedTypeCache;
    private Dictionary<NetworkValueDef, float> StoredValues = new();

    public NetworkContainer()
    {
    }

    public NetworkContainer(IContainerHolder parent)
    {
        parentHolder = parent;
        Capacity = Props.maxStorage;
    }

    public NetworkContainer(IContainerHolder parent, NetworkValueStack valueStack)
    {
        parentHolder = parent;
        Capacity = Props.maxStorage;
        AcceptedTypes = valueStack.AllTypes.ToList();
        foreach (var type in AcceptedTypes) Filter.Add(type, true);
        LoadFromStack(valueStack);
    }

    public NetworkContainer(IContainerHolder parent, List<NetworkValueDef> acceptedTypes)
    {
        parentHolder = parent;
        Capacity = Props.maxStorage;
        if (!acceptedTypes.NullOrEmpty())
        {
            AcceptedTypes = acceptedTypes;
            foreach (var type in AcceptedTypes) Filter.Add(type, true);
        }
        else
        {
            TLog.Warning($"Created NetworkContainer for {Parent?.Thing} without any allowed types!");
        }
        //TLog.Message($"Creating new container for {Parent?.Thing} with capacity {Capacity} | acceptedTypes: {this.AcceptedTypes.ToStringSafeEnumerable()}");
    }

    public string Title => parentHolder.ContainerTitle;

    //Capacity Values
    public float Capacity { get; private set; }

    public float TotalStored { get; private set; }

    public float StoredPercent => TotalStored / Capacity;

    //Capacity States
    public bool NotEmpty => TotalStored > 0;
    public bool Empty => TotalStored <= 0;
    public bool Full => TotalStored >= Capacity;

    //Misc States
    public bool ContainsForbiddenType => AllStoredTypes.Any(t => !AcceptsType(t));
    public bool HasStructureParent => parentHolder is IContainerHolderStructure;

    //
    public IContainerHolder Parent => parentHolder;

    public IContainerHolderStructure ParentStructure =>
        Parent is IContainerHolderStructure ? (IContainerHolderStructure)parentHolder : null;

    //
    public ContainerProperties Props => Parent.ContainerProps;

    //Values
    public NetworkValueDef MainValueType
    {
        get { return StoredValues.MaxBy(x => x.Value).Key; }
    }

    public HashSet<NetworkValueDef> AllStoredTypes
    {
        get { return storedTypeCache ??= new HashSet<NetworkValueDef>(); }
    }

    public Dictionary<NetworkValueDef, float> StoredValuesByType => StoredValues;
    public NetworkValueStack ValueStack { get; private set; }
    public Color Color { get; private set; }

    public Dictionary<NetworkValueDef, bool> Filter { get; private set; } = new();

    public List<NetworkValueDef> AcceptedTypes
    {
        get => acceptedTypes;
        set => acceptedTypes = value;
    }

    public Gizmo_NetworkStorage ContainerGizmo => containerGizmoInt ??= new Gizmo_NetworkStorage
    {
        container = this
    };

    public virtual void ExposeData()
    {
        Scribe_Collections.Look(ref StoredValues, "StoredTiberium");
        Scribe_Collections.Look(ref acceptedTypes, "acceptedTypes", LookMode.Def);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) UpdateContainerState(true);
    }

    public void Data_ChangeCapacity(int newCapacity)
    {
        Capacity = newCapacity;
    }

    public NetworkContainer Copy(IContainerHolder newHolder)
    {
        var newContainer = new NetworkContainer(newHolder, AcceptedTypes.ListFullCopy());
        newContainer.TotalStored = TotalStored;
        newContainer.AllStoredTypes.AddRange(AllStoredTypes);

        newContainer.StoredValues = StoredValues.Copy();
        newContainer.Filter = Filter.Copy();
        newContainer.UpdateContainerState(true);
        return newContainer;
    }

    //
    public void Parent_Destroyed(DestroyMode mode, Map previousMap)
    {
        if (Parent == null || TotalStored <= 0 || mode == DestroyMode.Vanish) return;
        if (mode is DestroyMode.Deconstruct or DestroyMode.Refund && Props.leaveContainer &&
            ParentStructure.NetworkComp.NetworkDef.portableContainerDef != null)
        {
            var container =
                (PortableContainer)ThingMaker.MakeThing(ParentStructure.NetworkComp.NetworkDef.portableContainerDef);
            container.SetupProperties(ParentStructure.NetworkComp.NetworkDef, Copy(container), Props);
            GenSpawn.Spawn(container, Parent.Thing.Position, previousMap);
        }

        if (mode is DestroyMode.KillFinalize)
        {
            if (Props.explosionProps != null)
                if (TotalStored > 0)
                    //float radius = Props.explosionProps.explosionRadius * StoredPercent;
                    //int damage = (int)(10 * StoredPercent);
                    //var mainTypeDef = MainValueType.dropThing;
                    Props.explosionProps.DoExplosion(Parent.Thing.Position, previousMap, Parent.Thing);
            //GenExplosion.DoExplosion(Parent.Thing.Position, previousMap, radius, DamageDefOf.Bomb, Parent.Thing, damage, 5, null, null, null, null, mainTypeDef, 0.18f);
            if (Props.dropContents)
            {
                var i = 0;
                var drops = PotentialItemDrops().ToList();
                Predicate<IntVec3> pred = c => c.InBounds(previousMap) && c.GetEdifice(previousMap) == null;
                var action = delegate(IntVec3 c)
                {
                    if (i < drops.Count)
                    {
                        var drop = drops[i];
                        if (drop != null)
                        {
                            GenSpawn.Spawn(drop, c, previousMap);
                            drops.Remove(drop);
                        }

                        i++;
                    }
                };
                _ = TeleFlooder.Flood(previousMap, Parent.Thing.OccupiedRect(), action, pred, drops.Count);
            }
        }

        Clear();
    }

    //Virtual Functions
    public virtual IEnumerable<Thing> PotentialItemDrops()
    {
        foreach (var storedValue in StoredValues)
        {
            if (storedValue.Key.thingDroppedFromContainer == null) continue;
            var count = Mathf.RoundToInt(storedValue.Value / storedValue.Key.valueToThingRatio);
            if (count <= 0) continue;
            yield return ThingMaker.MakeThing(storedValue.Key.thingDroppedFromContainer);
        }
    }

    //Helper Methods
    public void Notify_FilterChanged(NetworkValueDef def, bool state)
    {
        Filter[def] = state;
    }

    public void Notify_Full()
    {
        Parent?.Notify_ContainerFull();
    }

    public void Notify_AddedValue(NetworkValueDef valueType, float value)
    {
        TotalStored += value;
        ParentStructure?.ContainerSet?.Notify_AddedValue(valueType, value, ParentStructure.NetworkComp);
        AllStoredTypes.Add(valueType);

        //Update stack state
        UpdateContainerState();
    }

    public void Notify_RemovedValue(NetworkValueDef valueType, float value)
    {
        TotalStored -= value;
        ParentStructure?.ContainerSet?.Notify_RemovedValue(valueType, value, ParentStructure.NetworkComp);
        //TODO: Add value by role/
        if (AllStoredTypes.Contains(valueType) && ValueForType(valueType) <= 0)
            AllStoredTypes.RemoveWhere(v => v == valueType);

        //Update stack state
        UpdateContainerState();
    }

    public void LoadFromStack(NetworkValueStack stack)
    {
        Clear();
        foreach (var networkValue in stack.networkValues)
            TryAddValue(networkValue.valueDef, networkValue.valueF, out _);
    }

    public void Clear()
    {
        for (var i = StoredValues.Count - 1; i >= 0; i--)
        {
            var keyValuePair = StoredValues.ElementAt(i);
            TryRemoveValue(keyValuePair.Key, keyValuePair.Value, out _);
        }

        //
        UpdateContainerState();
    }

    public void FillWith(float wantedValue)
    {
        var val = wantedValue / AcceptedTypes.Count;
        foreach (var type in AcceptedTypes) TryAddValue(type, val, out var e);
    }

    //Transfer Functions
    public bool AcceptsType(NetworkValueDef valueType)
    {
        return Filter.TryGetValue(valueType, out var filterBool) && filterBool;
    }

    public bool CanFullyTransferTo(NetworkContainer other, float value)
    {
        return other.TotalStored + value <= other.Capacity;
    }

    // Value Functions
    public bool TryAddValue(NetworkValueDef valueType, float wantedValue, out float actualValue)
    {
        //If we add more than we can contain, we have an excess weight
        var excessValue = Mathf.Clamp(TotalStored + wantedValue - Capacity, 0, float.MaxValue);
        //The actual added weight is the wanted weight minus the excess
        actualValue = wantedValue - excessValue;

        //If the container is full, or doesnt accept the type, we dont add anything
        if (Full)
        {
            Notify_Full();
            return false;
        }

        if (!AcceptsType(valueType))
            return false;

        //If the weight type is already stored, add to it, if not, make a new entry
        if (StoredValues.ContainsKey(valueType))
            StoredValues[valueType] += actualValue;
        else
            StoredValues.Add(valueType, actualValue);

        Notify_AddedValue(valueType, actualValue);
        //If this adds the last drop, notify full
        if (Full)
            Notify_Full();

        return true;
    }

    public bool TryRemoveValue(NetworkValueDef valueType, float wantedValue, out float actualValue)
    {
        //Attempt to remove a certain weight from the container
        actualValue = wantedValue;
        if (StoredValues.TryGetValue(valueType, out var value) && value > 0)
        {
            if (value >= wantedValue)
                //If we have stored more than we need to pay, remove the wanted weight
            {
                StoredValues[valueType] -= wantedValue;
            }
            else if (value > 0)
            {
                //If not enough stored to "pay" the wanted weight, remove the existing weight and set actual removed weight to removed weight 
                StoredValues[valueType] = 0;
                actualValue = value;
            }
        }

        if (StoredValues[valueType] <= 0) StoredValues.Remove(valueType);

        Notify_RemovedValue(valueType, actualValue);
        return actualValue > 0;
    }

    public void TryTransferTo(NetworkContainer other, float value)
    {
        for (var i = AllStoredTypes.Count - 1; i >= 0; i--) TryTransferTo(other, AllStoredTypes.ElementAt(i), value);
    }

    public bool TryTransferTo(NetworkContainer other, NetworkValueDef valueType, float value)
    {
        //Attempt to transfer a weight to another container
        //Check if anything of that type is stored, check if transfer of weight is possible without loss, try remove the weight from this container
        if (!other.AcceptsType(valueType)) return false;
        if (StoredValues.TryGetValue(valueType) >= value && CanFullyTransferTo(other, value) &&
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
        if (TotalStored >= wantedValue)
        {
            var value = wantedValue;
            var allTypes = AllStoredTypes.ToArray();
            foreach (var type in allTypes)
                if (value > 0f && TryRemoveValue(type, value, out var leftOver))
                    value = leftOver;

            return true;
        }

        return false;
    }

    public bool TryConsume(NetworkValueDef valueType, float wantedValue)
    {
        if (ValueForType(valueType) >= wantedValue) return TryRemoveValue(valueType, wantedValue, out var leftOver);
        return false;
    }

    //Value
    public float ValueForTypes(List<NetworkValueDef> types)
    {
        float value = 0;
        foreach (var type in types)
            if (StoredValues.ContainsKey(type))
                value += StoredValues[type];

        return value;
    }

    public float ValueForType(NetworkValueDef valueType)
    {
        if (StoredValues.ContainsKey(valueType)) return StoredValues[valueType];
        return 0;
    }

    public bool PotentialCapacityFull(NetworkValueDef valueType, float potentialVal, out bool overfilled)
    {
        var val = potentialVal;
        foreach (var type2 in AllStoredTypes)
            if (!type2.Equals(valueType))
                val += StoredValues[type2];

        overfilled = val > Capacity;
        return val >= Capacity;
    }

    public void UpdateContainerState(bool updateMetaData = false)
    {
        //Set Stack
        ValueStack = new NetworkValueStack(StoredValues);

        //Update metadata
        if (updateMetaData)
        {
            TotalStored = ValueStack.TotalValue;
            AllStoredTypes.AddRange(ValueStack.AllTypes);
        }

        Color = Color.clear;

        if (StoredValues.Count > 0)
            foreach (var value in StoredValues)
                Color += value.Key.valueColor * (value.Value / Capacity);

        Parent?.Notify_ContainerStateChanged();
    }

    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        if (Capacity <= 0) yield break;


        if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(Parent.Thing)) yield return ContainerGizmo;

        /*
        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = $"DEBUG: Container Options {Props.maxStorage}",
                icon = TiberiumContent.ContainMode_TripleSwitch,
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("Add ALL", delegate
                    {
                        foreach (var type in AcceptedTypes)
                        {
                            TryAddValue(type, 500, out _);
                        }
                    }));
                    list.Add(new FloatMenuOption("Remove ALL", delegate
                    {
                        foreach (var type in AcceptedTypes)
                        {
                            TryRemoveValue(type, 500, out _);
                        }
                    }));
                    foreach (var type in AcceptedTypes)
                    {
                        list.Add(new FloatMenuOption($"Add {type}", delegate
                        {
                            TryAddValue(type, 500, out var _);
                        }));
                    }
                    FloatMenu menu = new FloatMenu(list, $"Add NetworkValue", true);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            };
        }
        */
    }
}