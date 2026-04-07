
namespace TeleCore.Utility.Serialization;

public static class Scribe_Container
{
    internal static bool InvalidState = true;

    // public static void Look<TContainer, TValue>(ref TContainer target, ContainerConfig<TValue> config,
    //     IContainerHolderBase<TValue> holder, string label)
    //     where TContainer : ValueContainerBase<TValue>
    //     where TValue : FlowValueDef
    // {
    //     InvalidState = false;
    //     Scribe_Deep.Look(ref target, label, config, holder);
    //     InvalidState = true;
    // }
}