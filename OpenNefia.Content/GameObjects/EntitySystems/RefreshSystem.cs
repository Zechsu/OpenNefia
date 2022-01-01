﻿using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public interface IRefreshSystem
    {
        void Refresh(EntityUid entity);
    }

    public class RefreshSystem : EntitySystem, IRefreshSystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MetaDataComponent, MapInitEvent>(OnMapInit, nameof(OnMapInit));
        }

        public void Refresh(EntityUid entity)
        {
            var ev = new EntityRefreshEvent();
            RaiseLocalEvent(entity, ref ev);
        }

        private void OnMapInit(EntityUid uid, MetaDataComponent component, ref MapInitEvent args)
        {
            Refresh(uid);
        }
    }

    public struct EntityRefreshEvent
    {
    }
}