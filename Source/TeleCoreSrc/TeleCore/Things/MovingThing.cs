using TeleCore.Types;
using Verse;

namespace TeleCore.Things;

//A simple thing which can move along a pather
public class MovingThing : ThingWithComps
{
    private Generic_PathFollower _pather;
}