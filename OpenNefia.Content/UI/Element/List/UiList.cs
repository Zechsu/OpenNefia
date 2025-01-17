﻿using Love;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System.Collections;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Input;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiList<T> : UiElement, IUiList<T>, IRawInputControl
    {
        public const int DEFAULT_ITEM_HEIGHT = 19;

        protected IList<UiListCell<T>> Cells { get; }
        public int ItemHeight { get; }
        public int ItemOffsetX { get; }

        public bool HighlightSelected { get; set; }
        public bool SelectOnActivate { get; set; }

        private int _SelectedIndex;
        private bool _needsUpdate;

        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                _SelectedIndex = Math.Clamp(value, 0, Cells.Count);
            }
        }

        public IUiListCell<T>? SelectedCell
        {
            get
            {
                if (Cells.Count == 0)
                    return null;

                return Cells[SelectedIndex];
            }
        }

        protected readonly Dictionary<int, UiListChoiceKey> ChoiceKeys = new();

        public event UiListEventHandler<T>? EventOnSelect;
        public event UiListEventHandler<T>? EventOnActivate;

        public UiList(IEnumerable<UiListCell<T>>? cells = null, int itemOffsetX = 0)
        {
            if (cells == null)
                cells = new List<UiListCell<T>>();

            ItemHeight = DEFAULT_ITEM_HEIGHT;
            ItemOffsetX = 0;
            HighlightSelected = true;
            SelectOnActivate = true;

            Cells = cells.ToList();

            RefreshCellPositionsAndKeys();

            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
            CanKeyboardFocus = true;
        }

        public void RefreshCellPositionsAndKeys()
        {
            RemoveAllChildren();

            ChoiceKeys.Clear();
            for (var i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];
                cell.IndexInList = i;
                if (cell.Key == null)
                {
                    cell.Key = UiListChoiceKey.MakeDefault(i);
                }
                ChoiceKeys[i] = cell.Key;
                AddChild(cell);
            }

            // Set the size/position of the child list cells.
            SetSize(Width, Height);
            SetPosition(X, Y);
        }

        public UiList(IEnumerable<T> items, int itemOffsetX = 0)
            : this(MakeDefaultList(items), itemOffsetX)
        {
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            Logger.Info($"BIND {args.Function} {args.State}");
            if (args.Function == EngineKeyFunctions.UISelect)
            {
                Activate(SelectedIndex);
            }
            else if (args.Function == EngineKeyFunctions.UIClick)
            {
                if (UserInterfaceManager.CurrentlyHovered == SelectedCell)
                {
                    Activate(SelectedIndex);
                }
            }
            else if (args.Function == EngineKeyFunctions.UIUp)
            {
                Sounds.Play(Protos.Sound.Cursor1);
                IncrementIndex(-1);
            }
            else if (args.Function == EngineKeyFunctions.UIDown)
            {
                Sounds.Play(Protos.Sound.Cursor1);
                IncrementIndex(1);
            }
        }

        public bool RawKeyEvent(in GuiRawKeyEvent guiRawEvent)
        {
            if (guiRawEvent.Action != RawKeyAction.Down)
                return false;

            for (int index = 0; index < ChoiceKeys.Count; index++)
            {
                var choiceKey = ChoiceKeys[index];
                if (choiceKey.Key == guiRawEvent.Key)
                {
                    Activate(index);
                    return true;
                }
            }

            return false;
        }

        public override void Localize(LocaleKey key)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];
                cell.Localize(key.With(cell.LocalizeKey ?? i.ToString()));
            }
        }

        private static IEnumerable<UiListCell<TItem>> MakeDefaultList<TItem>(IEnumerable<TItem> items)
        {
            UiListCell<TItem> MakeListCell(TItem item, int index)
            {
                if (item is IUiListItem)
                {
                    var listItem = (IUiListItem)item;
                    return new UiListCell<TItem>(item, listItem.GetChoiceText(index), listItem.GetChoiceKey(index));
                }
                else
                {
                    return new UiListCell<TItem>(item, $"{item}", UiListChoiceKey.MakeDefault(index));
                }
            }
            return items.Select(MakeListCell);
        }

        #region Data Creation

        public override List<UiKeyHint> MakeKeyHints()
        {
            return new List<UiKeyHint>();
        }

        #endregion

        #region List Handling

        protected virtual void OnSelect(UiListEventArgs<T> e)
        {
            EventOnSelect?.Invoke(this, e);
        }

        protected virtual void OnActivate(UiListEventArgs<T> e)
        {
            EventOnActivate?.Invoke(this, e);
        }

        public virtual bool CanSelect(int index)
        {
            return index >= 0 && index < Cells.Count;
        }

        public void IncrementIndex(int delta)
        {
            if (Count == 0)
                return;

            var newIndex = SelectedIndex + delta;
            var sign = Math.Sign(delta);

            while (!CanSelect(newIndex) && newIndex != SelectedIndex)
            {
                newIndex += sign;
                if (newIndex < 0)
                    newIndex = Count - 1;
                else if (newIndex >= Count)
                    newIndex = 0;
            }
            Select(newIndex);
        }

        public void Select(int index)
        {
            if (!CanSelect(index))
            {
                return;
            }

            SelectedIndex = index;
            OnSelect(new UiListEventArgs<T>(this[index], index));
        }

        public virtual bool CanActivate(int index)
        {
            return index >= 0 && index < Cells.Count;
        }

        public void Activate(int index)
        {
            if (!CanActivate(index))
            {
                return;
            }

            if (SelectOnActivate)
                Select(index);

            OnActivate(new UiListEventArgs<T>(this[index], index));
        }

        #endregion

        #region UI Handling

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            var iy = Y;

            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.XOffset = ItemOffsetX;
                cell.SetPosition(X, iy);

                iy += cell.Height;
            }
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = Vector2i.Zero;

            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.GetPreferredSize(out var cellSize);
                size.X = Math.Max(size.X, cellSize.X);
                size.Y += Math.Max(cellSize.Y, ItemHeight);
            }
        }

        public override void SetSize(int width, int height)
        {
            var totalHeight = 0;

            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.GetPreferredSize(out var cellSize);
                var cellHeight = Math.Max(cellSize.Y, ItemHeight);
                cell.SetSize(width, cellHeight);
                width = Math.Max(width, cell.Width);
                totalHeight += cell.Height;
            }

            base.SetSize(width, Math.Max(height, totalHeight));
        }

        public override void Update(float dt)
        {
            if (_needsUpdate)
            {
                RefreshCellPositionsAndKeys();
                _needsUpdate = false;
            }

            for (int i = 0; i < Count; i++)
            {
                var cell = Cells[i];
                cell.Update(dt);
            }
        }

        public override void Draw()
        {
            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.Draw();

                if (HighlightSelected && index == SelectedIndex)
                {
                    cell.DrawHighlight();
                }
            }
        }

        public override void Dispose()
        {
            foreach (var cell in Cells)
            {
                cell.Dispose();
            }
        }

        #endregion

        #region IList implementation

        public int Count => Cells.Count;
        public bool IsReadOnly => Cells.IsReadOnly;

        public UiListCell<T> this[int index]
        {
            get => Cells[index];
            set
            {
                Cells[index] = value;
                _needsUpdate = true;
            }
        }
        public int IndexOf(UiListCell<T> item)
        {
            return Cells.IndexOf(item);
        }

        public void Insert(int index, UiListCell<T> item)
        {
            Cells.Insert(index, item);
            _needsUpdate = true;
        }

        public void RemoveAt(int index)
        {
            Cells.RemoveAt(index);
            _needsUpdate = true;
        }

        public void Add(UiListCell<T> item)
        {
            Cells.Add(item);
            _needsUpdate = true;
        }

        public void Clear()
        {
            Cells.Clear();
            _needsUpdate = true;
        }

        public void AddRange(IEnumerable<UiListCell<T>> items)
        {
            Cells.AddRange(items);
            _needsUpdate = true;
        }

        public void SetFrom(IEnumerable<T> items)
        {
            Clear();
            AddRange(MakeDefaultList(items));
        }

        public bool Contains(UiListCell<T> item) => Cells.Contains(item);
        public void CopyTo(UiListCell<T>[] array, int arrayIndex) => Cells.CopyTo(array, arrayIndex);
        public bool Remove(UiListCell<T> item)
        {
            _needsUpdate = true;
            return Cells.Remove(item);
        }

        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public IEnumerator<UiListCell<T>> GetEnumerator() => Cells.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cells.GetEnumerator();

        #endregion
    }
}
