using System;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class TRLog
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

        public static void Message(string msg, Color color)
        {
            if (Log.ReachedMaxMessagesLimit)
                Log.ResetMessageCount();

            UnityEngine.Debug.Log(msg);
            Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Message, $"{"[TR]".Colorize(color)} {msg}", StackTraceUtility.ExtractStackTrace()));
            Log.PostMessage();
        }

        public static void Message(string msg)
        {
            if(Log.ReachedMaxMessagesLimit)
                Log.ResetMessageCount();

            UnityEngine.Debug.Log(msg);
            Log.messageQueue.Enqueue(new LogMessage(LogMessageType.Message, $"{"[TR]".Colorize(TRColor.Green)} {msg}", StackTraceUtility.ExtractStackTrace()));
            Log.PostMessage();
        }

        public static void Debug(string msg, bool flag = true)
        {
            if (TiberiumRimMod.isDebug && flag)
            {
                Log.Message($"{"[TR-Debug]".Colorize(TRColor.Green)} {msg}");
            }
        }
    }
}
