using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{ 
    public enum NetworkMode
    {
        Alpha,
        Beta,
        Gamma,
        Delta,
        Epsilon
    }

    public class TiberiumNetwork
    {
        public MapComponent_TNWManager Manager;
        public StructureSet NetworkSet = new StructureSet(); 

        public int NetworkID = -1;
        public NetworkMode NetworkMode = NetworkMode.Alpha;
        public bool networkDirty = false;

        public TiberiumNetwork(MapComponent_TNWManager Manager)
        {
            this.Manager = Manager;
        }

        public TiberiumNetwork(CompTNW root, MapComponent_TNWManager Manager)
        {
            this.Manager = Manager;
            Manager.MakeNewNetwork(root, this);
            Manager.RegisterNetwork(this);
        }

        public void Tick()
        {
            SiloTick();
        }

        private void SiloTick()
        {
            foreach(CompTNW_Silo silo in NetworkSet.Silos)
            {
                if (silo.Container.ContainsForbiddenType)
                {
                    var forbiddenTypes = silo.Container.AllStoredTypes.Where(t => !silo.Container.AcceptsType(t));
                    foreach(TiberiumValueType type in forbiddenTypes)
                    {
                        var siloOther = SiloForType(type);
                        if (siloOther != null)
                        {
                            silo.Container.TryTransferTo(siloOther.Container, type, 5f);
                        }
                    }
                }
            }
        }

        public CompTNW_Silo SiloForType(TiberiumValueType valueType)
        {
            return NetworkSet.Silos.Find(s => !s.Container.CapacityFull && s.Container.AcceptsType(valueType));
        }

        public CompTNW_TNC NetworkController
        {
            get
            {
                return Manager.NetworkController;
            }
        }

        public Color GeneralColor
        {
            get
            {
                Color color = new Color(0, 0, 0, 0);
                if (!NetworkSet.Silos.NullOrEmpty())
                {
                    int count = NetworkSet.Silos.Count;
                    for(int i = 0; i < count; i++)
                    {
                        color += NetworkSet.Silos[i].Container.Color;
                    }
                    color /= count;
                }
                return color;
            }
        }

        public bool ValidFor(TNWMode mode, out string reason)
        {
            reason = string.Empty;
            switch (mode)
            {
                case TNWMode.Consumer:
                    reason = "TR_ConsumerLack";
                    return NetworkSet.FullList.Any(x => x.NetworkMode == TNWMode.Storage || x.NetworkMode == TNWMode.Producer);
                case TNWMode.Producer:
                    reason = "TR_ProducerLack";
                    return NetworkSet.FullList.Any(x => x.NetworkMode == TNWMode.Storage || x.NetworkMode == TNWMode.Consumer);
            }
            return true;
        }

        public bool IsWorking => NetworkController?.CompPower?.PowerOn ?? false;


        public float NetworkValueFor(TiberiumValueType valueType)
        {
            float value = 0;
            foreach (var silo in NetworkSet.Silos)
            {
                value += silo.Container.ValueForType(valueType);
            }
            return value;
        }

        public float NetworkValueFor(List<TiberiumValueType> types)
        {
            float value = 0;
            foreach(var silo in NetworkSet.Silos)
            {
                value += silo.Container.ValueForTypes(types);
            }
            return value;
        }

        public List<IntVec3> NetworkCells()
        {
            List<IntVec3> cells = new List<IntVec3>();
            foreach(CompTNW comp in NetworkSet.FullList)
            {
                cells.AddRange(comp.InnerConnectionCells);
            }
            return cells;
        }

        public void AddStructure(CompTNW tnwb)
        {
            NetworkSet.AddNewStructure(tnwb, tnwb.parent.Position);
        }

        public void MakeDirty()
        {
            networkDirty = true;
        }

        public void NotifyPotentialSplit(CompTNW from)
        {
            from.Network = null;
            TiberiumNetwork newNet = null;
            foreach (CompTNW root in from.StructureSet.FullList)
            {
                if (root.Network != newNet)
                {
                    newNet = root.Network = new TiberiumNetwork(root, Manager);
                }
            }
        }

        public string GreekLetter
        {
            get
            {
                switch (NetworkMode)
                {
                    case NetworkMode.Alpha:
                        return "α";
                    case NetworkMode.Beta:
                        return "β";
                    case NetworkMode.Gamma:
                        return "γ";
                    case NetworkMode.Delta:
                        return "δ";
                    case NetworkMode.Epsilon:
                        return "ε";
                }
                return "";
            }
        }
    }
}
