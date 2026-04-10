using TeleCore.Logging;
using TeleCore.Static;
using Verse;

namespace TeleCore.Mod.Loader.Identification;

public readonly struct DefID<TDef> where TDef : Def
{
    public int ID { get; }

    public TDef Def
    {
        get
        {
            if (ID == -1)
            {
                TLog.Warning($"No Def {typeof(TDef)} with ID -1 found.");
                return null;
            }

            return DefIDStack.ToDef<TDef>(ID);
        }
    }

    public DefID(int id)
    {
        ID = id;
    }

    public static implicit operator DefID<TDef>(TDef def)
    {
        return new DefID<TDef>(def?.ToID() ?? -1);
    }

    public static implicit operator DefID<TDef>(int id)
    {
        return new DefID<TDef>(id);
    }

    public static implicit operator int(DefID<TDef> defID)
    {
        return defID.ID;
    }

    public static implicit operator TDef(DefID<TDef> defID)
    {
        return defID.Def;
    }

    public static TDef ToDef(int id)
    {
        return DefIDStack.ToDef<TDef>(id);
    }

    public static int ToID(TDef def)
    {
        return def.ToID();
    }

    public override string ToString()
    {
        return $"[{typeof(TDef)}:{ID}]";
    }
}