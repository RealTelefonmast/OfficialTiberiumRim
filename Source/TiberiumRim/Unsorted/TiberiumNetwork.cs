using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim;

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

    public int NetworkID = -1;
    public NetworkMode NetworkMode = NetworkMode.Alpha;
    public StructureSet NetworkSet = new();
    public TNW_TNC Parent;
    public StoreMode StoreMode = StoreMode.RGB;

    public TiberiumNetwork()
    {
    }

    public TiberiumNetwork(TiberiumNetworkBuilding Root, MapComponent_TNWManager Manager, NetworkMode predefined)
    {
        this.Manager = Manager;
        NetworkMode = predefined;
        NetworkFlood(Root, this);
        Manager.RegisterNetwork(this);
    }

    public TiberiumNetwork(TNW_TNC parent, MapComponent_TNWManager Manager, TiberiumNetworkBuilding tnwb = null,
        List<TiberiumNetwork> networks = null, NetworkMode nMode = NetworkMode.Alpha)
    {
        if (parent != null)
        {
            //Log.Error("Trying to set up TiberiumNetwork without TNC parent!");
            Parent = parent;
            parent.Network = this;
        }

        NetworkMode = nMode;
        this.Manager = Manager;

        if (tnwb != null) AddStructure(tnwb);
        if (!networks.NullOrEmpty())
            foreach (var network in networks)
                if (network != this)
                    NetworkSet.MergeWith(network.NetworkSet, this);

        Manager.RegisterNetwork(this);
    }

    public bool IsActive => Parent?.CompTNW.compPower.PowerOn ?? false;

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

    public void AddStructure(TiberiumNetworkBuilding tnwb)
    {
        NetworkSet.AddNewStructure(tnwb);
    }

    public void UpdateTiberiumNetwork(TiberiumNetworkBuilding tnwb)
    {
        NetworkSet.RemoveStructure(tnwb);
        var structures = tnwb.StructureSet.FullList;
        TiberiumNetwork newNet = null;
        foreach (var begin in structures)
            if (!begin.DestroyedOrNull())
                if (begin.Network != newNet)
                {
                    newNet = new TiberiumNetwork(Parent, begin.Manager, begin);
                    NetworkFlood(begin, newNet);
                }
    }

    private static void NetworkFlood(TiberiumNetworkBuilding root, TiberiumNetwork newNet)
    {
        var closedSet = new HashSet<TiberiumNetworkBuilding>();
        var openSet = new HashSet<TiberiumNetworkBuilding> { root };
        var currentSet = new HashSet<TiberiumNetworkBuilding>();
        while (openSet.Count > 0)
        {
            foreach (var item in openSet)
            {
                item.Network = newNet;
                newNet.AddStructure(item);
                closedSet.Add(item);
            }

            var hashSet = currentSet;
            currentSet = openSet;
            openSet = hashSet;
            openSet.Clear();
            foreach (var tnwb in currentSet)
            foreach (var c in tnwb.CardinalConnectableCells)
            {
                var thingList = c.GetThingList(tnwb.Map);
                for (var i = 0; i < thingList.Count; i++)
                    if (thingList[i] is TiberiumNetworkBuilding newTnwb && !closedSet.Contains(newTnwb) &&
                        newTnwb.CanConnectTo(c, tnwb))
                    {
                        tnwb.StructureSet.AddNewStructure(newTnwb, c);
                        newTnwb.StructureSet.AddNewStructure(tnwb,
                            GenAdj.CellsAdjacentCardinal(newTnwb).Where(cell => tnwb.ConnectableCells.Contains(cell))
                                .First());
                        openSet.Add(newTnwb);
                        break;
                    }
            }
        }
    }
}