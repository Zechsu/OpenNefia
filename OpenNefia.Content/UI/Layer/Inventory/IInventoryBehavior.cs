﻿using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.UI.Layer.Inventory
{
    /// <summary>
    /// Defines the filtering/selection logic for a single inventory screen.
    /// </summary>
    public interface IInventoryBehavior : IHspIds<InvElonaId>
    {
        /// <summary>
        /// Whether to allow entries in this behavior to be bindable to shortcuts.
        /// </summary>
        bool EnableShortcuts { get; }

        /// <summary>
        /// Icon this behavior will display in the icon bar.
        /// </summary>
        PrototypeId<AssetPrototype>? Icon { get; }

        /// <summary>
        /// Whether the player should be queried for an item count after selecting an item.
        /// </summary>
        bool QueryAmount { get; }

        /// <summary>
        /// Whether to show the total amount of gold in the inventory screen.
        /// </summary>
        bool ShowMoney { get; }

        /// <summary>
        /// Whether to show the target's equipment in the inventory screen.
        /// </summary>
        bool ShowTargetEquip { get; }

        /// <summary>
        /// Window title to display.
        /// </summary>
        string WindowTitle { get; }

        /// <summary>
        /// The default amount to use when querying for an item amount.
        /// </summary>
        int DefaultAmount { get; }

        /// <summary>
        /// Whether to include items with a <see cref="OwnState.Special"/> own state in the filtered list.
        /// </summary>
        bool AllowSpecialOwned { get; }

        IEnumerable<IInventorySource> GetSources(InventoryContext context);

        string GetQueryText(InventoryContext context);

        bool IsAccepted(InventoryContext context, EntityUid item);

        InventoryResult OnSelect(InventoryContext context, EntityUid item, int amount);

        InventoryResult AfterFilter(InventoryContext context, IReadOnlyList<EntityUid> filteredItems);
    }

    public struct InvElonaId
    {
        /// <summary>
        /// ID of the inventory screen.
        /// </summary>
        public int Id;

        /// <summary>
        /// Sub ID of the inventory screen. For example, the "get"
        /// action has sub IDs for taking items from the ground
        /// and taking items from containers.
        /// </summary>
        public int SubId;

        public InvElonaId(int id, int subId = 0)
        {
            Id = id;
            SubId = subId;
        }
    }
}