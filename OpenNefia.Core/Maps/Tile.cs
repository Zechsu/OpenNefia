﻿using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Maps
{
    public struct Tile
    {
        /// <summary>
        /// Internal index mapping to a TilePrototype.
        /// </summary>
        public readonly int Type;

        public Tile(int type)
        {
            Type = type;
        }

        /// <summary>
        ///     An empty tile that can be compared against.
        /// </summary>
        public static readonly Tile Empty = new(0);

        public TilePrototype ResolvePrototype() => IoCManager.Resolve<ITileDefinitionManager>()[Type];

        public override bool Equals(object? obj)
        {
            return obj is Tile tile && Type == tile.Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(Tile a, Tile b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(Tile a, Tile b)
        {
            return !a.Equals(b);
        }
    }
}