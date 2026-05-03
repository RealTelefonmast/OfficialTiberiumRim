using System.Collections.Generic;
using System.IO;
using Verse;

namespace TeleCore.Unsorted;

public class AnimationConfig : IExposable
{
    internal static string DefaultAnimationDefLocation =
        Path.Combine(GenFilePaths.FolderUnderSaveData("Animations"), "Defs");

    private readonly TeleCoreSettings parent;

    //Tools.Animation
    public string userDefinedAnimationDefLocation;
    public List<Dictionary<string, bool>> valueList;

    public AnimationConfig(TeleCoreSettings settings)
    {
        parent = settings;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref userDefinedAnimationDefLocation, "userDefinedAnimationDefLocation");

        //TODO: Move module specific settings to their own classes and inject
        if (userDefinedAnimationDefLocation == null)
            SetAnimationDefLocation(DefaultAnimationDefLocation, false);
    }

    internal void SetAnimationDefLocation(string newPath, bool write = true)
    {
        userDefinedAnimationDefLocation = newPath;
        if (write)
            parent.Write();
    }

    internal void ResetAnimationDefLocation()
    {
        SetAnimationDefLocation(DefaultAnimationDefLocation);
    }
}