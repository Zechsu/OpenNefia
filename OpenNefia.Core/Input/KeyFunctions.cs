﻿using System;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Core.Input
{
    public enum BoundKeyState : byte
    {
        Up = 0,
        Down = 1
    }

    [KeyFunctions]
    public static class EngineKeyFunctions
    {
        public static readonly BoundKeyFunction North = "North";
        public static readonly BoundKeyFunction South = "South";
        public static readonly BoundKeyFunction West = "West";
        public static readonly BoundKeyFunction East = "East";
        public static readonly BoundKeyFunction Southeast = "Southeast";
        public static readonly BoundKeyFunction Northeast = "Northeast";
        public static readonly BoundKeyFunction Northwest = "Northwest";
        public static readonly BoundKeyFunction Southwest = "Southwest";

        public static readonly BoundKeyFunction UIClick = "UIClick";
        public static readonly BoundKeyFunction UIRightClick = "UIRightClick";

        public static readonly BoundKeyFunction UIUp = "UIUp";
        public static readonly BoundKeyFunction UIDown = "UIDown";
        public static readonly BoundKeyFunction UILeft = "UILeft";
        public static readonly BoundKeyFunction UIRight = "UIRight";
        public static readonly BoundKeyFunction UISelect = "UISelect";
        public static readonly BoundKeyFunction UICancel = "UICancel";

        public static readonly BoundKeyFunction ShowDebugConsole = "ShowDebugConsole";
        public static readonly BoundKeyFunction ShowDebugMonitors = "ShowDebugMonitors";
        public static readonly BoundKeyFunction ShowEscapeMenu = "ShowEscapeMenu";

        // Cursor keys in LineEdit and such.
        public static readonly BoundKeyFunction TextCursorLeft = "TextCursorLeft";
        public static readonly BoundKeyFunction TextCursorRight = "TextCursorRight";
        public static readonly BoundKeyFunction TextCursorWordLeft = "TextCursorWordLeft";
        public static readonly BoundKeyFunction TextCursorWordRight = "TextCursorWordRight";
        public static readonly BoundKeyFunction TextCursorBegin = "TextCursorBegin";
        public static readonly BoundKeyFunction TextCursorEnd = "TextCursorEnd";

        // Cursor keys for also selecting text.
        public static readonly BoundKeyFunction TextCursorSelect = "TextCursorSelect";
        public static readonly BoundKeyFunction TextCursorSelectLeft = "TextCursorSelectLeft";
        public static readonly BoundKeyFunction TextCursorSelectRight = "TextCursorSelectRight";
        public static readonly BoundKeyFunction TextCursorSelectWordLeft = "TextCursorSelectWordLeft";
        public static readonly BoundKeyFunction TextCursorSelectWordRight = "TextCursorSelectWordRight";
        public static readonly BoundKeyFunction TextCursorSelectBegin = "TextCursorSelectBegin";
        public static readonly BoundKeyFunction TextCursorSelectEnd = "TextCursorSelectEnd";

        public static readonly BoundKeyFunction TextBackspace = "TextBackspace";
        public static readonly BoundKeyFunction TextSubmit = "TextSubmit";
        public static readonly BoundKeyFunction TextSelectAll = "TextSelectAll";
        public static readonly BoundKeyFunction TextCopy = "TextCopy";
        public static readonly BoundKeyFunction TextCut = "TextCut";
        public static readonly BoundKeyFunction TextPaste = "TextPaste";
        public static readonly BoundKeyFunction TextHistoryPrev = "TextHistoryPrev";
        public static readonly BoundKeyFunction TextHistoryNext = "TextHistoryNext";
        public static readonly BoundKeyFunction TextReleaseFocus = "TextReleaseFocus";
        public static readonly BoundKeyFunction TextScrollToBottom = "TextScrollToBottom";
        public static readonly BoundKeyFunction TextDelete = "TextDelete";
        public static readonly BoundKeyFunction TextTabComplete = "TextTabComplete";
    }

    [Serializable]
    public struct BoundKeyFunction : IComparable, IComparable<BoundKeyFunction>, IEquatable<BoundKeyFunction>, ISelfSerialize
    {
        public readonly string FunctionName;

        public BoundKeyFunction(string name)
        {
            FunctionName = name;
        }

        public static implicit operator BoundKeyFunction(string name)
        {
            return new(name);
        }

        public override readonly string ToString()
        {
            return $"KeyFunction({FunctionName})";
        }

        #region Code for easy equality and sorting.

        public readonly int CompareTo(object? obj)
        {
            if (!(obj is BoundKeyFunction func))
            {
                return 1;
            }
            return CompareTo(func);
        }

        public readonly int CompareTo(BoundKeyFunction other)
        {
            return string.Compare(FunctionName, other.FunctionName, StringComparison.InvariantCultureIgnoreCase);
        }

        // Could maybe go dirty and optimize these on the assumption that they're singletons.
        public override readonly bool Equals(object? obj)
        {
            return obj is BoundKeyFunction func && Equals(func);
        }

        public readonly bool Equals(BoundKeyFunction other)
        {
            return other.FunctionName == FunctionName;
        }

        public override readonly int GetHashCode()
        {
            return FunctionName.GetHashCode();
        }

        public static bool operator ==(BoundKeyFunction a, BoundKeyFunction b)
        {
            return a.FunctionName == b.FunctionName;
        }

        public static bool operator !=(BoundKeyFunction a, BoundKeyFunction b)
        {
            return !(a == b);
        }

        #endregion

        public void Deserialize(string value)
        {
            this = new BoundKeyFunction(value);
        }

        public readonly string Serialize()
        {
            return FunctionName;
        }
    }

    /// <summary>
    ///     Makes all constant strings on this static class be added as input functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KeyFunctionsAttribute : Attribute { }
}
