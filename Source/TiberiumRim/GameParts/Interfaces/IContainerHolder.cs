namespace TR.GameParts.Interfaces;

public interface IContainerHolder
{
    TiberiumContainer Container { get; }
    void Notify_ContainerFull();
}