using TeleCore.GameData.Defs;

namespace TeleCore.Interfaces;

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