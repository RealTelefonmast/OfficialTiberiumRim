using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.Types.Enums;
using TeleCore.Types.Exposables;
using TeleCore.UI;
using Verse;

namespace TeleCore.Types;

public class NetworkComponentProperties
{
    private Dictionary<NetworkRole, List<NetworkValueDef>> allowedValuesByRoleInt;
    private List<NetworkValueDef> allowedValuesInt;
    public ContainerProperties containerProps;
    private List<NetworkValueDef> handledValues;
    public NetworkDef networkDef;

    //
    public NetworkDef networkDefForValues;

    //Cached Data
    private NetworkRole? networkRole;

    //TODO NetworkRoleProperties
    public List<NetworkRoleProperties> networkRoles = new() { this.NetworkRole.Transmitter };

    //Loaded from XML
    public Type workerType = typeof(NetworkComponent);

    public Dictionary<NetworkRole, List<NetworkValueDef>> AllowedValuesByRole
    {
        get
        {
            if (allowedValuesByRoleInt == null)
            {
                allowedValuesByRoleInt = new Dictionary<NetworkRole, List<NetworkValueDef>>();
                foreach (var role in networkRoles)
                {
                    if (role.HasSubValues && role != this.NetworkRole.Transmitter)
                    {
                        allowedValuesByRoleInt.Add(role, role.subValues);
                        continue;
                    }

                    allowedValuesByRoleInt.Add(role, AllowedValues);
                }
            }

            return allowedValuesByRoleInt;
        }
    }

    public List<NetworkValueDef> AllowedValues
    {
        get
        {
            if (allowedValuesInt == null)
            {
                var list = new List<NetworkValueDef>();
                if (networkDefForValues != null) list.AddRange(networkDefForValues.NetworkValueDefs);
                if (!handledValues.NullOrEmpty()) list.AddRange(handledValues);
                allowedValuesInt = list.Distinct().ToList();
            }

            return allowedValuesInt;
        }
    }

    public NetworkRole NetworkRole
    {
        get
        {
            if (networkRole == null)
            {
                networkRole = this.NetworkRole.Transmitter;
                foreach (var role in networkRoles) networkRole |= role;
            }

            return networkRole.Value;
        }
    }
}