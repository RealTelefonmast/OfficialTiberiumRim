namespace TR.Interfaces;

public interface IContainerLeaker
{
    bool ShouldLeak { get; }
    TiberiumContainer Container { get; }
}