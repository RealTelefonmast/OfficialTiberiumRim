using System;
using TeleCore.Events.Args;
using TeleCore.Loader;

namespace TeleCore.Events;

public static partial class GlobalEventHandler
{
    public static class Pawns
    {
        public static event PawnHediffChangedEvent? HediffChanged;
        
        //TODO:
        //public static event PawnEnteredRoomEvent PawnEnteredRoom;
        //public static event PawnLeftRoomEvent PawnLeftRoom;

        internal static void OnPawnHediffChanged(PawnHediffChangedEventArgs args)
        {
            try
            {
                HediffChanged?.Invoke(args);
            }
            catch (Exception ex)
            {
                TLog.Error($"Error trying to register hediff change on pawn: {args.Pawn}\n{ex.Message}");
            }
        }

        internal static void Clear()
        {
            HediffChanged = null;
        }
    }
}