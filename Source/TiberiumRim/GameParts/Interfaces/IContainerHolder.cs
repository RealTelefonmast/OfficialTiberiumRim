namespace TR.Interfaces;

public interface IContainerHolder
{
    TiberiumContainer Container { get; }
    void Notify_ContainerFull();
}