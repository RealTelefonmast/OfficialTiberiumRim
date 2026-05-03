using System.Collections.Generic;
using UnityEngine;

namespace TeleCore.Unsorted;

public class ExtendedGraphicData
{
    public bool alignToBottom = false;
    public Vector3 drawOffset = Vector3.zero;
    public bool? drawRotatedOverride = null;
    public List<string> linkStrings;
    public bool rotateDrawSize = true;
}