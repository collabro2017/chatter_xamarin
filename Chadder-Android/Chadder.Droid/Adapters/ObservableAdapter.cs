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
using System.ComponentModel;
using Chadder.Data.Util;

namespace Chadder.Droid.Adapters
{
    public class BasicHolder : Java.Lang.Object
    {
        public int ItemType { get; set; }
    }
    abstract class ObservableAdapter<T> : BaseAdapter<T> where T : INotifyPropertyChanged
    {
        private Dictionary<T, PropertyChangedEventHandler> ChangedEvents = new Dictionary<T, PropertyChangedEventHandler>();
        private Dictionary<View, Tuple<int, T>> initializedViews = new Dictionary<View, Tuple<int, T>>();
        protected Activity Context { get; private set; }

        public ObservableAdapter(Activity c)
        {
            Context = c;
        }
        protected virtual void InitializeNewView(View view, int itemType)
        {
        }
        protected virtual void PrepareView(T item, View view, int position)
        {
        }
        public View GetVisableViewFor(T item)
        {
            return initializedViews.Where(i => i.Value.Equals(item)).Select(i => i.Key).FirstOrDefault();
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override int ViewTypeCount
        {
            get { return 1; }
        }
        public override int GetItemViewType(int position)
        {
            if (ViewTypeCount == 1)
                return 0;
            else
                throw new NotImplementedException();
        }
        protected abstract int GetResourceByItemType(int itemType);

        public override View GetView(int position, View view, ViewGroup parent)
        {
            var shouldBeType = GetItemViewType(position);
            bool createNewView = view == null;
            if (createNewView == false)
            {
                if (initializedViews.ContainsKey(view) == true)
                {
                    var tuple = initializedViews[view];
                    if (tuple.Item1 != shouldBeType)
                        createNewView = true;
                }
                else
                    createNewView = true;
            }
            if (createNewView)
            {
                var layoutResource = GetResourceByItemType(shouldBeType);
                view = this.Context.LayoutInflater.Inflate(layoutResource, null);
                this.InitializeNewView(view, shouldBeType);
            }

            T item = this[position];

            if (initializedViews.ContainsKey(view) == false || item == null || item.Equals(initializedViews[view].Item2) == false)
            {
                this.initializedViews[view] = new Tuple<int, T>(shouldBeType, item);
                try
                {
                    this.PrepareView(item, view, position);
                }
                catch (Exception ex)
                {
                    Insight.Track("Exception thrown PrepareView - " + this.GetType().Name);
                    Insight.Report(ex);
                }
            }
            if (item != null)
            {
                PropertyChangedEventHandler Changed = (a, e) =>
                {
                    try
                    {
                        PrepareView(item, view, position);
                    }
                    catch (Exception ex)
                    {
                        Insight.Track("Exception thrown PrepareView - " + this.GetType().Name);
                        Insight.Report(ex);
                    }
                };
                if (ChangedEvents.ContainsKey(item) == true)
                    item.PropertyChanged -= ChangedEvents[item];

                ChangedEvents[item] = Changed;

                item.PropertyChanged += Changed;
            }
            return view;
        }

        protected bool OnPauseCalled = false;
        public virtual void OnPause()
        {
            foreach (var item in ChangedEvents)
            {
                item.Key.PropertyChanged -= item.Value;
            }
            OnPauseCalled = true;
        }

        public virtual bool OnResume()
        {
            if (OnPauseCalled)
            {
                NotifyDataSetInvalidated();
                foreach (var item in ChangedEvents)
                {
                    item.Key.PropertyChanged += item.Value;
                }
            }
            return OnPauseCalled;
        }
    }
}