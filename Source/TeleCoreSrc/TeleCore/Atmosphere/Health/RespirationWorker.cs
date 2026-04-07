using Verse;

namespace TeleCore.Atmosphere.Health;

public abstract class RespirationWorker
{
    public abstract void OnInterval(Pawn pawn, float needLevel, Need_Respiration need = null);
}