﻿using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public interface IUiLayer : IDrawable, IUiDefaultSizeable, ILocalizable
    {
        int ZOrder { get; set; }

        void GetPreferredBounds(out UIBox2i bounds);
        void OnQuery();
        void OnQueryFinish();
        bool IsQuerying();
        bool IsInActiveLayerList();
    }
}
