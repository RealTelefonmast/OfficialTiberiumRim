using TeleCore.PlaySettings;
using Verse;

namespace TeleCore.BuildMenu;

public class BuildMenuInjector : DefInjectBase
{
    public override void OnBuildableDefInject(BuildableDef def)
    {
        //TODO: Handle mod extension caching maybe?
        var extension = def.GetModExtension<BuildMenuExtension>();
        if (extension != null)
            SubMenuThingDefList.Add(def, extension);
        
    }
}