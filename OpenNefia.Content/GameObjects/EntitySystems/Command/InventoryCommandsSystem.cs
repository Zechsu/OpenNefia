﻿using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// System for commands that open the inventory.
    /// </summary>
    public class InventoryCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.PickUp, InputCmdHandler.FromDelegate(HandlePickUp))
                .Bind(ContentKeyFunctions.Examine,
                    new InventoryInputCmdHandler(_uiMgr, new ExamineInventoryBehavior()))
                .Bind(ContentKeyFunctions.Drop,
                    new InventoryInputCmdHandler(_uiMgr, new DropInventoryBehavior()))
                .Bind(ContentKeyFunctions.Drink,
                    new InventoryInputCmdHandler(_uiMgr, new DrinkInventoryBehavior()))
                .Bind(ContentKeyFunctions.Throw,
                    new InventoryInputCmdHandler(_uiMgr, new ThrowInventoryBehavior()))
                .Register<InventoryCommandsSystem>();
        }

        private TurnResult? HandlePickUp(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            var player = session.Player.Uid;

            if (!EntityManager.TryGetComponent(player, out SpatialComponent? spatial))
                return null;

            var verb = new Verb(PickableSystem.VerbIDPickUp);
            var ents = _lookup.EntitiesUnderneath(session.Player.Uid).ToList();

            if (ents.Count == 0)
            {
                Mes.Display(Loc.GetString("Elona.GameObjects.Pickable.GraspAtAir"));
                return TurnResult.Failed;
            }
            else if (ents.Count == 1)
            {
                var ent = ents.First();
                var result = _verbSystem.ExecuteVerb(player, ent.Uid, verb);

                if (result == TurnResult.NoResult)
                {
                    // Try map-specific actions, like gathering snow.
                    var mapEntity = _mapManager.GetMap(spatial.MapID);
                    result = _verbSystem.ExecuteVerb(player, mapEntity.MapEntityUid, verb);
                }

                if (result == TurnResult.NoResult)
                {
                    Mes.Display(Loc.GetString("Elona.GameObjects.Pickable.GraspAtAir"));
                    result = TurnResult.Failed;
                }

                return result;
            }
            else
            {
                var context = new InventoryContext(player, new PickUpInventoryBehavior());
                var result = _uiMgr.Query<InventoryLayer, InventoryContext, InventoryResult>(context);
                
                if (result.HasValue && result.Value is InventoryResult.Finished finished)
                {
                    return finished.TurnResult;
                }

                return TurnResult.Aborted;
            }
        }

        private sealed class InventoryInputCmdHandler : InputCmdHandler
        {
            private readonly IUserInterfaceManager _uiMgr;
            private readonly IInventoryBehavior _behavior;

            public InventoryInputCmdHandler(IUserInterfaceManager uiMgr, IInventoryBehavior behavior)
            {
                _behavior = behavior;
                _uiMgr = uiMgr;
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage full)
                {
                    return null;
                }

                if (full.State == BoundKeyState.Down && session?.Player != null)
                {
                    var context = new InventoryContext(session.Player.Uid, _behavior);
                    var result = _uiMgr.Query<InventoryLayer, InventoryContext, InventoryResult>(context);

                    if (result.HasValue)
                    {
                        switch (result.Value)
                        {
                            case InventoryResult.Finished finished:
                                return finished.TurnResult;
                            default:
                                return TurnResult.Aborted;
                        }
                    }
                    else
                    {
                        return TurnResult.Aborted;
                    }
                }
                return null;
            }
        }
    }
}
