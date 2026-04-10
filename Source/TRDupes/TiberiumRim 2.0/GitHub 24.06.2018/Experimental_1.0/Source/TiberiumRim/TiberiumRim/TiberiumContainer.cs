using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumContainer : IExposable
    {
        public float maxStorage;

        private TiberiumType allowedType = TiberiumType.None;

        public Comp_TNW parent;

        private Dictionary<TiberiumType, float> ContainedTiberium = new Dictionary<TiberiumType, float>();

        public TiberiumContainer(float maxStorage, Comp_TNW parent = null) : base()
        {
            this.maxStorage = maxStorage;
            this.parent = parent;
        }

        public TiberiumContainer() : base()
        {
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref ContainedTiberium, "ContainedTinerium");
            Scribe_Values.Look(ref maxStorage, "maxStorage");
            Scribe_Values.Look(ref allowedType, "allowedType");
            Scribe_Values.Look(ref maxStorage, "maxStorage");
        }

        public Color Color
        {
            get
            {
                Color newColor = new Color();
                if (parent != null && !parent.IsStorage)
                {
                    if (newColor == new Color())
                    {
                        newColor = TypeColor(TiberiumType.Green);
                        newColor.a = 1;
                    }
                    return newColor;
                }
                if (ContainedTiberium.Keys.Count > 0)
                {
                    foreach (TiberiumType type in ContainedTiberium.Keys)
                    {
                        newColor += TypeColor(type) * (ContainedTiberium[type] / maxStorage);
                    }
                }
                return newColor;
            }
        }

        public Color TypeColor (TiberiumType type)
        {
            Color color = Color.white;
            TiberiumControlDef def = MainTCD.MainTiberiumControlDef;
            switch (type) {
                case TiberiumType.Green:
                    color = def.GreenColor;
                    break;
                case TiberiumType.Blue:
                    color = def.BlueColor;
                    break;
                case TiberiumType.Red:
                    color = def.RedColor;
                    break;
                case TiberiumType.Sludge:
                    color = def.SludgeColor;
                    break;
            }
            return color;
        }

        public TiberiumType MainType
        {
            get
            {
                float maxFlt = 0f;
                TiberiumType tibType = TiberiumType.None;
                foreach(TiberiumType type in ContainedTiberium.Keys)
                {
                    if (maxFlt < ContainedTiberium[type])
                    {
                        maxFlt = ContainedTiberium[type];
                        tibType = type;
                    }
                }
                return tibType;
            }
        }

        public List<TiberiumType> GetTypes
        {
            get
            {
                return ContainedTiberium.Keys.ToList();
            }
        }

        public float ValueForType(TiberiumType type)
        {
            if (ContainedTiberium.ContainsKey(type))
            {
                return ContainedTiberium[type];
            }
            return 0f;
        }

        public float StoredPct
        {
            get
            {
                return GetTotalStorage / maxStorage;
            }
        }

        public float GetTotalStorage
        {
            get
            {
                float flt = 0f;
                foreach(TiberiumType type in ContainedTiberium.Keys)
                {                
                    flt += ContainedTiberium[type];
                }
                return flt;
            }
        }

        public bool CapacityFull
        {
            get
            {
                return GetTotalStorage >= maxStorage;
            }
        }

        public bool ShouldUnloadSludgeInContainer(out TiberiumContainer container)
        {
            if (ContainedTiberium.ContainsKey(TiberiumType.Sludge) && ContainedTiberium[TiberiumType.Sludge] > 0)
            {
                Comp_TNW comp = parent.Connector.Network.AllStorages.Find((Comp_TNW x) => x.Container.allowedType == TiberiumType.Sludge && !x.Container.CapacityFull && x != this.parent);
                if (comp != null)
                {
                    container = comp.Container;
                    return true;
                }
            }
            container = null;
            return false;
        }

        public bool ShouldEmptyUnallowedTypes
        {
            get
            {
                if (allowedType != TiberiumType.None)
                {
                    bool flag = false;
                    foreach (TiberiumType type in ContainedTiberium.Keys)
                    {
                        if (type != allowedType && !flag)
                        {
                            flag = true;
                        }
                    }
                    return flag;
                }
                return false;
            }
        }

        public void SetAllowedType(TiberiumType type)
        {
            this.allowedType = type;
        }


        public bool AcceptsType(TiberiumType type)
        {
            if (allowedType != TiberiumType.None)
            {
                if (type != this.allowedType)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddCrystal(TiberiumType type, float value, out float excess)
        {
            excess = 0;
            float value2 = value;
            if (CapacityFull)
            {
                excess = value;
                return false;
            }
            if (ContainedTiberium.ContainsKey(type))
            {
                if(ContainedTiberium[type] + value > maxStorage)
                {
                    excess = (ContainedTiberium[type] + value) - maxStorage;
                    value2 = value - excess;
                }
            }
            if (!ContainedTiberium.ContainsKey(type))
            {
                ContainedTiberium.Add(type, value);
               return true;
            }
            ContainedTiberium[type] += value2;
            return true;
        }

        public void RemoveCrystal(TiberiumType type, float value)
        {
            if (ContainedTiberium.ContainsKey(type))
            {
                ContainedTiberium[type] -= value;
                if(ContainedTiberium[type] < 0)
                {
                    ContainedTiberium.Remove(type);
                }
            }
        }
    }
}
