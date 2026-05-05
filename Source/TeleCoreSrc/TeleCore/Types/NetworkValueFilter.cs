using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Types.Enums;
using Verse;

namespace TeleCore.Types;

public class NetworkValueFilter : FlowValueFilter<NetworkValueDef>
{
    public List<NetworkValueFilterByRole> allowedValuesByRole;

    [Unsaved] private Dictionary<NetworkRole, List<NetworkValueDef>>? allowedValuesByRoleInt;

    public List<NetworkValueDef> this[NetworkRole role] => allowedValuesByRoleInt[role];


    /// <summary>
    ///     Provides sub-managed values by role, if set in the networkRole props.
    /// </summary>
    public Dictionary<NetworkRole, List<NetworkValueDef>> AllowedValuesByRole
    {
        get
        {
            if (allowedValuesByRoleInt != null) return allowedValuesByRoleInt;

            allowedValuesByRoleInt = new Dictionary<NetworkRole, List<NetworkValueDef>>();
            foreach (var filter in allowedValuesByRole)
                if (filter.HasSubValues && filter != NetworkRole.Transmitter)
                    allowedValuesByRoleInt.Add(filter, filter.subValues);
            return allowedValuesByRoleInt;
        }
    }


    public void PostLoad()
    {
    }
}