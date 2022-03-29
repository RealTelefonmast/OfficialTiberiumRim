
using System;
using Verse;

namespace TiberiumRim
{
    public interface IContainerHolderStructure : IContainerHolder
    {
        INetworkComponent NetworkComp { get; }
        NetworkContainerSet ContainerSet { get; }
    }

    public interface IContainerHolder
    {
        string ContainerTitle { get; }
        ContainerProperties ContainerProps { get; }
        NetworkContainer Container { get; }
        Thing Thing { get; }

        void Notify_ContainerFull();
        void Notify_ContainerStateChanged();
    }
}
