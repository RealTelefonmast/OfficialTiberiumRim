using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class MuzzleFlashProperties : Editable
{
    public FleckDef? fleck;
    public float scale = 1;
    public Vector3 offset = Vector3.zero;
}