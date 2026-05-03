using Verse;

namespace TeleCore.Unsorted;

public class ResearchModExtension : DefModExtension
{
    public Requisites requisites;

    public bool IsFinished => requisites.FulFilled();
}