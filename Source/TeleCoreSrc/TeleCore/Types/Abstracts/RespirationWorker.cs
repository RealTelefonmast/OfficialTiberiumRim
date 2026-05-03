using TeleCore.Needs;
using Verse;

namespace TeleCore.Unsorted;

public abstract class RespirationWorker
{
    public abstract void OnInterval(Pawn pawn, float needLevel, Need_Respiration need = null);
}