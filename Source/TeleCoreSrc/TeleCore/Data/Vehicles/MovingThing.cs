using TeleCore.AI.Pathing;
using Verse;

namespace TeleCore.Vehicles;

//A simple thing which can move along a pather
public class MovingThing : ThingWithComps
{
    private Generic_PathFollower _pather;
}