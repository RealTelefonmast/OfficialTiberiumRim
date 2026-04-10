using TeleCore.GameData.AI.Pathing;
using Verse;

namespace TeleCore.RWExtended.ThingClasses;

//A simple thing which can move along a pather
public class MovingThing : ThingWithComps
{
    private Generic_PathFollower _pather;
}