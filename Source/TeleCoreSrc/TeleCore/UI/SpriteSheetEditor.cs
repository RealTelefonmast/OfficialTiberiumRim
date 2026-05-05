using System;
using System.Collections.Generic;
using TeleCore.Rendering.UI.DynaUI;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class SpriteSheetEditor : Rendering.UI.DynaUI.UIElement, IDragAndDropReceiver
{
    //
    private const int _MinGridSize = 8;
    private const int _MinTileSize = 12;
    private const int _MaxTileSize = 48;
    private const int _GridPixels = _MinGridSize * _MaxTileSize; //8*48 = 384
    private static readonly int[] GridSizes = new int[3] { 1, 2, 4 };

    //
    private static readonly Vector2 _ListSize = new(75, 20);

    private int currentScaleIndex = 1;
    private Rect? currentTile;

    //SavableSheet
    private TextureSpriteSheet saveableSheet;
    private int selIndex;

    //
    private SpriteTile? selTile;

    //
    private Vector2 spriteListScrollPos;

    //Texture Data
    private WrappedTexture texture;

    //
    public SpriteSheetEditor(UIElementMode mode) : base(mode)
    {
        Title = "Texture Sheet Editor";
        bgColor = TColor.BGP3;

        //
        Tiles = new List<SpriteTile>();
        UIDragNDropper.RegisterAcceptor(this);
    }

    private bool Shifting { get; set; }


    public Texture Texture => texture.Texture;
    public List<SpriteTile> Tiles { get; }

    private Rect TopPartRect => InRect.TopPartPixels(_GridPixels + 20);
    private Rect BottomRect => InRect.BottomPartPixels(InRect.height - (_GridPixels + 20));

    //Work Area
    private int TileSize => GridSizes[currentScaleIndex] * _MinTileSize;
    private int CanvasDimensions => _GridPixels / TileSize;
    private Rect CanvasRect => TopPartRect.LeftPartPixels(_GridPixels + 20).ContractedBy(10);
    private Rect SettingsRect => TopPartRect.RightPartPixels(TopPartRect.width - (_GridPixels + 20));

    private Rect TileOutPutRect => BottomRect.ContractedBy(10f).Rounded();

    //
    public override string Label => "Sprite Sheet Editor";

    //Drag N Drop
    public void DrawHoveredData(object draggedObject, Vector2 pos)
    {
        if (draggedObject is WrappedTexture texture)
        {
            var label = $"Splice '{texture.Texture.name}'";
            var size = Text.CalcSize(label);
            pos.y -= size.y;

            //
            GUI.color = TColor.White075;
            Widgets.DrawTextureFitted(CanvasRect, texture.Texture, 1);
            GUI.color = Color.white;

            Widgets.Label(new Rect(pos, size), label);
        }
    }

    public bool TryAcceptDrop(object draggedObject, Vector2 pos)
    {
        if (draggedObject is WrappedTexture texture)
        {
            LoadTexture(texture);
            return true;
        }

        return false;
    }

    public bool CanAcceptDrop(object draggedObject)
    {
        return draggedObject is WrappedTexture;
    }

    public static void DrawSpriteSheet(Vector2 topLeft, TextureSpriteSheet sheet)
    {
    }

    //
    private void Clear()
    {
        Tiles.Clear();
        texture.Clear();
        saveableSheet = null;
    }

    public void LoadTexture(WrappedTexture texture)
    {
        Clear();
        this.texture = texture;
    }

    private void CreateTile(Rect tileRect)
    {
        if (tileRect.width == 0 || tileRect.height == 0) return;

        //
        saveableSheet ??= new TextureSpriteSheet(Texture, Tiles);

        var tile = new SpriteTile(CanvasRect, tileRect, Texture);
        Tiles.Add(tile);
        selIndex = Tiles.Count - 1;
        selTile = Tiles[selIndex];
    }

    //
    protected override void HandleEvent_Custom(Event ev, bool inContext)
    {
        //Dont accept input when empty
        if (Texture == null) return;

        if (Input.GetKeyDown(KeyCode.LeftShift)) Shifting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) Shifting = false;

        //
        if (CanvasRect.Contains(StartDragPos) && CanvasRect.Contains(EndDragPos))
        {
            var startDragSnapped = MousePosSnapped(StartDragPos - CanvasRect.position);
            var dragDiffFromSnapped = EndDragPos - CanvasRect.position - startDragSnapped;
            var dragDiffSnapped = MousePosSnapped(dragDiffFromSnapped);

            if (dragDiffFromSnapped.x < 0) startDragSnapped.x += dragDiffSnapped.x;
            if (dragDiffFromSnapped.y < 0) startDragSnapped.y += dragDiffSnapped.y;

            currentTile = new Rect(startDragSnapped, dragDiffSnapped.Abs());
        }

        //
        if (ev.type == EventType.MouseUp)
        {
            if (currentTile.HasValue) CreateTile(currentTile.Value);
            currentTile = null;
        }
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        DrawCanvas(CanvasRect.Rounded());

        DoSettings(SettingsRect.ContractedBy(10, 0).Rounded());

        //
        DrawOutputArea(TileOutPutRect);
    }

    private void DrawMouseOnGrid(Rect rect)
    {
        var mousePos = MousePosSnapped(Event.current.mousePosition);
        //Draw SnapPos
        Widgets.DrawLineHorizontal(mousePos.x - 5, mousePos.y, 10);
        Widgets.DrawLineVertical(mousePos.x, mousePos.y - 5, 10);

        TWidgets.DoTinyLabel(new Rect(rect.x, rect.yMax - 25, 200, 25), mousePos.ToString());
    }

    private Rect RectClipped(Rect rectToClip, Rect clippingRect)
    {
        var newX = Utils.TMath.Min(clippingRect.x, rectToClip.x);
        var newY = TMath.Min(clippingRect.y, rectToClip.y);
        var newXMax = TMath.Min(clippingRect.xMax, rectToClip.xMax);
        var newYMax = TMath.Min(clippingRect.yMax, rectToClip.yMax);

        return new Rect(newX, newY, newXMax - newX, newYMax - newY).Rounded();
    }

    private Vector2 MousePosSnapped(Vector2 mp)
    {
        if (Shifting) return mp;
        //
        return new Vector2(Mathf.RoundToInt(mp.x / TileSize) * TileSize, Mathf.RoundToInt(mp.y / TileSize) * TileSize);
        ;
    }

    private void DrawCanvasGrid(Rect canvasRect, int tileSize, int dimension)
    {
        TWidgets.DrawColoredBox(canvasRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);
        GUI.color = TColor.White025;
        {
            var curX = canvasRect.x;
            var curY = canvasRect.y;
            for (var x = 0; x < dimension; x++)
            {
                Widgets.DrawLineVertical(curX, canvasRect.y, canvasRect.height);
                Widgets.DrawLineHorizontal(canvasRect.x, curY, canvasRect.width);
                curY += tileSize;
                curX += tileSize;
            }
        }
        GUI.color = Color.white;
    }

    //
    private void DrawCanvas(Rect rect)
    {
        DrawCanvasGrid(rect, TileSize, CanvasDimensions);

        if (Texture == null) return;
        Widgets.DrawTextureFitted(rect, Texture, 1);

        //Draw Tiles
        Widgets.BeginGroup(rect);
        {
            for (var i = Tiles.Count - 1; i >= 0; i--)
            {
                var spriteTile = Tiles[i];
                var color = Color.white;

                var sRect = spriteTile.rect.Rounded();
                TWidgets.DrawBox(sRect, color, 1);
                Widgets.Label(sRect.ContractedBy(1), $"[{i}]");

                //
                if (Widgets.CloseButtonFor(sRect))
                {
                    Tiles.RemoveAt(i);
                    if (selTile != null)
                        if (spriteTile == selTile.Value)
                            selTile = null;
                }
            }

            if (currentTile != null)
                TWidgets.DrawBox(currentTile.Value, Color.green, 1);

            //Mouse
            DrawMouseOnGrid(rect);

            if (Widgets.ButtonImage(new Rect(rect.width - 25, rect.height - 25, 25, 25), TeleContent.LockOpen,
                    false))
                Clear();
        }

        if (selTile.HasValue)
            TWidgets.DrawBox(selTile.Value.rect.Rounded(), Color.cyan, 1);

        Widgets.EndGroup();
    }

    private void DoSettings(Rect rect)
    {
        var listing = new Listing_Standard();
        listing.Begin(rect);
        listing.Label("Settings");
        listing.Label($"Tile Scale: {currentScaleIndex + 1}");
        listing.GapLine(4);
        currentScaleIndex = Mathf.RoundToInt(listing.Slider(currentScaleIndex, 0, GridSizes.Length - 1));
        listing.Gap();
        listing.Label("Info:");
        listing.Label($"Free Shape: [{KeyCode.LeftShift}]");

        listing.End();
    }

    //
    private void DrawHovered(SpriteTile spriteTile)
    {
        Widgets.BeginGroup(CanvasRect);
        {
            TWidgets.DrawBox(spriteTile.rect.Rounded(), Color.green, 1);
        }
        Widgets.EndGroup();
    }

    private void DrawTileList(Rect rect)
    {
        TWidgets.DrawColoredBox(rect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);

        Widgets.BeginGroup(rect);
        rect = rect.AtZero();

        var fullRect = new Rect(0, 0, rect.width, Tiles.Count * _ListSize.y);
        Widgets.BeginScrollView(rect, ref spriteListScrollPos, fullRect, false);
        float curY = 0;
        for (var i = 0; i < Tiles.Count; i++)
        {
            var tile = Tiles[i];
            var tileRect = new Rect(0, curY, rect.width, _ListSize.y);
            var color = selIndex == i ? Color.cyan : Color.white;
            TWidgets.DrawBox(tileRect, color, 1);
            Widgets.Label(tileRect, $"[{i}]");
            curY += _ListSize.y;
            if (Widgets.ButtonInvisible(tileRect))
            {
                selTile = tile;
                selIndex = i;
            }
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawOutputArea(Rect rect)
    {
        Widgets.DrawLine(new Vector2(rect.x, rect.y - 10f), new Vector2(rect.xMax, rect.y - 10), TColor.White025, 1);
        Widgets.DrawBoxSolid(TileOutPutRect, TColor.BGDarker);

        var width = rect.width;
        float size = 32;
        var height = (float)TMath.Round(Tiles.Count / 4f, 0, MidpointRounding.AwayFromZero) * size;

        Widgets.BeginGroup(TileOutPutRect);
        var XY = Vector2.zero;
        SpriteTile? hoveredTile = null;
        for (var i = 0; i < Tiles.Count; i++)
        {
            var sFlag = i % 2 != 0;
            var tile = Tiles[i];
            var spriteRect = new Rect(XY, new Vector2(size, size));

            if (sFlag)
                Widgets.DrawHighlight(spriteRect);

            tile.DrawTile(spriteRect);

            if (Mouse.IsOver(spriteRect))
            {
                hoveredTile = tile;
                DragAndDropData = tile;
                TWidgets.DrawBox(spriteRect, TColor.White05, 1);
            }

            if (Widgets.ButtonInvisible(spriteRect))
            {
                selTile = tile;
                selIndex = i;
            }

            if (XY.x + size * 2 > width)
            {
                XY.y += size;
                XY.x = 0;
            }
            else
            {
                XY.x += size;
            }
        }

        Widgets.EndGroup();

        if (hoveredTile.HasValue)
            DrawHovered(hoveredTile.Value);
    }

    protected override IEnumerable<FloatMenuOption> RightClickOptions()
    {
        return base.RightClickOptions();
    }
}