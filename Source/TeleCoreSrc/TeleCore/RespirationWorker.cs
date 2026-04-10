using TeleCore.Atmosphere.Health;
using Verse;

namespace TeleCore;

public abstract class RespirationWorker
{
    public abstract void OnInterval(Pawn pawn, float needLevel, Need_Respiration need = null);
}