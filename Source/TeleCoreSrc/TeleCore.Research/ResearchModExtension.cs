using TeleCore.Types;
using Verse;

namespace TeleCore.Research;

public class ResearchModExtension : DefModExtension
{
    public Requisites requisites;

    public bool IsFinished => requisites.FulFilled();
}