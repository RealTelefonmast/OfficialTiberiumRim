
using System;
using Verse;

namespace TiberiumRim
{
    public interface IContainerHolder
    {
        string ContainerTitle { get; }
        Thing Thing { get; }
        NetworkContainer Container { get; }

        void Notify_ContainerFull();
        void Notify_ContainerStateChanged();
    }
}
