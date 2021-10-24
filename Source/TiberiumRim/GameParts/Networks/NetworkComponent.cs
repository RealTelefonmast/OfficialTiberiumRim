using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NetworkComponentProperties
    {
        //Cached Data
        private NetworkRole? networkRole;

        //Loaded from XML
        public bool storeEvenly = false;

        public NetworkDef networkDef;
        public ContainerProperties containerProps;
        public List<NetworkRole> networkRoles = new() { NetworkRole.Transmitter };
        public List<NetworkValueDef> allowedValues;

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

    public class NetworkComponent : IExposable, INetworkComponent, IContainerHolder
    {
        protected Comp_NetworkStructure parent;
        protected NetworkContainer container;
        protected Network network;
        protected NetworkComponentSet componentSet;

        protected NetworkComponentProperties props;

        protected List<INetworkComponent> currentReceivers = new List<INetworkComponent>();

        //DEBUG
        protected bool DebugNetworkCells = false;

        public NetworkComponentProperties Props => props;
        public Thing Thing => parent.Thing;

        //Network Data
        public bool IsMainController => Network.NetworkController == Parent;
        public bool IsPowered => parent.IsPowered;
        public bool HasLeak => false;
        public bool HasConnection => ConnectedComponentSet.Transmitters.Any();
        public bool HasContainer => Props.containerProps != null;
        public bool IsReceiving { get; set; }
        public NetworkRole NetworkRole => Props.NetworkRole;

        public INetworkStructure Parent => parent;
        public NetworkDef NetworkDef => Props.networkDef;
        public Network Network { get; set; }
        public NetworkComponentSet ConnectedComponentSet => componentSet;

        public string ContainerTitle => NetworkDef.containerLabel;

        public NetworkContainer Container
        {
            get => container;
            private set => container = value;
        }

        public NetworkComponent(Comp_NetworkStructure parent, NetworkComponentProperties properties)
        {
            this.parent = parent;
            this.props = properties;
        }

        public virtual void ExposeData()
        {
            Scribe_Deep.Look(ref container, "container");
        }

        public void ComponentSetup(bool respawningAfterLoad)
        {
            //Generate components
            componentSet = new NetworkComponentSet(NetworkDef, this);
            if (HasContainer)
                Container = new NetworkContainer(this, Props.containerProps, Props.allowedValues);

        }

        public void PostDestroy(DestroyMode mode, Map previousMap)
        {
            ConnectedComponentSet.ParentDestroyed();
            Container?.Parent_Destroyed(mode, previousMap);
            Network.RemoveComponent(this);
        }

        public virtual void NetworkCompTick()
        {
            if (!Network.IsWorking || !IsPowered || IsMainController)
            {
                StopBinding();
                return;
            }
            ProcessValues();
        }

        //Data Notifiers
        public void Notify_ContainerFull()
        {

        }

        public void Notify_NewComponentAdded(INetworkComponent component)
        {
            ConnectedComponentSet.AddNewComponent(component);
        }

        public void Notify_NewComponentRemoved(INetworkComponent component)
        {
            ConnectedComponentSet.RemoveComponent(component);
        }

        public void StartBinding(INetworkComponent toOther)
        {
            currentReceivers.Add(toOther);
            toOther.IsReceiving = true;
        }

        public void StopBinding(INetworkComponent toOther)
        {
            currentReceivers.Remove(toOther);
            toOther.IsReceiving = false;
        }

        public void StopBinding()
        {
            currentReceivers.ForEach(r => r.IsReceiving = false);
        }

        //Network 
        //Process current stored values according to rules of the network role
        private void ProcessValues()
        {
            //Producers push to Storages
            if (NetworkRole.HasFlag(NetworkRole.Producer))
            {
                ProducerTick();
            }

            //Storages push to Consumers
            if (NetworkRole.HasFlag(NetworkRole.Storage))
            {
                StorageTick();
            }

            //Consumers slowly use up own container
            if (NetworkRole.HasFlag(NetworkRole.Consumer))
            {
                ConsumerTick();
            }
        }

        protected virtual void ProducerTick()
        {
            TransferToOthers(NetworkRole.Storage, false);
        }

        protected virtual void StorageTick()
        {
            if (Props.storeEvenly)
            {
                TransferToOthers(NetworkRole.Storage, true);
            }
            TransferToOthers(NetworkRole.Consumer, false);
        }

        private void TransferToOthers(NetworkRole ofRole, bool evenly)
        {
            StopBinding();
            if (!Container.HasValueStored) return;
            foreach (var component in Network.ComponentSet[ofRole])
            {
                if (!Container.HasValueStored || component.Container.CapacityFull) continue;
                if (evenly && component.Container.StoredPercent > Container.StoredPercent) continue;

                //Log.Message($"Transferring from {parent} to structures of role {ofRole} with {Container.AllStoredTypes.Count} stored types.");
                for (int i = Container.AllStoredTypes.Count - 1; i >= 0; i--)
                {
                    var type = Container.AllStoredTypes.ElementAt(i);
                    if (!component.NeedsValue(type)) continue;
                    if (Container.TryTransferTo(component.Container, type, 1))
                    {
                        StartBinding(component);
                    }
                }
            }
        }

        protected virtual void ConsumerTick()
        {
        }

        public void SendFirstValue(INetworkComponent other)
        {
            Container.TryTransferTo(other.Container, Container.AllStoredTypes.FirstOrDefault(), 1);
        }

        public bool ConnectsTo(INetworkComponent other)
        {
            if (other == this) return false;
            return NetworkDef == other.NetworkDef && CompatibleWith(other) && parent.ConnectsTo(other.Parent);
        }

        private bool CompatibleWith(INetworkComponent other)
        {
            if (other.Network == null)
            {
                Log.Error($"{other.Parent.Thing} is not part of any Network - this should not be the case.");
                return false;
            }
            return other.Network.NetworkRank == Network.NetworkRank;
        }

        public bool NeedsValue(NetworkValueDef value)
        {
            //Feel free to extend as needed
            return parent.AcceptsValue(value); // && Whatever..
        }

        public void Draw()
        {
            if (DebugNetworkCells)
            {
                GenDraw.DrawFieldEdges(Network.NetworkCells, Color.cyan);
            }
        }

        public virtual string NetworkInspectString()
        {
            return string.Empty;
        }

        public virtual Gizmo SpecialNetworkDescription
        {
            get => null;
        }

        protected virtual IEnumerable<Gizmo> GetSpecialNetworkGizmos()
        {
            if (!IsMainController && Network.NetworkController == null)
            {
                yield return new Designator_BuildFixed(NetworkDef.controllerDef);
            }
        }

        public virtual IEnumerable<Gizmo> GetPartGizmos()
        {
            if (HasContainer)
            {
                foreach (var containerGizmo in Container.GetGizmos())
                {
                    yield return containerGizmo;
                }
            }

            if (IsMainController)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Show Entire Network",
                    action = delegate
                    {
                        DebugNetworkCells = !DebugNetworkCells;
                    }
                };
            }

            yield return new Designator_BuildFixed(parent.Thing.def);
            yield return new Designator_BuildFixed(NetworkDef.transmitter);

            foreach (var networkGizmo in GetSpecialNetworkGizmos())
            {
                yield return networkGizmo;
            }

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = $"View {NetworkDef.defName} Set",
                    defaultDesc = ConnectedComponentSet.ToString(),
                    action = delegate { }
                };

                yield return new Command_Action
                {
                    defaultLabel = $"View Entire {NetworkDef.defName} Set",
                    defaultDesc = Network.ComponentSet.ToString(),
                    action = delegate { }
                };
            }
        }
    }
}
