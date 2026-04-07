using Verse;

namespace TeleCore.Loader.Configs;

public class PatchConfig : IExposable
{
    private string _oldHash;
    private string _modHash;


    public void ExposeData()
    {
        Scribe_Values.Look(ref _oldHash, "oldHash");
        Scribe_Values.Look(ref _modHash, "modHash");
    }
}