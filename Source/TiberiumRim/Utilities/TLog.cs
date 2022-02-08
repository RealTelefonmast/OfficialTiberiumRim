using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TLog
    {
        public static void Error(string msg)
        {
            Log.Error($"{"[TR]".Colorize(TRColor.Green)} {msg}");
        }

        public static void ErrorOnce(string msg, int id)
        {
            Log.ErrorOnce($"{"[TR]".Colorize(TRColor.Green)} {msg}", id);
        }


        public static void Warning(string msg)
        {
            Log.Warning($"{"[TR]".Colorize(TRColor.Green)} {msg}");
        }

        public static void Message(string msg)
        {
            Log.Message($"{"[TR]".Colorize(TRColor.Green)} {msg}");
        }

        public static void Debug(string msg)
        {
            if (TiberiumRimMod.isDebug)
            {
                Log.Message($"{"[TR-Debug]".Colorize(TRColor.Green)} {msg}");
            }
        }
    }
}
