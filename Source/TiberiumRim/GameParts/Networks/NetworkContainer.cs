using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NetworkContainer : IExposable
    {
        private NetworkContainerSet parentSet;
        private Type valueType;

        private IContainerHolder parentHolder;
        private float totalCapacity;
        private float totalStorageCache;
        private List<Enum> storedTypeCache;
        private List<Enum> acceptedTypes;

        private Dictionary<Enum, bool> TypeFilter = new Dictionary<Enum, bool>();
        private Dictionary<Enum, float> StoredValues = new Dictionary<Enum, float>();


        public float TotalCapacity => totalCapacity;
        public float TotalStorage => totalStorageCache;
        public float StoredPercent => totalStorageCache / totalCapacity;

        public bool HasStorage => TotalStorage > 0;
        public bool Empty => !StoredValues.Any() || TotalStorage <= 0;
        public bool CapacityFull => TotalStorage >= totalCapacity;
        public bool ContainsForbiddenType => Enumerable.Any(AllStoredTypes, t => !AcceptsType(t));

        public Enum MainValueType
        {
            get
            {
                return StoredValues.MaxBy(x => x.Value).Key;
            }
        }

        public List<Enum> AllStoredTypes => storedTypeCache;
        public Dictionary<Enum, float> StoredValuesByType => StoredValues;

        public virtual Color Color => Color.white;

        public List<Enum> AcceptedTypes
        {
            get => acceptedTypes;
            set => acceptedTypes = value;
        }

        public IContainerHolder Parent => parentHolder;

        public NetworkContainer() { }

        public NetworkContainer(IContainerHolder parent, Type containedType)
        {
            this.parentHolder = parent;
            this.valueType = containedType;
        }

        public NetworkContainer(IContainerHolder parent, float capacity, Type containedType)
        {
            this.parentHolder = parent;
            this.totalCapacity = capacity;
            this.valueType = containedType;
        }

        public NetworkContainer(IContainerHolder parent, float capacity, List<Enum> acceptedTypes, Type containedType)
        {
            this.parentHolder = parent;
            this.totalCapacity = capacity;
            if (!acceptedTypes.NullOrEmpty())
                AcceptedTypes = acceptedTypes;
            this.valueType = containedType;
        }

        public NetworkContainer Copy(Thing thing)
        {
            NetworkContainer newContainer = new NetworkContainer();
            newContainer.totalCapacity = totalCapacity;
            newContainer.AcceptedTypes = AcceptedTypes.ListFullCopy();
            newContainer.TypeFilter = TypeFilter.Copy();
            foreach (Enum type in AllStoredTypes)
            {
                newContainer.TryAddValue(type, StoredValues[type], out float e);
            }
            return newContainer;
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref StoredValues, "StoredTiberium");
            Scribe_Collections.Look(ref acceptedTypes, "acceptedTypes");
            Scribe_Values.Look(ref totalCapacity, "capacity");
        }

        //Virtual Functions
        public virtual List<Thing> PotentialItemDrops()
        {
            return null;
        }

        //Helper Methods
        public void Notify_Full()
        {
            parentHolder?.Notify_ContainerFull();
        }

        public void Notify_AddedValue(Enum valueType, float value)
        {
            parentSet.Notify_AddedValue(valueType, value);
        }

        public void Notify_RemovedValue(Enum valueType, float value)
        {
            parentSet.Notify_RemovedValue(valueType, value);
        }

        public void Notify_SetParentSet(NetworkContainerSet parentSet)
        {
            this.parentSet = parentSet;
        }

        public void Clear()
        {
            StoredValues.RemoveAll(s => s.Value > 0);
        }

        public void FillWith(float wantedValue)
        {
            float val = wantedValue / AcceptedTypes.Count;
            foreach (Enum type in AcceptedTypes)
            {
                TryAddValue(type, val, out float e);
            }
        }

        //Transfer Functions
        public bool AcceptsType(Enum valueType)
        {
            return AcceptedTypes.Contains(valueType);
        }

        public bool CanFullyTransferTo(NetworkContainer other, float value)
        {
            return other.TotalStorage + value <= other.TotalCapacity;
        }

        // Value Functions
        public bool TryAddValue(Enum valueType, float wantedValue, out float actualValue)
        {
            //If we add more than we can contain, we have an excess weight
            var excessValue = Mathf.Clamp((TotalStorage + wantedValue) - totalCapacity, 0, float.MaxValue);
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
            if (StoredValues.TryGetValue(valueType, out float value))
                StoredValues[valueType] += actualValue;
            else
                StoredValues.Add(valueType, actualValue);

            //If this adds the last drop, notify full
            if (CapacityFull)
                Notify_Full();

            Notify_AddedValue(valueType, actualValue);
            return true;
        }

        public bool TryRemoveValue(Enum valueType, float wantedValue, out float actualValue)
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
            return actualValue == wantedValue;
        }

        public bool TryTransferTo(NetworkContainer other, Enum valueType, float value)
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
            if (TotalStorage >= wantedValue)
            {
                float value = wantedValue;
                foreach (Enum type in AllStoredTypes)
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

        public bool TryConsume(Enum valueType, float wantedValue)
        {
            if (ValueForType(valueType) >= wantedValue)
            {
                return TryRemoveValue(valueType, wantedValue, out float leftOver);
            }
            return false;
        }

        //Value
        public float ValueForTypes(List<Enum> types)
        {
            float value = 0;
            foreach (Enum type in types)
            {
                if (StoredValues.ContainsKey(type))
                {
                    value += StoredValues[type];
                }
            }
            return value;
        }

        public float ValueForType(Enum valueType)
        {
            if (StoredValues.ContainsKey(valueType))
            {
                return StoredValues[valueType];
            }
            return 0;
        }

        public bool PotentialCapacityFull(Enum valueType, float potentialVal, out bool overfilled)
        {
            float val = potentialVal;
            foreach (var type2 in AllStoredTypes)
            {
                if (!type2.Equals(valueType))
                {
                    val += StoredValues[type2];
                }
            }
            overfilled = val > TotalCapacity;
            return val >= TotalCapacity;
        }

        //
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}
