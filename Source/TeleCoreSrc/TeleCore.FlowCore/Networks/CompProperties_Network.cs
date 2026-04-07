using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.FlowCore;

/// <summary>
/// </summary>
public class CompProperties_Network : CompProperties
{
    [Description("Default IO configuration for this structure. Any sub-IOConfigs will override these settings.")]
    public NetIOConfig generalIOConfig = new NetIOConfig();

    [Description("Custom individual configurations for how this structure should interact with other networks.")]
    public List<NetworkPartConfig>? networks;

    [Description("When set, applies this config for each existing NetworkDef loaded, providing a universal network structure")]
    public DefaultNetworkPartConfig? defaultNetworkPartConfig = null;

    public CompProperties_Network()
    {
        compClass = typeof(CompNetwork);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }

        if (networks is { Count: > 0 } && defaultNetworkPartConfig != null)
        {
            yield return $"Networks defined in {parentDef}.comps[{nameof(CompProperties_Network)}]{nameof(networks)} will not be applied and instead overriden by {nameof(defaultNetworkPartConfig)}!";
        }
    }

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);
        generalIOConfig.PostLoadCustom(parent);

        if (defaultNetworkPartConfig != null)
        {
            networks.Clear();
            networks = new List<NetworkPartConfig>();
            foreach (var networkDef in DefDatabase<NetworkDef>.AllDefs)
            {
                var networkPartConfig = new NetworkPartConfig();
                networkPartConfig.networkDef = networkDef;
                networkPartConfig.roles = defaultNetworkPartConfig.roles;
                networkPartConfig.netIOConfig = defaultNetworkPartConfig.netIOConfig;
                networkPartConfig.volumeConfig = defaultNetworkPartConfig.volumeConfig;
                networks.Add(networkPartConfig);
            }
        }

        foreach (var network in networks)
            network.PostLoadSpecial(parent);
    }
}

public class DefaultNetworkPartConfig
{
    public Type defaultWorker = typeof(NetworkPart);
    public NetworkRole roles = NetworkRole.Transmitter;
    public NetIOConfig? netIOConfig;
    public FlowVolumeConfig<NetworkValueDef> volumeConfig;
}