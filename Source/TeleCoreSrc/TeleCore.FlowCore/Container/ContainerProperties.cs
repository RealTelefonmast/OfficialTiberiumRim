using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore.Static;
using Verse;

namespace TeleCore
{
    public class ContainerProperties : Editable, IExposable
    {
        public Type containerClass = typeof(NetworkContainer);

        //Direct Container Values
        public int maxStorage = 0;

        //Container Processing
        public bool storeEvenly = false;

        //Override
        public string containerLabel;

        //Events
        //public ThingDef droppedContainer = TeleDefOf.PortableContainer;
        public bool dropContents = false;
        public bool leaveContainer = false;

        public ExplosionProperties explosionProps;

        public ContainerProperties Copy()
        {
            return new ContainerProperties
            {
                containerClass = this.containerClass,
                maxStorage = maxStorage,
                storeEvenly = storeEvenly,
                dropContents = dropContents,
                leaveContainer = leaveContainer,
                explosionProps = explosionProps,
            };
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref maxStorage, "maxStorage");
            Scribe_Values.Look(ref dropContents, "dropContents");
            Scribe_Values.Look(ref leaveContainer, "leaveContainer");
            Scribe_Deep.Look(ref explosionProps, "explosionProperties");
        }
    }
}
