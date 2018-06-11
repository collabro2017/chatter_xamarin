using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using Chadder.Client.Data;

namespace Chadder.Client.Util
{
    public class ChadderOrderedCollection<T> : ChadderObservableCollection<T> where T : MyNotifyChanged
    {
        public delegate int CompareDelegate(T e1, T e2);
        public CompareDelegate Compare;
        public ChadderOrderedCollection(CompareDelegate order)
        {
            Compare = order;
        }
        public override void Add(T item)
        {
            InsertItem(GetFinalPosition(item), item);
        }

        private int GetFinalPosition(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (Compare(item, this.ElementAt(i)) < 0)
                {
                    return i;
                }
            }
            return Count;
        }

        private int GetNewPosition(T item)
        {
            int old = IndexOf(item);
            for (int i = 0; i < Count; ++i)
            {
                if (Compare(item, this.ElementAt(i)) < 0)
                {
                    if (old != -1 && old < i)
                        return i - 1;
                    else
                        return i;
                }
            }
            if (old != -1)
                return Count - 1;
            else
                return Count;
        }

        public void resort()
        {
            SuspendCollectionChangeNotification();
            bool anyChange = false;
            bool changed = true;
            do
            {
                changed = false;
                for (int i = 1; i < Count; ++i)
                {
                    if (Compare(this.ElementAt(i), this.ElementAt(i - 1)) < 0)
                    {
                        Move(i, i - 1);
                        changed = true;
                        anyChange = true;
                    }
                }

            } while (changed);
            if (anyChange)
                NotifyChanges();
            else
                ResumeCollectionChangeNotification();
        }
        protected override void MyObservableCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.MyObservableCollection_PropertyChanged(sender, e);
            var item = (T)sender;
            int old = IndexOf(item);
            if (old == -1)
                return;
            int newIndex = GetNewPosition(item);
            if (old != newIndex)
            {
#if !WINDOWS_PHONE
                Move(old, newIndex);
#else
                Remove(item);
                Add(item);
#endif
            }
        }
    }
}
