using System;
using TeleCore.Types.Exposables;
using Verse;

namespace TeleCore.UI;

public class ContainerProperties : Editable, IExposable
{
    public Type containerClass = typeof(NetworkContainer);

    //Override
    public string containerLabel;

    //Events
    //public ThingDef droppedContainer = TeleDefOf.PortableContainer;
    public bool dropContents;

    public ExplosionProperties explosionProps;
    public bool leaveContainer;

    //Direct Container Values
    public int maxStorage;

    //Container Processing
    public bool storeEvenly;

    public void ExposeData()
    {
        Scribe_Values.Look(ref maxStorage, "maxStorage");
        Scribe_Values.Look(ref dropContents, "dropContents");
        Scribe_Values.Look(ref leaveContainer, "leaveContainer");
        Scribe_Deep.Look(ref explosionProps, "explosionProperties");
    }

    public ContainerProperties Copy()
    {
        return new ContainerProperties
        {
            containerClass = containerClass,
            maxStorage = maxStorage,
            storeEvenly = storeEvenly,
            dropContents = dropContents,
            leaveContainer = leaveContainer,
            explosionProps = explosionProps
        };
    }
}