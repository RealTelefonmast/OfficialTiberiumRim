using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SpriteSheetEditor : UIElement, IDragAndDropReceiver
    {
        private WrappedTexture texture;
        private List<SpriteTile> tiles;

        private TextureSpriteSheet finalSheet;

        //
        private SpriteTile? selTile;
        private int selIndex;

        //
        private Vector2 spriteListScrollPos;
        private Rect? currentTile;


        public Texture Texture => texture.Texture;
        public List<SpriteTile> Tiles => tiles;

        //
        private static readonly Vector2 _ListSize = new Vector2(75, 20);
        private Rect BaseRect => Rect.BottomPartPixels(Rect.height-TopRect.height);
        private Rect TopPartRect => BaseRect.TopPart(.85f);
        private Rect BottomRect => BaseRect.BottomPart(.15f);
        //Work Area
        private Rect CanvasRect => TopPartRect.LeftPartPixels(TopPartRect.height).ContractedBy(10f);
        private Rect TileDataRect => TopPartRect.RightPartPixels(TopPartRect.width - TopPartRect.height).ContractedBy(10f);
        private Rect TileListRect => TileDataRect.LeftPartPixels(_ListSize.x);
        private Rect TileInfoRect => TileDataRect.RightPartPixels(TileDataRect.height);

        //Settings
        private Rect SettingsRect => BottomRect.ContractedBy(10f);
        private Rect ExportInteractRect => SettingsRect.RightPartPixels((SettingsRect.height * 1.5f) + 5);
        private Rect TileReadoutRect => SettingsRect.LeftPartPixels(SettingsRect.width - (ExportInteractRect.width + 5));
        private Rect ExportRect => ExportInteractRect.LeftPartPixels(SettingsRect.height);
        private Rect ViewRectButton => ExportInteractRect.RightPartPixels(SettingsRect.height * 0.5f);

        //Viewer
        //private bool ViewerIsActive { get; set; }
        //private Rect ViewerRect => new Rect(BaseRect.x, BaseRect.yMax, BaseRect.width, 250);

        //
        public override string Label => "Sprite Sheet Editor";

        public static void DrawSpriteSheet(Vector2 topLeft, TextureSpriteSheet sheet)
        {
        }

        //
        public SpriteSheetEditor(UIElementMode mode) : base(mode)
        {
            Title = "Texture Sheet Editor";
            Size = new Vector2(800, 425);
            tiles = new List<SpriteTile>();

            UIDragNDropper.RegisterAcceptor(this);
        }

        //
        public void LoadTexture(WrappedTexture texture)
        {
            //TODO: Notify replacemenet
            this.texture = texture;
        }

        private void CreateTile(Rect tileRect)
        {
            finalSheet ??= new TextureSpriteSheet(Texture, tiles);

            var tile = new SpriteTile(CanvasRect, tileRect, Texture);
            tiles.Add(tile);
            selIndex = tiles.Count - 1;
            selTile = tiles[selIndex];
        }

        private void Clear()
        {
            tiles.Clear();
            texture.Clear();
            finalSheet = null;
        }

        private static int GridDimensions = 10;

        //
        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {
            var inEditor = CanvasRect.Contains(ev.mousePosition);



            if (finalSheet != null && ExportRect.Contains(ev.mousePosition))
            {
                if(ev.type == EventType.MouseDown)
                    UIDragNDropper.Notify_DraggingData(this, finalSheet, ev);
            }

            if (inEditor && CanvasRect.Contains(StartDragPos))
            {
                if (Tiles.Any(t => t.rect.Contains(ev.mousePosition)))
                {
                    
                }
                else
                {
                    var tileSize = CanvasRect.width/ GridDimensions;
                    var tp = (StartDragPos - CanvasRect.position);
                    var cdd = CurrentDragDiff;
                    var startDragSnapped = new Vector2(Mathf.RoundToInt(tp.x / tileSize) * tileSize, Mathf.RoundToInt(tp.y / tileSize) * tileSize);
                    var dragDiffSnapped = new Vector2(Mathf.RoundToInt(cdd.x / tileSize) * tileSize, Mathf.RoundToInt(cdd.y / tileSize) * tileSize);

                    if (CurrentDragDiff.x < 0)
                        startDragSnapped.x += dragDiffSnapped.x;

                    if (CurrentDragDiff.y < 0)
                        startDragSnapped.y += dragDiffSnapped.y;

                    currentTile = new Rect(startDragSnapped, dragDiffSnapped.Abs());
                }

                if (ev.type == EventType.MouseUp)
                {
                    if (inEditor && currentTile != null)
                        CreateTile(currentTile.Value);
                    currentTile = null;
                }
            }
        }

        protected override void DrawContents(Rect inRect)
        {
            DrawCanvas(CanvasRect.Rounded());

            //

            if (Texture == null) return;
            DrawTileList(TileListRect);
            DrawSettings(SettingsRect);

            if (selTile == null) return;
            DrawTileInfo(TileInfoRect, selTile.Value);
        }

        private void DrawMouseOnGrid(Rect rect, int gridTileSize)
        {
            Widgets.BeginGroup(rect);
            {
                var mp = Event.current.mousePosition;
                var mousePos = new Vector2(Mathf.RoundToInt(mp.x / gridTileSize) * gridTileSize, Mathf.RoundToInt(mp.y / gridTileSize) * gridTileSize);
                //Draw SnapPos
                Widgets.DrawLineHorizontal(mousePos.x - 5, mousePos.y, 10);
                Widgets.DrawLineVertical(mousePos.x, mousePos.y - 5, 10);
            }
            Widgets.EndGroup();
        }

        private void DrawCanvasGrid(Rect canvasRect, int tileSize, int dimension)
        {
            TRWidgets.DrawColoredBox(canvasRect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);
            GUI.color = TRColor.White025;
            {
                var curX = canvasRect.x;
                var curY = canvasRect.y;
                for (int x = 0; x < dimension; x++)
                {
                    Widgets.DrawLineVertical(curX, canvasRect.y, canvasRect.height);
                    Widgets.DrawLineHorizontal(canvasRect.x, curY, canvasRect.width);
                    curY += tileSize;
                    curX += tileSize;
                }
            }
            GUI.color = Color.white;
            DrawMouseOnGrid(canvasRect, tileSize);
        }

        private void DrawCanvas(Rect rect)
        {
            DrawCanvasGrid(rect, Mathf.RoundToInt(rect.width / GridDimensions), GridDimensions);

            if (Texture == null) return;
            Widgets.DrawTextureFitted(rect, Texture, 1);

            //Draw Tiles
            Widgets.BeginGroup(rect);
            for (var i = 0; i < Tiles.Count; i++)
            {
                var spriteTile = Tiles[i];
                var color = selIndex == i ? Color.cyan : Color.red;
                TRWidgets.DrawBox(spriteTile.rect, color, 1);

                Widgets.Label(spriteTile.rect.ContractedBy(1), $"[{i}]");
            }

            if (currentTile != null)
                TRWidgets.DrawBox(currentTile.Value, Color.green, 1);

            if (Widgets.ButtonImage(new Rect(rect.width - 25, rect.height - 25, 25, 25), TiberiumContent.LockOpen, false))
            {
                Clear();
            }

            Widgets.EndGroup();
        }

        private void DrawTileList(Rect rect)
        {
            TRWidgets.DrawColoredBox(rect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);

            Widgets.BeginGroup(rect);
            rect = rect.AtZero();

            Rect fullRect = new Rect(0, 0, rect.width, Tiles.Count * _ListSize.y);
            Widgets.BeginScrollView(rect, ref spriteListScrollPos, fullRect, false);
            float curY = 0;
            for (var i = 0; i < Tiles.Count; i++)
            {
                var tile = Tiles[i];
                Rect tileRect = new Rect(0, curY, rect.width, _ListSize.y);
                var color = selIndex == i ? Color.cyan : Color.white;
                TRWidgets.DrawBox(tileRect, color, 1);
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

        private void DrawTileInfo(Rect rect, SpriteTile tile)
        {
            TRWidgets.DrawColoredBox(rect, TRMats.BGLighter, TRMats.MenuSectionBGBorderColor, 1);

            Widgets.BeginGroup(rect);
            rect = rect.AtZero();
            var center = rect.center;
            var rect2 = center.RectOnPos(tile.rect);

            tile.DrawTile(rect2);
            Widgets.DrawBox(rect2, 1);

            Widgets.EndGroup();
        }

        private void DrawSettings(Rect rect)
        {
            var sheetExists = finalSheet != null;
            Widgets.DrawLine(new(rect.x, rect.y - 10f), new(rect.xMax, rect.y - 10), TRColor.White025, 1);

            //Readout
            Widgets.DrawBoxSolid(TileReadoutRect, TRMats.BGDarker);

            float width = rect.width;
            float size = 24;

            float height = ((float)(Math.Round((Tiles.Count / 4f), 0, MidpointRounding.AwayFromZero)) * size);

            Widgets.BeginGroup(TileReadoutRect);
            Vector2 XY = Vector2.zero;
            for (var i = 0; i < tiles.Count; i++)
            {
                var sFlag = i % 2 != 0;
                var tile = tiles[i];
                Rect spriteRect = new Rect(XY, new Vector2(size, size));

                if(sFlag)
                    Widgets.DrawHighlight(spriteRect);

                tile.DrawTile(spriteRect);

                if (Mouse.IsOver(spriteRect))
                {
                    DragAndDropData = tile;
                    TRWidgets.DrawBox(spriteRect, TRColor.White05, 1);
                }

                if (XY.x + (size * 2) > width)
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

            if (sheetExists)
            {
                finalSheet.DrawData(rect);
                //Draw DragSource
                TRWidgets.DrawColoredBox(ExportRect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(ExportRect, "Take");
                Text.Anchor = default;
            }
        }

        protected override IEnumerable<FloatMenuOption> RightClickOptions()
        {
            return base.RightClickOptions();
        }

        //Drag N Drop
        public void DrawHoveredData(object draggedObject, Vector2 pos)
        {
            if (draggedObject is WrappedTexture texture)
            {
                var label = $"Splice '{texture.Texture.name}'";
                var size = Text.CalcSize(label);
                pos.y -= size.y;

                //
                GUI.color = TRColor.White075;
                Widgets.DrawTextureFitted(CanvasRect, texture.Texture, 1);
                GUI.color = Color.white;

                Widgets.Label(new Rect(pos, size), label);
            }
        }

        public bool TryAccept(object draggedObject, Vector2 pos)
        {
            if (draggedObject is WrappedTexture texture)
            {
                LoadTexture(texture);
                return true;
            }
            return false;
        }

        public bool Accepts(object draggedObject)
        {
            return draggedObject is WrappedTexture;
        }
    }
}
