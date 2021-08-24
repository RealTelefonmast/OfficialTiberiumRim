using Verse;

namespace TiberiumRim
{
    public static class TLog
    {
        public static void Error(string msg)
        {
            Log.Error($"[TiberiumRim] {msg}");
        }

        public static void Warning(string msg)
        {
            Log.Warning($"[TiberiumRim] {msg}");
        }

        public static void Message(string msg)
        {
            Log.Message(msg);
        }
    }
}
