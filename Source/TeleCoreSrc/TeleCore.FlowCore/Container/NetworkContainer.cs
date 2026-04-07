using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class NetworkContainer : IExposable
    {
        //Local Set Data
        private IContainerHolder parentHolder;
        private float totalCapacity;
        private List<NetworkValueDef> acceptedTypes;

        //Dynamic Data
        private Color colorInt;
        private float totalStoredCache;
        private HashSet<NetworkValueDef> storedTypeCache;

        private Dictionary<NetworkValueDef, bool> TypeFilter = new();
        private Dictionary<NetworkValueDef, float> StoredValues = new();

        public string Title => parentHolder.ContainerTitle;

        //Capacity Values
        public float Capacity => totalCapacity;
        public float TotalStored => totalStoredCache;
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
        public IContainerHolderStructure ParentStructure => Parent is IContainerHolderStructure ? (IContainerHolderStructure)parentHolder : null;

        //
        public ContainerProperties Props => Parent.ContainerProps;

        //Values
        public NetworkValueDef MainValueType
        {
            get
            {
                return StoredValues.MaxBy(x => x.Value).Key;
            }
        }

        public HashSet<NetworkValueDef> AllStoredTypes
        {
            get { return storedTypeCache ??= new HashSet<NetworkValueDef>(); }
        }

        public Dictionary<NetworkValueDef, float> StoredValuesByType => StoredValues;
        public NetworkValueStack ValueStack { get; private set; }
        public Color Color => colorInt;

        public Dictionary<NetworkValueDef, bool> Filter => TypeFilter;
        public List<NetworkValueDef> AcceptedTypes
        {
            get => acceptedTypes;
            set => acceptedTypes = value;
        }

        public NetworkContainer() { }

        public NetworkContainer(IContainerHolder parent)
        {
            this.parentHolder = parent;
            this.totalCapacity = Props.maxStorage;
        }

        public NetworkContainer(IContainerHolder parent, NetworkValueStack valueStack)
        {
            this.parentHolder = parent;
            this.totalCapacity = Props.maxStorage;
            AcceptedTypes = valueStack.AllTypes.ToList();
            foreach (var type in AcceptedTypes)
            {
                TypeFilter.Add(type, true);
            }
            LoadFromStack(valueStack);
        }

        public NetworkContainer(IContainerHolder parent, List<NetworkValueDef> acceptedTypes)
        {
            this.parentHolder = parent;
            this.totalCapacity = Props.maxStorage;
            if (!acceptedTypes.NullOrEmpty())
            {
                AcceptedTypes = acceptedTypes;
                foreach (var type in AcceptedTypes)
                {
                    TypeFilter.Add(type, true);
                }
            }
            else
            {
                TLog.Warning($"Created NetworkContainer for {Parent?.Thing} without any allowed types!");
            }
            //TLog.Message($"Creating new container for {Parent?.Thing} with capacity {Capacity} | acceptedTypes: {this.AcceptedTypes.ToStringSafeEnumerable()}");
        }

        public void Data_ChangeCapacity(int newCapacity)
        {
            totalCapacity = newCapacity;
        }

        public NetworkContainer Copy(IContainerHolder newHolder)
        {
            NetworkContainer newContainer = new NetworkContainer(newHolder, AcceptedTypes.ListFullCopy());
            newContainer.totalStoredCache = TotalStored;
            newContainer.AllStoredTypes.AddRange(AllStoredTypes);

            newContainer.StoredValues = StoredValues.Copy();
            newContainer.TypeFilter = TypeFilter.Copy();
            newContainer.UpdateContainerState(true);
            return newContainer;
        }

        //
        public void Parent_Destroyed(DestroyMode mode, Map previousMap)
        {
            if (Parent == null || TotalStored <= 0 || mode == DestroyMode.Vanish) return;
            if ((mode is DestroyMode.Deconstruct or DestroyMode.Refund) && Props.leaveContainer && ParentStructure.NetworkComp.NetworkDef.portableContainerDef != null)
            {
                PortableContainer container = (PortableContainer)ThingMaker.MakeThing(ParentStructure.NetworkComp.NetworkDef.portableContainerDef);
                container.SetupProperties(ParentStructure.NetworkComp.NetworkDef, Copy(container), Props);
                GenSpawn.Spawn(container, Parent.Thing.Position, previousMap);
            }

            if (mode is DestroyMode.KillFinalize)
            {
                if (Props.explosionProps != null)
                {
                    if (TotalStored > 0)
                    {
                        //float radius = Props.explosionProps.explosionRadius * StoredPercent;
                        //int damage = (int)(10 * StoredPercent);
                        //var mainTypeDef = MainValueType.dropThing;
                        Props.explosionProps.DoExplosion(Parent.Thing.Position, previousMap, Parent.Thing);

                        //GenExplosion.DoExplosion(Parent.Thing.Position, previousMap, radius, DamageDefOf.Bomb, Parent.Thing, damage, 5, null, null, null, null, mainTypeDef, 0.18f);
                    }
                }

                if (Props.dropContents)
                {
                    int i = 0;
                    List<Thing> drops = this.PotentialItemDrops().ToList();
                    Predicate<IntVec3> pred = c => c.InBounds(previousMap) && c.GetEdifice(previousMap) == null;
                    Action<IntVec3> action = delegate(IntVec3 c)
                    {
                        if (i < drops.Count)
                        {
                            Thing drop = drops[i];
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

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref StoredValues, "StoredTiberium");
            Scribe_Collections.Look(ref acceptedTypes, "acceptedTypes", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateContainerState(true);
            }
        }

        //Virtual Functions
        public virtual IEnumerable<Thing> PotentialItemDrops()
        {
            foreach (var storedValue in StoredValues)
            {
                if(storedValue.Key.thingDroppedFromContainer == null) continue;
                int count = Mathf.RoundToInt(storedValue.Value / storedValue.Key.valueToThingRatio);
                if(count <= 0) continue;
                yield return ThingMaker.MakeThing(storedValue.Key.thingDroppedFromContainer);
            }
            yield break;
        }

        //Helper Methods
        public void Notify_FilterChanged(NetworkValueDef def, bool state)
        {
            TypeFilter[def] = state;
        }

        public void Notify_Full()
        {
            Parent?.Notify_ContainerFull();
        }

        public void Notify_AddedValue(NetworkValueDef valueType, float value)
        {
            totalStoredCache += value;
            ParentStructure?.ContainerSet?.Notify_AddedValue(valueType, value, ParentStructure.NetworkComp);
            AllStoredTypes.Add(valueType);

            //Update stack state
            UpdateContainerState();
        }

        public void Notify_RemovedValue(NetworkValueDef valueType, float value)
        {
            totalStoredCache -= value;
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
            {
                TryAddValue(networkValue.valueDef, networkValue.valueF, out _);
            }
        }

        public void Clear()
        {
            for (int i = StoredValues.Count - 1; i >= 0; i--)
            {
                var keyValuePair = StoredValues.ElementAt(i);
                TryRemoveValue(keyValuePair.Key, keyValuePair.Value, out _);
            }

            //
            UpdateContainerState();
        }

        public void FillWith(float wantedValue)
        {
            float val = wantedValue / AcceptedTypes.Count;
            foreach (NetworkValueDef type in AcceptedTypes)
            {
                TryAddValue(type, val, out float e);
            }
        }

        //Transfer Functions
        public bool AcceptsType(NetworkValueDef valueType)
        {
            return TypeFilter.TryGetValue(valueType, out bool filterBool) && filterBool;
        }

        public bool CanFullyTransferTo(NetworkContainer other, float value)
        {
            return other.TotalStored + value <= other.Capacity;
        }

        // Value Functions
        public bool TryAddValue(NetworkValueDef valueType, float wantedValue, out float actualValue)
        {
            //If we add more than we can contain, we have an excess weight
            var excessValue = Mathf.Clamp((TotalStored + wantedValue) - Capacity, 0, float.MaxValue);
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
            if (StoredValues.TryGetValue(valueType, out float value) && value > 0)
            {
                if (value >= wantedValue)
                    //If we have stored more than we need to pay, remove the wanted weight
                    StoredValues[valueType] -= wantedValue;
                else if (value > 0)
                {
                    //If not enough stored to "pay" the wanted weight, remove the existing weight and set actual removed weight to removed weight 
                    StoredValues[valueType] = 0;
                    actualValue = value;
                }
            }

            if (StoredValues[valueType] <= 0)
            {
                StoredValues.Remove(valueType);
            }

            Notify_RemovedValue(valueType, actualValue);
            return actualValue > 0;
        }

        public void TryTransferTo(NetworkContainer other, float value)
        {
            for (int i = AllStoredTypes.Count - 1; i >= 0; i--)
            {
                TryTransferTo(other, AllStoredTypes.ElementAt(i), value);
            }
        }

        public bool TryTransferTo(NetworkContainer other, NetworkValueDef valueType, float value)
        {
            //Attempt to transfer a weight to another container
            //Check if anything of that type is stored, check if transfer of weight is possible without loss, try remove the weight from this container
            if (!other.AcceptsType(valueType)) return false;
            if (StoredValues.TryGetValue(valueType) >= value && CanFullyTransferTo(other, value) && TryRemoveValue(valueType, value, out float actualValue))
            {
                //If passed, try to add the actual weight removed from this container, to the other.
                other.TryAddValue(valueType, actualValue, out float actualAddedValue);
                return true;
            }
            return false;
        }

        public bool TryConsume(float wantedValue)
        {
            if (TotalStored >= wantedValue)
            {
                float value = wantedValue;
                var allTypes = AllStoredTypes.ToArray();
                foreach (NetworkValueDef type in allTypes)
                {
                    if (value > 0f && TryRemoveValue(type, value, out float leftOver))
                    {
                        value = leftOver;
                    }
                }
                return true;
            }
            return false;
        }

        public bool TryConsume(NetworkValueDef valueType, float wantedValue)
        {
            if (ValueForType(valueType) >= wantedValue)
            {
                return TryRemoveValue(valueType, wantedValue, out float leftOver);
            }
            return false;
        }

        //Value
        public float ValueForTypes(List<NetworkValueDef> types)
        {
            float value = 0;
            foreach (NetworkValueDef type in types)
            {
                if (StoredValues.ContainsKey(type))
                {
                    value += StoredValues[type];
                }
            }
            return value;
        }

        public float ValueForType(NetworkValueDef valueType)
        {
            if (StoredValues.ContainsKey(valueType))
            {
                return StoredValues[valueType];
            }
            return 0;
        }

        public bool PotentialCapacityFull(NetworkValueDef valueType, float potentialVal, out bool overfilled)
        {
            float val = potentialVal;
            foreach (var type2 in AllStoredTypes)
            {
                if (!type2.Equals(valueType))
                {
                    val += StoredValues[type2];
                }
            }
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
                totalStoredCache = ValueStack.TotalValue;
                AllStoredTypes.AddRange(ValueStack.AllTypes);
            }
            colorInt = Color.clear;

            if (StoredValues.Count > 0)
            {
                foreach (var value in StoredValues)
                {
                    colorInt += value.Key.valueColor * (value.Value / Capacity);
                }
            }
            Parent?.Notify_ContainerStateChanged();
        }

        //
        private Gizmo_NetworkStorage containerGizmoInt;

        public Gizmo_NetworkStorage ContainerGizmo => containerGizmoInt ??= new Gizmo_NetworkStorage()
        {
            container = this
        };

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            if (Capacity <= 0) yield break;


            if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(Parent.Thing))
            {
                yield return ContainerGizmo;
            }

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
}
