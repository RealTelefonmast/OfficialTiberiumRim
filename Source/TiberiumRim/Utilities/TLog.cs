using Verse;

namespace TiberiumRim
{
    public static class TLog
    {
        public static void Error(string msg)
        {
            Log.Error($"[TiberiumRim] {msg}");
        }

        public static void ErrorOnce(string msg, int id)
        {
            Log.ErrorOnce($"[TiberiumRim] {msg}", id);
        }


        public static void Warning(string msg)
        {
            Log.Warning($"[TiberiumRim] {msg}");
        }

        public static void Message(string msg)
        {
            Log.Message($"[TiberiumRim] {msg}");
        }

        public static void Debug(string msg)
        {
            if (TiberiumRimMod.isDebug)
            {
                Log.Message($"[T-DEBUG] {msg}");
            }
        }
    }
}
