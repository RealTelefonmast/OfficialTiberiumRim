using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public class NetworkComponentProperties
    {
        //Cached Data
        private NetworkRole? networkRole;
        private List<NetworkValueDef> allowedValuesInt;
        private Dictionary<NetworkRole, List<NetworkValueDef>> allowedValuesByRoleInt;

        //Loaded from XML
        public Type workerType = typeof(NetworkComponent);
        public NetworkDef networkDef;
        public ContainerProperties containerProps;

        //TODO NetworkRoleProperties
        public List<NetworkRoleProperties> networkRoles = new(){ NetworkRole.Transmitter };

        //
        public NetworkDef networkDefForValues;
        private List<NetworkValueDef> handledValues;

        public Dictionary<NetworkRole, List<NetworkValueDef>> AllowedValuesByRole
        {
            get
            {
                if (allowedValuesByRoleInt == null)
                {
                    allowedValuesByRoleInt = new();
                    foreach (var role in networkRoles)
                    {
                        if (role.HasSubValues && role != NetworkRole.Transmitter)
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
                    if (networkDefForValues != null)
                    {
                        list.AddRange(networkDefForValues.NetworkValueDefs);
                    }
                    if (!handledValues.NullOrEmpty())
                    {
                        list.AddRange(handledValues);
                    }
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
                    networkRole = NetworkRole.Transmitter;
                    foreach (var role in networkRoles)
                    {
                        networkRole |= role;
                    }
                }
                return networkRole.Value;
            }
        }
    }
}
