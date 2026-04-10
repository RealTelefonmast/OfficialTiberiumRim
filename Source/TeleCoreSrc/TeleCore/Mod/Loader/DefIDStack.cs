using System.Collections.Generic;
using Verse;

namespace TeleCore.Loader;

public static class DefIDStack
{
    internal static int _MasterID;
    internal static Dictionary<Def, int> _defToID;
    internal static Dictionary<int, Def> _idToDef;

    static DefIDStack()
    {
        _defToID = new Dictionary<Def, int>();
        _idToDef = new Dictionary<int, Def>();
    }

    public static int ToID<TDef>(this TDef def) where TDef : Def
    {
        if (_defToID.TryGetValue(def, out var id)) return id;
        TLog.Warning($"Cannot find id for ({def.GetType()}){def}. Make sure to call base.PostLoad().");
        return def.index;
    }

    internal static TDef ToDef<TDef>(int id) where TDef : Def
    {
        if (_idToDef.TryGetValue(id, out var def))
        {
            if (def is TDef casted)
                return casted;
            TLog.Warning($"Cannot cast {def} to {typeof(TDef)}");
            return null;
        }

        TLog.Warning($"Cannot find Def for {id} of type {typeof(TDef)}. Make sure PostLoad calls base.PostLoad.");
        return null;
    }

    internal static void RegisterNew<TDef>(TDef def) where TDef : Def
    {
        if (_defToID.ContainsKey(def))
        {
            //var trygetid = _defToID.TryGetValue(def);
            //var trygetdef = _idToDef.TryGetValue(trygetid);
            //TLog.Warning($"({def.GetType().FullName}){def} is already registered: {trygetdef?.defName}::{trygetid}.");
            return;
        }

        _defToID.Add(def, _MasterID);
        _idToDef.Add(_MasterID, def);
        _MasterID++;
    }
}