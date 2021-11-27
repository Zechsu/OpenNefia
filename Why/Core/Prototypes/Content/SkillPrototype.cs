﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    [Prototype("Skill")]
    public class SkillPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;
    }
}
