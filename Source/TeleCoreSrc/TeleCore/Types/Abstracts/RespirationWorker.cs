using TeleCore.Needs;
using Verse;

namespace TeleCore.Types.Abstracts;

public abstract class RespirationWorker
{
    public abstract void OnInterval(Pawn pawn, float needLevel, Need_Respiration need = null);
}