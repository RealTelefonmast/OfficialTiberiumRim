using Verse;

namespace TeleCore.Unsorted;

public class PatchConfig : IExposable
{
    private string _modHash;
    private string _oldHash;


    public void ExposeData()
    {
        Scribe_Values.Look(ref _oldHash, "oldHash");
        Scribe_Values.Look(ref _modHash, "modHash");
    }
}