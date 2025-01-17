﻿using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using System.Collections.Generic;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Generates tile boundries for calculating FOV.
    /// </summary>
    public static class FovRadius
    {
        private static Dictionary<int, int[,]> Cache = new();

        /// <summary>
        /// Gets a set of tile boundaries representing a circle with the given radius.
        /// </summary>
        /// <param name="fovMax">Radius of the circle.</param>
        /// <returns>An Nx2 array with the start and end positions of the radius.</returns>
        public static int[,] Get(int fovMax)
        {
            if (Cache.TryGetValue(fovMax, out var fovList))
                return fovList;

            var radius = (fovMax + 2) / 2;
            var radiusVector = new Vector2i(radius, radius);
            double maxDist = (double)fovMax / 2;

            var fovMap = new bool[fovMax+2, fovMax+2];

            for (int y = 0; y < fovMax+2; y++)
            {
                for (int x = 0; x < fovMax+2; x++)
                {
                    fovMap[y, x] = (new Vector2i(x, y) - radiusVector).LengthSquared < maxDist * maxDist;
                }
            }

            fovList = new int[fovMax + 2, 2];

            for (int y = 0; y < fovMax + 2; y++)
            {
                var found = false;
                for (int x = 0; x < fovMax + 2; x++)
                {
                    if (fovMap[y, x])
                    {
                        if (!found)
                        {
                            fovList[y, 0] = x;
                            found = true;
                        }
                    }
                    else if (found)
                    {
                        fovList[y, 1] = x;
                        break;
                    }
                }
            }

            Cache.Add(fovMax, fovList);
            
            return fovList;
        }
    }
}