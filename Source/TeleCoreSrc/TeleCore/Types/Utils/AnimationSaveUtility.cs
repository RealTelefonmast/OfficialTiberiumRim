using System;
using System.IO;
using TeleCore.Mod;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.FileIO;

//TODO: Generalize into generic saveUtility
internal static class AnimationSaveUtility
{
    private const string NewFileSuffix = ".anim";
    private const string NewDefSuffix = ".xml";
    public const string _SavingNode = "animation";

    public static string SavedWorkingFilesFolderPath => GenFilePaths.FolderUnderSaveData("Animations");
    public static string SavedAnimationDefsFolderPath => TeleCoreMod.Settings.SaveAnimationDefLocation;


    public static string PathForAnimationFile(string animationName)
    {
        return Path.Combine(SavedWorkingFilesFolderPath, animationName + NewFileSuffix);
    }


    /// <summary>
    ///     Saves the custom .anim file for working on the animation.
    /// </summary>
    public static void SaveWorkFile(string fileName, string documentElementName, Action saveAction)
    {
        var path = Path.GetFullPath($"{Path.Combine(SavedWorkingFilesFolderPath, fileName)}{NewFileSuffix}");
        try
        {
            if (!File.Exists(path))
                FileSaveUtility.ProcessSavingAction(path, documentElementName, saveAction);
            else
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation($"Override {fileName}?", delegate
                {
                    FileSaveUtility.RemoveFileIfExists(path, false);
                    FileSaveUtility.ProcessSavingAction(path, documentElementName, saveAction);
                }, true, null, WindowLayer.Super));
        }
        catch (Exception ex4)
        {
            GenUI.ErrorDialog("ProblemSavingFile".Translate(path, ex4.ToString()));
            throw;
        }
    }

    /// <summary>
    ///     Creates a new AnimationDef file.
    /// </summary>
    public static void CreateAnimationDef(string fileName, string documentElementName, Action saveAction)
    {
        var path = Path.GetFullPath($"{Path.Combine(SavedAnimationDefsFolderPath, fileName)}{NewDefSuffix}");
        if (!File.Exists(path))
            FileSaveUtility.ProcessSavingAction(path, documentElementName, saveAction);
        else
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation($"Override {fileName}?", delegate
            {
                FileSaveUtility.RemoveFileIfExists(path, false);
                FileSaveUtility.ProcessSavingAction(path, documentElementName, saveAction);
            }, true, null, WindowLayer.Super));
    }
}