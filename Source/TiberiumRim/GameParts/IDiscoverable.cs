
namespace TiberiumRim
{
    public interface IDiscoverable
    {
        DiscoveryDef DiscoveryDef { get; }
        bool Discovered { get; }

        string DiscoveredLabel { get; }
        string UnknownLabel { get; }
        string DiscoveredDescription { get; }
        string UnknownDescription { get; }
        string DescriptionExtra { get; }
    }
}
