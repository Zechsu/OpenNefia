﻿using Love;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    [Serializable]
    public class AnimFrame
    {
        public string TileId = string.Empty;
        public float Duration = 0f;
    }

    [Serializable]
    public class AtlasTile
    {
        public Love.Quad Quad;
        public int YOffset = 0;
        public bool HasOverhang = false;

        public AtlasTile() 
        {
            Quad = Love.Graphics.NewQuad(0, 0, 0, 0, 0, 0);
        }

        public AtlasTile(Quad quad, int yOffset = 0, bool hasOverhang = false)
        {
            Quad = quad;
            YOffset = yOffset;
            HasOverhang = hasOverhang;
        }
    }

    [Serializable]
    public class TileAtlas : IDisposable
    {
        private Dictionary<string, AtlasTile> _tiles = new Dictionary<string, AtlasTile>();
        private Dictionary<string, List<AnimFrame>> _anims = new Dictionary<string, List<AnimFrame>>();

        public Love.Image Image { get; private set; }

        public TileAtlas(Image image, Dictionary<string, AtlasTile> atlasTiles)
        {
            this.Image = image;
            this._tiles = atlasTiles;
        }

        public bool TryGetTile(string tileId, [NotNullWhen(true)] out AtlasTile? tile)
        {
            return _tiles.TryGetValue(tileId, out tile);
        }

        public bool TryGetTile(TileSpecifier spec, [NotNullWhen(true)] out AtlasTile? tile)
            => TryGetTile(spec.AtlasIndex, out tile);

        public bool GetTileSize(TileSpecifier spec, out int width, out int height)
        {
            if (!TryGetTile(spec, out var tile))
            {
                width = 0;
                height = 0;
                return false;
            }

            var rect = tile.Quad.GetViewport();

            width = (int)rect.Width;
            height = (int)rect.Height;

            return true;
        }

        public void Dispose()
        {
            Image.Dispose();

            foreach (var tile in _tiles.Values)
            {
                tile.Quad.Dispose();
            }
        }
    }
}
