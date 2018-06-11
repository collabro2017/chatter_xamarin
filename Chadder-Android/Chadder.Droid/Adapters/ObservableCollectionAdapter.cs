using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Chadder.Droid.Adapters
{
    public delegate void CollectionChangedEventHandler(object sender, EventArgs e);

    abstract class ObservableCollectionAdapter<T> : ObservableAdapter<T> where T : INotifyPropertyChanged
    {
        private readonly IList<T> items;
        private readonly int resource;
        private View _empty;
        public View EmptyItem
        {
            get { return _empty; }
            set
            {
                if (_empty != value)
                {
                    _empty = value;
                    updateEmpty();
                }
            }
        }
        public void updateEmpty()
        {
            if (_empty != null)
            {
                _empty.Visibility = Count > 0 ? ViewStates.Gone : ViewStates.Visible;
            }
        }

        public ObservableCollectionAdapter(Activity context, int resource, IList<T> items)
            : base(context)
        {
            this.resource = resource;
            this.items = items;
            if (this.items is INotifyCollectionChanged)
                (this.items as INotifyCollectionChanged).CollectionChanged += this.OnCollectionChanged;
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
            updateEmpty();
        }

        public override T this[int position]
        {
            get { return this.items[position]; }
        }

        public override int Count
        {
            get { return this.items.Count; }
        }

        public override void OnPause()
        {
            base.OnPause();
            if (this.items is INotifyCollectionChanged)
                (this.items as INotifyCollectionChanged).CollectionChanged -= this.OnCollectionChanged;
        }
        public override bool OnResume()
        {
            if (base.OnResume())
            {
                if (this.items is INotifyCollectionChanged)
                    (this.items as INotifyCollectionChanged).CollectionChanged += this.OnCollectionChanged;
                return true;
            }
            return false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnPause();
            }

            base.Dispose(disposing);
        }

        protected override int GetResourceByItemType(int itemType)
        {
            if (this.GetItemViewType(itemType) == 0)
            {
                return resource;
            }
            else
                throw new NotImplementedException();
        }
    }

    public class ExtraInfo<T> : Java.Lang.Object
    {
        public ExtraInfo(T t)
        {
            Info = t;
        }
        public T Info;
    }
}