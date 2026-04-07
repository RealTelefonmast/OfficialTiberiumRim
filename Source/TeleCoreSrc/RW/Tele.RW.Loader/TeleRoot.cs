using UnityEngine;

namespace TeleCore.Loader;

/// <summary>
///     Experimental Updating of custom core related parts
/// </summary>
public sealed class TeleRoot : MonoBehaviour
{

    public void Start()
    {
    }

    public void Update()
    {
    }

    private void OnApplicationQuit()
    {
        ApplicationQuitUtility.Notify_ApplicationQuit();
    }
}