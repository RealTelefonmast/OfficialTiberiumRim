using System.Linq;
using Verse;

namespace TeleCore.Unsorted;

public abstract class RoomLinkWorker
{
    protected RoomLink link;

    public RoomLinkWorker(RoomLink link)
    {
        this.link = link;
    }
}

public class RoomLink
{
    private readonly Room[] rooms = new Room[2];
    private EdgeSpan span;

    public Room RoomA
    {
        get => rooms[0];
        set => rooms[0] = value;
    }

    public Room RoomB
    {
        get => rooms[1];
        set => rooms[1] = value;
    }

    public EdgeSpan Span => span;

    public void Register(Room room)
    {
        if (rooms[0] == room || rooms[1] == room)
        {
            Log.Error(string.Concat("Tried to double-register region ", room.ToString(), " in ", this));
            return;
        }

        if (RoomA == null || RoomA.Dereferenced)
        {
            RoomA = room;
            return;
        }

        if (RoomB == null || RoomB.Dereferenced)
        {
            RoomB = room;
            return;
        }

        Log.Error(string.Concat("Could not register region ", room.ToString(), " in link ", this,
            ": > 2 regions on link!\nRegionA: ", RoomA.DebugString(), "\nRegionB: ", RoomB.DebugString()));
    }

    public void Deregister(Room room)
    {
        if (RoomA == room)
        {
            RoomA = null;
            if (RoomB == null)
            {
                //room.Map.regionLinkDatabase.Notify_LinkHasNoRegions(this);
            }
        }
        else if (RoomB == room)
        {
            RoomB = null;
            if (RoomA == null)
            {
                //room.Map.regionLinkDatabase.Notify_LinkHasNoRegions(this);
            }
        }
    }

    public Room OppositeRoom(Room reg)
    {
        if (reg != RoomA) return RoomA;

        return RoomB;
    }

    public ulong UniqueHashCode()
    {
        return span.UniqueHashCode();
    }

    public override string ToString()
    {
        var text = (from r in rooms
            where r != null
            select r.ID.ToString()).ToCommaList();
        var text2 = string.Concat("span=", span.ToString(), " hash=", UniqueHashCode());
        return string.Concat("(", text2, ", regions=", text, ")");
    }
}