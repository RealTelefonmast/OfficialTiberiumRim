using System.Collections.Generic;
using TR.GameParts.Networks;
using Verse;

namespace TR.Info;

public class NetworkMapInfo : MapInformation
{
    private readonly Dictionary<NetworkType, NetworkMaster> NetworksByType = new();
    private readonly List<NetworkMaster> NetworkSystems = new();

    public NetworkMapInfo(Map map) : base(map)
    {
    }

    public bool TryStartNewNetworkMaster(NetworkType type, out NetworkMaster network)
    {
        if (NetworksByType.TryGetValue(type, out network)) return false;
        var networkMaster = new NetworkMaster(Map, type);
        NetworksByType.Add(type, networkMaster);
        NetworkSystems.Add(networkMaster);
        return true;
    }

    public override void Tick()
    {
        base.Tick();
        foreach (var networkSystem in NetworkSystems) networkSystem.TickNetwork();
    }

    public override void Draw()
    {
        base.Draw();
        foreach (var networkSystem in NetworkSystems) networkSystem.DrawNetwork();
    }
}