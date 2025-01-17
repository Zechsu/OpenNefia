﻿using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Allows you to enter a field map by using the activate action
    /// if the map entity has this component.
    /// </summary>
    [RegisterComponent]
    public class WorldMapFieldsComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "WorldMapFields";
    }
}
