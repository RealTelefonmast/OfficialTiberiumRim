using Verse;

namespace TiberiumRim
{
    public static class TLog
    {
        public static void Error(string msg)
        {
            Log.Error($"[TiberiumRim] {msg}", true);
        }

        public static void Warning(string msg)
        {
            Log.Warning($"[TiberiumRim] {msg}");
        }
    }
}
