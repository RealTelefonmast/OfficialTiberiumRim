using TeleCore.Defs;
using TeleCore.Types.Abstracts;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Types;

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