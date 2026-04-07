using TeleCore.Defs;
using TeleCore.Network.Flow.Values;

namespace TeleCore.Absorbed.RWLib.Patches.InternalTests;

internal class Profiling
{
    public static NetworkDef[] NetworkDefs = new NetworkDef[1]
    {
        new()
        {
            defName = "TestNetworkDef",
            containerLabel = "ContainerLabel",
            transmitterGraphic = null,
            overlayGraphic = null,
            controllerDef = null,
            transmitterDef = null
        }
    };


    public static NetworkValueDef[] ValueDefs = new NetworkValueDef[2]
    {
        new()
        {
            defName = "Value1",
            label = null,
            description = null,
            descriptionHyperlinks = null,
            ignoreConfigErrors = false,
            ignoreIllegalLabelCharacterConfigError = false,
            modExtensions = null,
            shortHash = 0,
            index = 0,
            modContentPack = null,
            fileName = null,
            generated = false,
            debugRandomId = 0,
            labelShort = null,
            valueUnit = null,
            valueColor = default,
            sharesCapacity = false,
            viscosity = 0,
            capacityFactor = 0,
            collectionDef = null,
            SpecialDroppedContainerDef = null,
            ThingDroppedFromContainer = null,
            ValueToThingRatio = 0
        },
        new()
        {
            defName = "Value2",
            label = null,
            description = null,
            descriptionHyperlinks = null,
            ignoreConfigErrors = false,
            ignoreIllegalLabelCharacterConfigError = false,
            modExtensions = null,
            shortHash = 0,
            index = 0,
            modContentPack = null,
            fileName = null,
            generated = false,
            debugRandomId = 0,
            labelShort = null,
            valueUnit = null,
            valueColor = default,
            sharesCapacity = false,
            viscosity = 0,
            capacityFactor = 0,
            collectionDef = null,
            SpecialDroppedContainerDef = null,
            ThingDroppedFromContainer = null,
            ValueToThingRatio = 0
        }
    };

    /*internal class NetworkClassTest : IContainerImplementer<NetworkValueDef, IContainerHolderNetwork, NetworkContainer>,
        IContainerHolderNetwork
    {
        public NetworkClassTest()
        {
            NetworkPart = null;
        }

        public string ContainerTitle { get; }

        public void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
        {
            //Console.WriteLine($"Funny result args:\n{args}");
        }

        public Thing Thing { get; }

        public bool ShowStorageGizmo { get; }
        public INetworkPart NetworkPart { get; }
        public NetworkContainerSet ContainerSet { get; }
        public NetworkContainer Container { get; }
        public NetworkDef NetworkDef { get; }
    }

    public NetworkContainer TestContainer { get; set; }

    //TODO: REGISTRATION BROKEY
    internal void ContainerInitTest()
    {
        Console.WriteLine("------------------------------------------------------------\n");

        TestContainer = new NetworkContainer(new ContainerConfig<NetworkValueDef>
        {
            containerClass = null,
            baseCapacity = 10000,
            containerLabel = null,
            storeEvenly = false,
            dropContents = false,
            leaveContainer = false,
            valueDefs = new List<NetworkValueDef>(ValueDefs),
            explosionProps = null
        }, new NetworkClassTest());
    }*/
}