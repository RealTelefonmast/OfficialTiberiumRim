using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class ResearchModExtension : DefModExtension
{
    public Requisites requisites;

    public bool IsFinished => requisites.FulFilled();
}