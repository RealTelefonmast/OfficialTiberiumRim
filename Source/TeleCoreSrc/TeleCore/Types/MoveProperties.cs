using UnityEngine;

namespace TeleCore.Types;

public class MoveProperties
{
    public int endOffset = 5;
    public int moveSpeed = 1;
    public int startOffset = 0;

    public float MoverSpeed => Mathf.Lerp(0, endOffset - startOffset, moveSpeed);
}