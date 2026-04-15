using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

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
        public TNW_TNC Parent;
        public MapComponent_TNWManager Manager;
        public StructureSet NetworkSet = new StructureSet(); 

        public int NetworkID = -1;
        public NetworkMode NetworkMode = NetworkMode.Alpha;
        public StoreMode StoreMode = StoreMode.RGB;

        public TiberiumNetwork() { }

        public TiberiumNetwork(TiberiumNetworkBuilding Root, MapComponent_TNWManager Manager, NetworkMode predefined)
        {
            this.Manager = Manager;
            NetworkMode = predefined;
            NetworkFlood(Root, this);
            Manager.RegisterNetwork(this);
        }

        public TiberiumNetwork(TNW_TNC parent, MapComponent_TNWManager Manager, TiberiumNetworkBuilding tnwb = null, List<TiberiumNetwork> networks = null, NetworkMode nMode = NetworkMode.Alpha)
        {
            if (parent != null)
            {
                //Log.Error("Trying to set up TiberiumNetwork without TNC parent!");
                this.Parent = parent;
                parent.Network = this;
            }
            NetworkMode = nMode;
            this.Manager = Manager;                        
            
            if (tnwb != null)
            {
                AddStructure(tnwb);
            }
            if (!networks.NullOrEmpty())
            {
                foreach (TiberiumNetwork network in networks)
                {
                    if (network != this)
                    {
                        NetworkSet.MergeWith(network.NetworkSet, this);
                    }
                }
            }
            Manager.RegisterNetwork(this);
        }

        public bool IsActive
        {
            get
            {
                return Parent?.CompTNW.compPower.PowerOn ?? false;
            }
        }

        public void AddStructure(TiberiumNetworkBuilding tnwb)
        {
            NetworkSet.AddNewStructure(tnwb);
        }

        public void UpdateTiberiumNetwork(TiberiumNetworkBuilding tnwb)
        {
            NetworkSet.RemoveStructure(tnwb);
            List<TiberiumNetworkBuilding> structures = tnwb.StructureSet.FullList;
            TiberiumNetwork newNet = null;
            foreach (TiberiumNetworkBuilding begin in structures)
            {
                if (!begin.DestroyedOrNull())
                {
                    if (begin.Network != newNet)
                    {
                        newNet = new TiberiumNetwork(Parent, begin.Manager, begin);
                        NetworkFlood(begin, newNet);
                    }
                }
            }
        }

        private static void NetworkFlood(TiberiumNetworkBuilding root, TiberiumNetwork newNet)
        {
            HashSet<TiberiumNetworkBuilding> closedSet = new HashSet<TiberiumNetworkBuilding>();
            HashSet<TiberiumNetworkBuilding> openSet = new HashSet<TiberiumNetworkBuilding>() { root };
            HashSet<TiberiumNetworkBuilding> currentSet = new HashSet<TiberiumNetworkBuilding>();
            while (openSet.Count > 0)
            {
                foreach (TiberiumNetworkBuilding item in openSet)
                {
                    item.Network = newNet;
                    newNet.AddStructure(item);
                    closedSet.Add(item);
                }
                HashSet<TiberiumNetworkBuilding> hashSet = currentSet;
                currentSet = openSet;
                openSet = hashSet;
                openSet.Clear();
                foreach (TiberiumNetworkBuilding tnwb in currentSet)
                {
                    foreach(IntVec3 c in tnwb.CardinalConnectableCells)
                    {
                        List<Thing> thingList = c.GetThingList(tnwb.Map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            if(thingList[i] is TiberiumNetworkBuilding newTnwb && !closedSet.Contains(newTnwb) && newTnwb.CanConnectTo(c, tnwb))
                            {
                                tnwb.StructureSet.AddNewStructure(newTnwb, c);
                                newTnwb.StructureSet.AddNewStructure(tnwb, GenAdj.CellsAdjacentCardinal(newTnwb).Where(cell => tnwb.ConnectableCells.Contains(cell)).First());
                                openSet.Add(newTnwb);
                                break;
                            }
                        }
                    }
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
