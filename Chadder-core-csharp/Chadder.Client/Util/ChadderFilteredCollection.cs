using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Chadder.Client.Util
{
    public class ConversationsFiltered : ChadderFilteredCollection<Chadder.Client.Data.ChadderConversation>
    {
        public ConversationsFiltered(ChadderObservableCollection<Chadder.Client.Data.ChadderConversation> lst)
            : base(lst, Compare)
        {

        }
        public static int Compare(Chadder.Client.Data.ChadderConversation e1, Chadder.Client.Data.ChadderConversation e2)
        {
            return 0;
        }
        public override bool isDisplayed(Chadder.Client.Data.ChadderConversation item)
        {
            return item.Hidden == false;
        }
    }
    public class ContactsFiltered : ChadderFilteredCollection<Chadder.Client.Data.ChadderContact>
    {

        public ContactsFiltered(ChadderObservableCollection<Chadder.Client.Data.ChadderContact> lst)
            : base(lst, Compare)
        {

        }
        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                value = value.Trim().ToUpper();
                if (_filter != value)
                {
                    if (value.Length == 0)
                    {
                        value = null;
                    }
                    if (_filter == null || (value != null && value.Contains(_filter)))
                    {
                        _filter = value;
                        refilter();
                    }
                    else
                    {
                        _filter = value;
                        reset();
                    }
                }
            }
        }
        public static int Compare(Chadder.Client.Data.ChadderContact e1, Chadder.Client.Data.ChadderContact e2) 
        {
            if (e1.Type == Chadder.Data.RelationshipType.FRIENDS && e2.Type == Chadder.Data.RelationshipType.BLOCKED)
                return -1;
            if (e2.Type == Chadder.Data.RelationshipType.FRIENDS && e1.Type == Chadder.Data.RelationshipType.BLOCKED)
                return 1;
            return e1.Name.CompareTo(e2.Name);
        }
        public override bool isDisplayed(Chadder.Client.Data.ChadderContact item)
        {
            if (Filter == null)
                return true;
            return item.DisplayName.ToUpper().Contains(Filter);
        }
    }
    public abstract class ChadderFilteredCollection<T> : ChadderOrderedCollection<T> where T : MyNotifyChanged
    {
        private ChadderObservableCollection<T> _collection;
        public ChadderObservableCollection<T> whole { get { return _collection; } }
        public ChadderFilteredCollection(ChadderObservableCollection<T> collection, CompareDelegate order)
            :base(order)
        {
            this._collection = collection;

            collection.CollectionChanged += collection_CollectionChanged;
            collection.PropertyChanged += collection_PropertyChanged;
            reset();
        }

        private void collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (isDisplayed((T)sender) == false)
                Remove((T)sender);
            else
                AddItem((T)sender);
        }

        public void reset()
        {
            SuspendCollectionChangeNotification();
            Clear();

            var copy = new List<T>(whole); // Make thread safe

            foreach (var item in copy)
            {
                if (isDisplayed(item))
                    Add(item);
            }
            NotifyChanges();
        }

        public void refilter()
        {
            SuspendCollectionChangeNotification();
            var items = new List<T>(this);
            foreach (var item in items)
            {
                if (isDisplayed(item) == false)
                    Remove(item);
            }
            NotifyChanges();
        }

        public abstract bool isDisplayed(T item);

        private void AddItem(T item)
        {
            if (isDisplayed(item) && Contains(item) == false)
            {
                if (Count == 0)
                    Add(item);
                else
                {
                    var mainIndex = whole.IndexOf(item);
                    if (mainIndex == -1)
                        return;
                    var filteredIndex = mainIndex;
                    if (mainIndex > Count)
                        filteredIndex = Count;
                    // Insert in the right order
                    while (true)
                    {
                        if (filteredIndex == 0)
                            break; // No more items. has to be at 0
                        var front = _collection[filteredIndex - 1];
                        if (whole.IndexOf(front) > mainIndex)
                            --filteredIndex; // Item is behind
                        else
                            break; // Found an item that should be in front
                    }
                    if (filteredIndex >= Count)
                        Add(item);
                    else
                        InsertItem(filteredIndex, item);
                }
            }
        }

        private void collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddItem((T)item);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in e.OldItems)
                    {
                        Remove((T)item);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move && e.NewItems.Count == 1)
                {
                    var item = (T)e.NewItems[0];
                    var wholeIndex = e.NewStartingIndex;
                    var current = IndexOf(item);
                    for (int i = 0; i < current; ++i)
                    {
                        if (whole.IndexOf(this[i]) > wholeIndex)
                        {
                            Move(current, i);
                            break;
                        }
                    }
                }
                else
                {
                    reset();
                }
            }
            catch (Exception ex)
            {
                Insight.Track("FilteredCollection_CollectionChanged " + e.Action.ToString());
                Insight.Report(ex);
            }
        }
    }
}
