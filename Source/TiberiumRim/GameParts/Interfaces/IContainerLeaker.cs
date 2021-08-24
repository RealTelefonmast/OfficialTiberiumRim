
namespace TiberiumRim
{
    public interface IContainerLeaker
    {
        bool ShouldLeak { get; }
        NetworkContainer Container { get; }
    }
}
