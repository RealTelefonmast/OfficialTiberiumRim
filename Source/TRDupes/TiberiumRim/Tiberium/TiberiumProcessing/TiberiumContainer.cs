using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public enum StoreMode
    {
        None,
        RGB,
        Red,
        Green,
        Blue,
        Gas,
        Sludge,
        Pipe    
    }

    public class TiberiumContainer : IExposable
    {
        public TiberiumNetworkBuilding parent;
        public int maxCapacity;
        public IntRange pressure = new IntRange(0, 100);

        //
        public StoreMode mode = StoreMode.RGB;
        private Dictionary<TiberiumType, int> StoredTiberium = new Dictionary<TiberiumType, int>();

        //
        public TiberiumContainer() { }

        public TiberiumContainer(TiberiumNetworkBuilding parent)
        {
            this.parent = parent;
        }

        public TiberiumContainer(int max, StoreMode storeMode = StoreMode.RGB, TiberiumNetworkBuilding parent = null)
        {
            maxCapacity = max;
            mode = storeMode;
            this.parent = parent;
        }

        public void MakeCopy(TiberiumContainer container)
        {
            maxCapacity += container.maxCapacity;
            foreach(TiberiumType type in container.AllStoredTypes)
            {
                StoredTiberium.Add(type, container.StoredTiberium[type]);
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref StoredTiberium, "StoredTiberium");
            Scribe_Values.Look(ref maxCapacity, "capacity");
            Scribe_Values.Look(ref mode, "mode");
        }

        public void Clear()
        {
            StoredTiberium.RemoveAll(s => s.Value > 0);
        }

        public bool AcceptsType(TiberiumType type)
        {
            if(mode == StoreMode.None)
            {
                return false;
            }
            switch (mode)
            {
                case StoreMode.Pipe:
                    return true;
                case StoreMode.RGB:
                    return type == TiberiumType.Red || type == TiberiumType.Green || type == TiberiumType.Blue;
                case StoreMode.Red:
                    return type == TiberiumType.Red;
                case StoreMode.Green:
                    return type == TiberiumType.Green;
                case StoreMode.Blue:
                    return type == TiberiumType.Blue;
                case StoreMode.Gas:
                    return type == TiberiumType.Gas;
                case StoreMode.Sludge:
                    return type == TiberiumType.Sludge;
            }
            return false;
        }

        public bool CanConnectTo(TiberiumContainer other)
        {
            if(mode == StoreMode.Pipe || other.mode == StoreMode.Pipe)
            {
                return true;
            }
            return mode == other.mode;
        }

        public bool CanEvenOutWith(TiberiumContainer other, int value)
        {
            int totalCapacity = maxCapacity + other.maxCapacity;
            int totalValue = GetTotalStorage + other.GetTotalStorage;
            return totalValue + Mathf.Abs(value) <= totalCapacity;
        }

        public bool TryTransferTo(TiberiumContainer otherContainer, TiberiumType type, int value)
        {
            //Log.Message("From: " + GetTotalStorage + " | To: " + otherContainer.GetTotalStorage + " | With: " + value);
            if (CanEvenOutWith(otherContainer, value) && otherContainer.AcceptsType(type) && StoredTiberium.ContainsKey(type) && StoredTiberium[type] > 0)
            {
                RemoveValue(type,value, out int removedValue);
                otherContainer.TryAddValue(type, removedValue, out int excess);
                if(excess > 0)
                {
                    TryAddValue(type, excess, out int excess2);
                }
                return true;
            }
            return false;
        }

        public bool TryAddValue(TiberiumType type, int value, out int excess)
        {
            excess = 0;
            int value2 = value;           
            if (CapacityFull || !AcceptsType(type))
            {
                excess = value;
                return false;
            }
            if (!StoredTiberium.ContainsKey(type))
            {
                StoredTiberium.Add(type, 0);
            }
            int potTotal = StoredTiberium[type] + value;
            if (potTotal > maxCapacity)
            {
                excess = potTotal - maxCapacity;
                value2 = value - excess;
            }
            StoredTiberium[type] += value2;
            return true;
        }

        public bool RemoveValue(TiberiumType type, int wantedValue, out int removedValue)
        {
            removedValue = wantedValue;
            if (StoredTiberium.ContainsKey(type) && StoredTiberium[type] > 0f)
            {
                int leftOver = StoredTiberium[type] - wantedValue;
                if(leftOver >= 0)
                {
                    StoredTiberium[type] -= wantedValue;
                }
                else
                {
                    removedValue = wantedValue - Math.Abs(leftOver);
                    StoredTiberium[type] -= removedValue;
                }
                return true;
            }
            return false;
        }

        public Color Color
        {
            get
            {
                Color color = new Color();
                if(StoredTiberium.Count > 0)
                {
                    foreach(TiberiumType type in StoredTiberium.Keys)
                    {
                        color += TRUtils.ColorForType(type) * ((float)StoredTiberium[type] / (float)maxCapacity);
                    }
                }
                return color;
            }
        }     

        public TiberiumType MainType
        {
            get
            {
                return StoredTiberium.MaxBy(x => x.Value).Key;
            }
        }

        public List<TiberiumType> AllStoredTypes
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
            foreach(TiberiumType type in AllStoredTypes)
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

        public float Pressure
        {
            get
            {
                return Mathf.Lerp(pressure.min, pressure.max, StoredPct);
            }
        }

        public int ValueForType(TiberiumType type)
        {
            if (StoredTiberium.ContainsKey(type))
            {
                return StoredTiberium[type];
            }
            return 0;
        }

        public float StoredPct
        {
            get
            {
                return (float)GetTotalStorage / (float)maxCapacity;
            }
        }

        public int GetTotalStorage
        {
            get
            {
                if (StoredTiberium.Count > 0)
                {
                    return Mathf.Clamp(StoredTiberium.Sum(t => t.Value), 0, maxCapacity);
                }
                return 0;
            }
        }

        public bool PotentialCapactiyFull(TiberiumType type, float potentialVal, out bool overfilled)
        {
            float val = potentialVal;
            overfilled = false;
            foreach(TiberiumType type2 in AllStoredTypes)
            {
                if(type2 != type)
                {
                    val += StoredTiberium[type2];
                }
            }
            if(val > maxCapacity)
            {
                overfilled = true;
            }
            return val >= maxCapacity;
        }

        public bool Empty
        {
            get
            {
                return StoredTiberium.Count == 0 || GetTotalStorage == 0f;
            }
        }

        public bool CapacityFull
        {
            get
            {
                return GetTotalStorage >= maxCapacity;
            }
        }

        public bool ContainForbiddenType
        {
            get
            {
                return AllStoredTypes.Any(t => !AcceptsType(t));
            }
        }
    }
}
