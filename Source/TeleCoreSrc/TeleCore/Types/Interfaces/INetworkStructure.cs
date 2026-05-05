using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Types.Enums;
using TeleCore.Types.Exposables;
using Verse;

namespace TeleCore.Types.Interfaces;

public interface INetworkStructure
{
    //Data References
    public Thing Thing { get; }

    public List<NetworkPart> NetworkParts { get; }
    public NetworkIO GeneralIO { get; }

    //States
    public bool IsPowered { get; }
    public bool IsWorking { get; }

    //
    void NetworkPostTick(INetworkPart netPart, bool isPowered);

    //
    void Notify_ReceivedValue();

    //Methods
    void Notify_StructureAdded(INetworkStructure other);
    void Notify_StructureRemoved(INetworkStructure other);

    //
    bool RoleIsActive(NetworkRole role);
    bool AcceptsValue(NetworkValueDef value);
    bool CanInteractWith(INetworkPart other);
    bool CanConnectToOther(INetworkStructure other);

    //
    /* From TR
    public Thing Thing { get; }
    public NetworkType NetworkType { get; }
    public TiberiumProcessing.NetworkMode NetworkMode { get; }
    public NetworkStructureSet StructureSet { get; }
    public NetworkRole NetworkRole { get; }
    public Network Network { get; set; }
    public NetworkStructureSet StructureSet { get; }
    public object ContainerObject { get; }

    IEnumerable<IntVec3> ConnectionCells { get; }
     */
}