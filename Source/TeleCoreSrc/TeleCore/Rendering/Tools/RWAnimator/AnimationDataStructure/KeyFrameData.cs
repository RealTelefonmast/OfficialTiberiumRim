using System.Text;
using TeleCore.Scribing;
using TeleCore.Utils;
using TeleCore.Utils.Serialization;
using UnityEngine;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public struct KeyFrameData : IExposable
{
    //TRS
    private float rotation;
    private Vector2 position, size, pivotPoint;
    private Rect texCoordsData;

    public Vector2 TSize
    {
        get => size;
        set => size = value;
    }

    public Vector2 TPosition
    {
        get => position;
        set => position = value;
    }

    public float TRotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Vector2 PivotPoint
    {
        get => pivotPoint;
        set => pivotPoint = value;
    }

    public Rect TexCoords
    {
        get => texCoordsData;
        set => texCoordsData = value;
    }

    public void UpdateBuffer(string[] buffer)
    {
        var tSize = TSize;
        var tPos = TPosition;
        var tRot = TRotation;
        var tPivot = PivotPoint;
        var texCoords = TexCoords;

        //Size
        buffer[0] = tSize.x.ToString();
        buffer[1] = tSize.y.ToString();
        //
        buffer[2] = tPos.x.ToString();
        buffer[3] = tPos.y.ToString();
        //
        buffer[4] = tRot.ToString();
        //
        buffer[5] = tPivot.x.ToString();
        buffer[6] = tPivot.y.ToString();
        //
        buffer[7] = texCoords.x.ToString();
        buffer[8] = texCoords.y.ToString();
        buffer[9] = texCoords.width.ToString();
        buffer[10] = texCoords.height.ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine($"Size: {size}");
        sb.AppendLine($"Pos: {position}");
        sb.AppendLine($"Rot: {rotation}");
        sb.AppendLine($"Pivot: {pivotPoint}");
        sb.AppendLine($"TexCoords: {texCoordsData}");

        return sb.ToString();
    }

    public void ExposeData()
    {
        /*
        if (AnimationFileSaveLoader._OLDLOAD)
        {
            Scribe_Values.Look(ref rotation, "rotation", forceSave: true);
            Scribe_Values.Look(ref position, "position", forceSave: true);
            Scribe_Rect.Look(ref texCoordsData, "texCoords", new Rect(0, 0, 1, 1));
            Scribe_Values.Look(ref size, "size", forceSave: true);
            Scribe_Values.Look(ref pivotPoint, "pivotPoint", forceSave: true);
            return;
        }
        */

        //
        Scribe_Values.Look(ref rotation, nameof(rotation), forceSave: true);
        Scribe_Values.Look(ref position, nameof(position), forceSave: true);
        Scribe_Rect.Look(ref texCoordsData, nameof(texCoordsData), new Rect(0, 0, 1, 1));
        Scribe_Values.Look(ref size, nameof(size), forceSave: true);
        Scribe_Values.Look(ref pivotPoint, nameof(pivotPoint), forceSave: true);
    }

    public KeyFrameData(Vector2 pos, float rot, Vector2 size)
    {
        position = pos;
        rotation = rot;
        this.size = size;
        pivotPoint = Vector2.zero;
        texCoordsData = default;
        TexCoords = new Rect(0, 0, 1, 1);
    }

    public KeyFrameData(Vector2 pos, float rot, Vector2 size, Vector2 pivot, Rect texCoords)
    {
        position = pos;
        rotation = rot;
        this.size = size;
        pivotPoint = pivot;
        texCoordsData = default;
        TexCoords = texCoords;
    }

    public KeyFrameData Interpolated(KeyFrameData other, float val)
    {
        return new KeyFrameData(
            Vector2.Lerp(position, other.position, val),
            Mathf.Lerp(rotation, other.rotation, val),
            Vector2.Lerp(size, other.size, val), Vector2.Lerp(pivotPoint, other.pivotPoint, val),
            TexCoords.Lerp(other.TexCoords, val));
    }

    //
    public static bool operator ==(KeyFrameData dataL, KeyFrameData dataR)
    {
        if (dataL.rotation != dataR.rotation) return false;
        if (dataL.position != dataR.position) return false;
        if (dataL.size != dataR.size) return false;
        if (dataL.pivotPoint != dataR.pivotPoint) return false;
        if (dataL.texCoordsData != dataR.texCoordsData) return false;
        return true;
    }

    public static bool operator !=(KeyFrameData dataL, KeyFrameData dataR)
    {
        return !(dataL == dataR);
    }

    public bool Equals(KeyFrameData other)
    {
        return rotation.Equals(other.rotation) && position.Equals(other.position) && size.Equals(other.size) &&
               pivotPoint.Equals(other.pivotPoint) && texCoordsData.Equals(other.texCoordsData);
    }

    public override bool Equals(object obj)
    {
        return obj is KeyFrameData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = rotation.GetHashCode();
            hashCode = (hashCode * 397) ^ position.GetHashCode();
            hashCode = (hashCode * 397) ^ size.GetHashCode();
            hashCode = (hashCode * 397) ^ pivotPoint.GetHashCode();
            hashCode = (hashCode * 397) ^ texCoordsData.GetHashCode();
            return hashCode;
        }
    }
}