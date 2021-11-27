﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class ItemComponent : Component
    {
        public override string Name => "Item";

        [DataField]
        public int Value { get; }
    }
}
