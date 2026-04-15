using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class FluidBox : IExposable
    {
        public TiberiumNetworkBuilding parent;
        public TiberiumContainer Container = new TiberiumContainer(20, StoreMode.Pipe, null);
        private int lastFlow = 0;

        public FluidBox(TiberiumNetworkBuilding parent)
        {
            this.parent = parent;
        }

        public void ExposeData() { }

        public void TransferTo(FluidBox to)
        {
            if (Container.GetTotalStorage >= 2)
            {
                int num = ThroughPut(to);
                if (num > 0)
                {
                    int value = num / Container.AllStoredTypes.Count;
                    foreach (TiberiumType type in this.Container.AllStoredTypes)
                    {
                        this.Container.TryTransferTo(to.Container, type, value);
                    }
                }
            }
        }

        public int ThroughPut(FluidBox to)
        {
            int result = (int)Math.Round(((float)this.Container.GetTotalStorage - (float)to.Container.GetTotalStorage) / 2f, 0, MidpointRounding.AwayFromZero);//(int)Mathf.Clamp(((this.Container.GetTotalStorage - to.Container.GetTotalStorage) / 2), 1, float.MaxValue);
            lastFlow = result;
            return result;
        }
    }
}
