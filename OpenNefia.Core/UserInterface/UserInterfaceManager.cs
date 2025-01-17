﻿using OpenNefia.Core.GameController;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UserInterface
{
    public sealed partial class UserInterfaceManager : IUserInterfaceManagerInternal
    {
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        /// <inheritdoc/>
        public UiElement? KeyboardFocused { get; private set; }

        /// <inheritdoc/>
        public UiElement? ControlFocused { get; set; }

        /// <inheritdoc/>
        public UiElement? CurrentlyHovered { get; private set; }

        /// <inheritdoc/>
        public ScreenCoordinates MousePositionScaled => _inputManager.MouseScreenPosition;

        public void Initialize()
        {
            _inputManager.UIKeyBindStateChanged += OnUIKeyBindStateChanged;
            _graphics.OnWindowResized += HandleWindowResized;
        }

        public void InitializeTesting()
        {
        }

        public void Shutdown()
        {
        }

        /// <inheritdoc/>
        public Vector2? CalcRelativeMousePositionFor(UiElement control, ScreenCoordinates mousePos)
        {
            return mousePos.Position - control.GlobalPixelPosition;
        }

        public void GrabKeyboardFocus(UiElement control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (!control.CanKeyboardFocus)
            {
                throw new ArgumentException("Control cannot get keyboard focus.", nameof(control));
            }

            if (control == KeyboardFocused)
            {
                return;
            }

            ReleaseKeyboardFocus();

            KeyboardFocused = control;

            KeyboardFocused.KeyboardFocusEntered();
        }

        public void ReleaseKeyboardFocus()
        {
            var oldFocused = KeyboardFocused;
            oldFocused?.KeyboardFocusExited();
            KeyboardFocused = null;
        }

        public void ReleaseKeyboardFocus(UiElement ifControl)
        {
            if (ifControl == null)
            {
                throw new ArgumentNullException(nameof(ifControl));
            }

            if (ifControl == KeyboardFocused)
            {
                ReleaseKeyboardFocus();
            }
        }

        public void ControlRemovedFromTree(UiElement control)
        {
            ReleaseKeyboardFocus(control);
            if (control == CurrentlyHovered)
            {
                control.MouseExited();
                CurrentlyHovered = null;
            }

            if (control != ControlFocused) return;
            ControlFocused = null;
        }

        public UiElement? MouseGetControl(ScreenCoordinates coordinates)
        {
            return MouseGetControlAndRel(coordinates)?.control;
        }

        private (UiElement control, Vector2 rel)? MouseGetControlAndRel(ScreenCoordinates coordinates)
        {
            if (CurrentLayer == null)
                return null;

            return MouseFindControlAtPos(CurrentLayer, coordinates.Position);
        }

        private static (UiElement control, Vector2 rel)? MouseFindControlAtPos(UiElement control, Vector2 position)
        {
            for (var i = control.ChildCount - 1; i >= 0; i--)
            {
                var child = control.GetChild(i);
                if (!child.Visible || !child.GlobalPixelBounds.Contains((Vector2i)position))
                {
                    continue;
                }

                var maybeFoundOnChild = MouseFindControlAtPos(child, position);
                if (maybeFoundOnChild != null)
                {
                    return maybeFoundOnChild;
                }
            }

            if (control.EventFilter != UIEventFilterMode.Ignore && control.ContainsPoint(position / control.UIScale))
            {
                return (control, position);
            }

            return null;
        }

        private bool OnUIKeyBindStateChanged(BoundKeyEventArgs args)
        {
            if (args.State == BoundKeyState.Down)
            {
                KeyBindDown(args);
            }
            else
            {
                KeyBindUp(args);
            }

            // If we are in a focused control or doing a CanFocus, return true
            // So that InputManager doesn't propagate events to simulation.
            if (!args.CanFocus && KeyboardFocused != null)
            {
                return true;
            }

            return false;
        }
    }
}