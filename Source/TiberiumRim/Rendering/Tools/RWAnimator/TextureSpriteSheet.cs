using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TextureSpriteSheet
    {
        private string name;

        private Texture texture;
        private List<SpriteTile> tiles;

        public Texture Texture => texture;
        public List<SpriteTile> Tiles => tiles;

        public TextureSpriteSheet(Texture texture, List<SpriteTile> tiles)
        {
            this.texture = texture;
            this.tiles = tiles;
        }

        public void DrawData(Rect rect)
        {

        }
    }
}
