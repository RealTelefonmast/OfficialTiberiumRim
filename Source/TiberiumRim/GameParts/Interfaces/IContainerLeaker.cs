
namespace TiberiumRim
{
    public interface IContainerLeaker
    {
        bool ShouldLeak { get; }
        TiberiumContainer Container { get; }
    }
}
