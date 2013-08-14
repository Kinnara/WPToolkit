using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    internal interface ISelector<T> where T : DependencyObject
    {
        bool CanSelectMultiple { get; }

        ItemCollection Items { get; }

        DataTemplate ItemTemplate { get; }

        int SelectedIndex { get; set; }

        object SelectedItem { get; set; }

        void OnSelectionChanged(SelectionChangedEventArgs e);

        void OnSelectionChanged(int oldIndex, int newIndex, object oldValue, object newValue);

        void SetItemIsSelected(object item, bool value);

        bool GetIsSelected(T element);

        T GetSelectorItem(int index);
    }

    internal sealed class SelectorHelper<T> where T : DependencyObject
    {
        private InitializingData _initializingData;

        internal InternalSelectedItemsStorage _selectedItems;

        private bool _selectedItemsStale;

        internal SelectionChanger _selectionChanger;

        private ObservableCollection<object> _selectedItemsImpl;

        public SelectorHelper(ISelector<T> selector)
        {
            Selector = selector;

            _selectedItemsImpl = new ObservableCollection<object>();
            _selectedItemsImpl.CollectionChanged += OnSelectedItemsCollectionChanged;
            _selectedItems = new InternalSelectedItemsStorage(1);
            _selectionChanger = new SelectionChanger(this);
            _selectedItemsStale = false;
        }

        public ISelector<T> Selector { get; private set; }

        internal IList SelectedItemsImpl
        {
            get
            {
                if (_selectedItemsStale)
                {
                    UpdateSelectedItems();
                }

                return _selectedItemsImpl;
            }
        }

        private bool IsInit
        {
            get { return _initializingData != null; }
        }

        internal void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (_selectionChanger.IsActive)
            {
                return;
            }

            if (IsInit)
            {
                return;
            }

            if (newIndex >= -1 && newIndex < Selector.Items.Count)
            {
                _selectionChanger.SelectJustThisItem(oldIndex, newIndex);
            }
            else
            {
                try
                {
                    _selectionChanger.Begin();
                    Selector.SelectedIndex = oldIndex;
                }
                finally
                {
                    _selectionChanger.Cancel();
                }

                throw new ArgumentOutOfRangeException("SelectedIndex");
            }
        }

        internal void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (_selectionChanger.IsActive)
            {
                return;
            }

            if (IsInit)
            {
                return;
            }

            try
            {
                if ((Selector.Items.IndexOf(newValue) == -1 && newValue != null))
                {
                    try
                    {
                        _selectionChanger.Begin();
                        Selector.SelectedItem = oldValue;
                    }
                    finally
                    {
                        _selectionChanger.Cancel();
                    }
                }
                else
                {
                    _selectionChanger.SelectJustThisItem(newValue);
                }
            }
            finally
            {
                if (_selectionChanger.IsActive)
                {
                    _selectionChanger.Cancel();
                }
            }
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_selectionChanger.IsActive)
            {
                return;
            }

            _selectedItemsStale = true;

            if (!Selector.CanSelectMultiple)
            {
                throw new InvalidOperationException(Properties.Resources.Selector_CannotModifySelectedItems);
            }

            if (IsInit)
            {
                return;
            }

            try
            {
                _selectionChanger.Begin();

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems.Count != 1)
                        {
                            throw new NotSupportedException(Properties.Resources.Selector_RangeActionsNotPermitted);
                        }

                        _selectionChanger.Select(Selector.Items.IndexOf(e.NewItems[0]), e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems.Count != 1)
                        {
                            throw new NotSupportedException(Properties.Resources.Selector_RangeActionsNotPermitted);
                        }

                        _selectionChanger.Unselect(e.OldItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewItems.Count != 1 || e.OldItems.Count != 1)
                        {
                            throw new NotSupportedException(Properties.Resources.Selector_RangeActionsNotPermitted);
                        }

                        _selectionChanger.Unselect(e.OldItems[0]);
                        _selectionChanger.Select(Selector.Items.IndexOf(e.NewItems[0]), e.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        for (int index = 0; index < _selectedItems.Count; ++index)
                        {
                            _selectionChanger.Unselect(_selectedItems[index]);
                        }

                        if (e.NewItems != null)
                        {
                            for (int index = 0; index < e.NewItems.Count; ++index)
                            {
                                _selectionChanger.Select(Selector.Items.IndexOf(e.NewItems[index]), e.NewItems[index]);
                            }
                        }
                        break;
                    default:
                        throw new InvalidOperationException(Properties.Resources.Selector_UnknownCollectionAction);
                }
                _selectionChanger.End();
            }
            finally
            {
                if (_selectionChanger.IsActive)
                {
                    _selectionChanger.Cancel();
                }
            }
        }

        private void InvokeSelectionChanged(List<object> unselectedItems, List<object> selectedItems)
        {
            Selector.OnSelectionChanged(new SelectionChangedEventArgs(unselectedItems, selectedItems));
        }

        internal void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            int selectedIndex = Selector.SelectedIndex;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    try
                    {
                        _selectedItems.InvalidateStoredIndexes(e.NewStartingIndex);
                        _selectionChanger.Begin();

                        bool anySelected = false;
                        for (int index = 0; index < e.NewItems.Count; ++index)
                        {
                            T selectorItem = e.NewItems[index] as T;
                            if (selectorItem != null && Selector.GetIsSelected(selectorItem))
                            {
                                anySelected = true;
                                _selectionChanger.Select(e.NewStartingIndex + index, selectorItem);
                            }
                        }

                        if (anySelected)
                        {
                            _selectionChanger.End();
                        }
                    }
                    finally
                    {
                        if (_selectionChanger.IsActive)
                        {
                            if (e.NewStartingIndex <= selectedIndex && !IsInit)
                            {
                                UpdatePublicSelectionProperties(Selector.SelectedIndex, selectedIndex + e.NewItems.Count, Selector.SelectedItem, Selector.SelectedItem);
                            }

                            _selectionChanger.Cancel();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    try
                    {
                        _selectedItems.InvalidateStoredIndexes(e.OldStartingIndex);
                        _selectionChanger.Begin();

                        foreach (object item in e.OldItems)
                        {
                            if (_selectedItems.Contains(item))
                            {
                                _selectionChanger.Unselect(item);
                            }
                        }
                    }
                    finally
                    {
                        _selectionChanger.End();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    try
                    {
                        _selectedItems.InvalidateStoredIndexes(e.NewStartingIndex);
                        _selectionChanger.Begin();

                        foreach (object item in e.OldItems)
                        {
                            if (_selectedItems.Contains(item))
                            {
                                _selectionChanger.Unselect(item);
                            }
                        }

                        for (int index = 0; index < e.NewItems.Count; ++index)
                        {
                            T selectorItem = e.NewItems[index] as T;
                            if (selectorItem != null && Selector.GetIsSelected(selectorItem))
                            {
                                _selectionChanger.Select(e.NewStartingIndex + index, selectorItem);
                            }
                        }
                    }
                    finally
                    {
                        _selectionChanger.End();
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    try
                    {
                        _selectionChanger.Begin();

                        foreach (object item in _selectedItems)
                        {
                            _selectionChanger.Unselect(item);
                        }

                        if (Selector.ItemTemplate == null)
                        {
                            for (int index = 0; index < Selector.Items.Count; ++index)
                            {
                                T selectorItem = Selector.GetSelectorItem(index);
                                if (selectorItem != null && Selector.GetIsSelected(selectorItem))
                                {
                                    _selectionChanger.Select(index, Selector.Items[index]);
                                }
                            }
                        }
                    }
                    finally
                    {
                        _selectionChanger.End();
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void UpdateSelectedItems()
        {
            _selectedItemsStale = false;

            try
            {
                _selectionChanger.Begin();

                IList list = _selectedItemsImpl;
                if (list != null)
                {
                    InternalSelectedItemsStorage selectedItemsStorage = new InternalSelectedItemsStorage(list.Count);

                    for (int index = 0; index < list.Count; ++index)
                    {
                        object item = list[index];
                        if (_selectedItems.Contains(item) && !selectedItemsStorage.Contains(item))
                        {
                            selectedItemsStorage.Add(item, _selectedItems.StoredIndexOf(item));
                        }
                        else
                        {
                            list.RemoveAt(index);
                            --index;
                        }
                    }

                    foreach (object item in _selectedItems)
                    {
                        if (!selectedItemsStorage.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            finally
            {
                _selectionChanger.Cancel();
            }
        }

        private void UpdatePublicSelectionProperties(int oldSelectedIndex, int newSelectedIndex, object oldSelectedItem, object newSelectedItem)
        {
            if (oldSelectedIndex == newSelectedIndex && InternalUtils.AreValuesEqual(oldSelectedItem, newSelectedItem))
            {
                return;
            }

            Selector.SelectedIndex = newSelectedIndex;
            Selector.SelectedItem = newSelectedItem;
            Selector.OnSelectionChanged(oldSelectedIndex, Selector.SelectedIndex, oldSelectedItem, Selector.SelectedItem);
        }

        internal void SelectRange(int startIndex, int endIndex, bool clearOldSelection)
        {
            try
            {
                _selectionChanger.Begin();

                if (clearOldSelection)
                {
                    foreach (object item in _selectedItems)
                    {
                        _selectionChanger.Unselect(item);
                    }
                }

                for (; startIndex <= endIndex; ++startIndex)
                {
                    if (Selector.Items[startIndex] != null)
                    {
                        _selectionChanger.Select(startIndex, Selector.Items[startIndex]);
                    }
                }
            }
            finally
            {
                _selectionChanger.End();
            }
        }

        internal void SelectRange(IEnumerable collection)
        {
            try
            {
                _selectionChanger.Begin();

                foreach (object o in _selectedItems)
                {
                    _selectionChanger.Unselect(o);
                }

                foreach (object item in collection)
                {
                    int index = Selector.Items.IndexOf(item);
                    if (index != -1)
                    {
                        _selectionChanger.Select(index, item);
                    }
                }
            }
            finally
            {
                _selectionChanger.End();
            }
        }

        internal void BeginInit()
        {
            _initializingData = new InitializingData
            {
                InitialItem = Selector.SelectedItem,
                InitialIndex = Selector.SelectedIndex
            };
        }

        internal void EndInit()
        {
            if (_initializingData == null)
            {
                throw new InvalidOperationException();
            }

            int selectedIndex = Selector.SelectedIndex;
            object selectedItem = Selector.SelectedItem;
            if (_selectedItems.Count == 0)
            {
                if (_initializingData.InitialIndex != selectedIndex)
                {
                    Selector.SelectedIndex = _initializingData.InitialIndex;
                    _initializingData = null;
                    Selector.SelectedIndex = selectedIndex;
                }
                else if (!ReferenceEquals(_initializingData.InitialItem, selectedItem))
                {
                    Selector.SelectedItem = _initializingData.InitialItem;
                    _initializingData = null;
                    Selector.SelectedItem = selectedItem;
                }
            }

            _initializingData = null;
        }

        #region Helper Classes

        private class InitializingData
        {
            public int InitialIndex;
            public object InitialItem;
        }

        internal class SelectionChanger
        {
            private SelectorHelper<T> _owner;
            private bool _isActive;
            private InternalSelectedItemsStorage _toSelect;
            private InternalSelectedItemsStorage _toUnselect;
            private int? _selectedIndex;

            internal bool IsActive
            {
                get { return _isActive; }
            }

            internal SelectionChanger(SelectorHelper<T> owner)
            {
                _owner = owner;
                _isActive = false;
                _toSelect = new InternalSelectedItemsStorage(1);
                _toUnselect = new InternalSelectedItemsStorage(1);
                _selectedIndex = new int?();
            }

            internal void Begin()
            {
                _isActive = true;
                _toSelect.Clear();
                _toUnselect.Clear();
            }

            internal void End()
            {
                List<object> unselected = new List<object>();
                List<object> selected = new List<object>();

                try
                {
                    ApplyCanSelectMultiple();

                    CreateDeltaSelectionChange(unselected, selected);

                    object newSelectedItem = null;
                    int newSelectedIndex = -1;

                    if (_owner._selectedItems.Count > 0)
                    {
                        newSelectedItem = _owner._selectedItems[0];
                        newSelectedIndex = _owner._selectedItems.StoredIndexOf(newSelectedItem);
                        if (newSelectedIndex == -1)
                        {
                            newSelectedIndex = _owner.Selector.Items.IndexOf(newSelectedItem);
                            _owner._selectedItems.StoreIndex(newSelectedItem, newSelectedIndex);
                        }
                    }

                    _owner.UpdatePublicSelectionProperties(_selectedIndex.HasValue ? _selectedIndex.Value : _owner.Selector.SelectedIndex, newSelectedIndex, _owner.Selector.SelectedItem, newSelectedItem);
                }
                finally
                {
                    Cleanup();
                }

                if (unselected.Count > 0 || selected.Count > 0)
                {
                    _owner.InvokeSelectionChanged(unselected, selected);
                }
            }

            internal bool Select(int index, object o)
            {
                if (_toUnselect.Remove(o))
                {
                    return true;
                }

                if (_owner._selectedItems.Contains(o) || _toSelect.Contains(o) || index == -1)
                {
                    return false;
                }

                if (!_owner.Selector.CanSelectMultiple && _toSelect.Count > 0)
                {
                    foreach (object item in _toSelect)
                    {
                        _toUnselect.Add(item, -1);
                    }
                    _toSelect.Clear();
                }

                _toSelect.Add(o, index);
                return true;
            }

            internal bool Unselect(object o)
            {
                if (_toSelect.Remove(o))
                {
                    return true;
                }

                if (!_owner._selectedItems.Contains(o) || _toUnselect.Contains(o))
                {
                    return false;
                }

                _toUnselect.Add(o, -1);
                return true;
            }

            internal void SelectJustThisItem(object o)
            {
                try
                {
                    Begin();

                    foreach (object item in _owner._selectedItems)
                    {
                        Unselect(item);
                    }

                    if (o != null)
                    {
                        Select(_owner.Selector.Items.IndexOf(o), o);
                    }
                }
                finally
                {
                    End();
                }
            }

            internal void SelectJustThisItem(int oldIndex, int newIndex)
            {
                try
                {
                    Begin();

                    foreach (object item in _owner._selectedItems)
                    {
                        Unselect(item);
                    }

                    if (newIndex >= 0)
                    {
                        Select(newIndex, _owner.Selector.Items[newIndex]);
                        _selectedIndex = oldIndex;
                    }
                }
                finally
                {
                    End();
                }
            }

            internal void Cancel()
            {
                Cleanup();
            }

            internal void Cleanup()
            {
                _selectedIndex = null;
                _isActive = false;
                _toSelect.Clear();
                _toUnselect.Clear();
            }

            private void ApplyCanSelectMultiple()
            {
                if (!_owner.Selector.CanSelectMultiple)
                {
                    if (_toSelect.Count == 1)
                    {
                        _toUnselect = new InternalSelectedItemsStorage(_owner._selectedItems);
                    }
                    else
                    {
                        if (_owner._selectedItems.Count > 1 && _owner._selectedItems.Count != _toUnselect.Count + 1)
                        {
                            object selectedItem = _owner._selectedItems[0];

                            _toUnselect.Clear();
                            foreach (object item in _owner._selectedItems)
                            {
                                if (!Equals(item, selectedItem))
                                {
                                    _toUnselect.Add(item, -1);
                                }
                            }
                        }
                    }
                }
            }

            private void CreateDeltaSelectionChange(List<object> unselectedItems, List<object> selectedItems)
            {
                if (_toUnselect.Count > 0 || _toSelect.Count > 0)
                {
                    InternalSelectedItemsStorage selectedItemsStorage = new InternalSelectedItemsStorage(_owner._selectedItems);
                    _owner._selectedItems.Clear();
                    _owner._selectedItemsStale = true;

                    foreach (object t in _toUnselect)
                    {
                        _owner.Selector.SetItemIsSelected(t, false);
                        if (selectedItemsStorage.Contains(t))
                        {
                            unselectedItems.Add(t);
                        }
                    }

                    foreach (object t in selectedItemsStorage)
                    {
                        if (!_toUnselect.Contains(t))
                        {
                            _owner._selectedItems.Add(t, selectedItemsStorage.StoredIndexOf(t));
                        }
                    }

                    foreach (object t in _toSelect)
                    {
                        _owner.Selector.SetItemIsSelected(t, true);
                        if (!_owner._selectedItems.Contains(t))
                        {
                            _owner._selectedItems.Add(t, _toSelect.StoredIndexOf(t));
                            selectedItems.Add(t);
                        }
                    }
                }
            }
        }

        internal class InternalSelectedItemsStorage : IEnumerable
        {
            private static object nullObject = new object();
            private List<object> _list;
            private Dictionary<object, int> _set;

            internal InternalSelectedItemsStorage(int capacity)
            {
                _list = new List<object>(capacity);
                _set = new Dictionary<object, int>(capacity);
            }

            internal InternalSelectedItemsStorage(InternalSelectedItemsStorage collection)
            {
                _list = new List<object>(collection._list);
                _set = new Dictionary<object, int>(collection._set);
            }

            public object this[int index]
            {
                get { return _list[index]; }
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public void Add(object t, int index)
            {
                _set.Add(GetStableRef(t), index);
                _list.Add(t);
            }

            public bool Remove(object t)
            {
                if (!_set.Remove(GetStableRef(t)))
                {
                    return false;
                }

                _list.Remove(t);
                return true;
            }

            public bool Contains(object t)
            {
                return _set.ContainsKey(GetStableRef(t));
            }

            public void Clear()
            {
                _list.Clear();
                _set.Clear();
            }

            public bool StoreIndex(object t, int index)
            {
                if (!Contains(t))
                {
                    return false;
                }

                _set[GetStableRef(t)] = index;
                return true;
            }

            public int StoredIndexOf(object t)
            {
                if (!Contains(t))
                {
                    return -1;
                }
                else
                {
                    return _set[GetStableRef(t)];
                }
            }

            public void InvalidateStoredIndexes(int index)
            {
                foreach (object t in _list)
                {
                    if (_set[GetStableRef(t)] >= index)
                    {
                        _set[GetStableRef(t)] = -1;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            private object GetStableRef(object t)
            {
                if (t == null)
                {
                    return nullObject;
                }
                else
                {
                    return t;
                }
            }
        }

        #endregion
    }
}
