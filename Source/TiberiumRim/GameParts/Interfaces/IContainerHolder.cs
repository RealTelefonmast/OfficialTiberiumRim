
using System;
using Verse;

namespace TiberiumRim
{
    public interface IContainerHolder
    {
        Thing Thing { get; }
        NetworkContainer Container { get; }
        void Notify_ContainerFull();
    }
}
