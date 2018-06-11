using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace Chadder.Client.Util
{
    public class ChadderObservableCollection<T> : ObservableCollection<T>, INotifyPropertyChanged where T : MyNotifyChanged
    {
        private bool suspendCollectionChangeNotification;
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public ChadderObservableCollection()
            : base()
        {
            this.suspendCollectionChangeNotification = false;
        }

        public virtual new void Add(T item)
        {
            base.Add(item);
        }

        public virtual void AddItems(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
            // TODO: optimize for groups
        }

        public void NotifyChanges()
        {
            this.ResumeCollectionChangeNotification();
            var arg
                 = new NotifyCollectionChangedEventArgs
                      (NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(arg);
        }

        public void ResumeCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = false;
        }

        public void SuspendCollectionChangeNotification()
        {
            this.suspendCollectionChangeNotification = true;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += MyObservableCollection_PropertyChanged;
                }
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= MyObservableCollection_PropertyChanged;
                }
            using (BlockReentrancy())
            {
                if (!this.suspendCollectionChangeNotification)
                {
                    NotifyCollectionChangedEventHandler eventHandler =
                          this.CollectionChanged;
                    if (eventHandler == null)
                    {
                        return;
                    }

                    // Walk thru invocation list.
                    Delegate[] delegates = eventHandler.GetInvocationList();

                    foreach
                    (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        handler(this, e);
                    }
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        protected virtual void MyObservableCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, e);
            }
        }
    }
}
