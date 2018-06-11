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
using ChadderLib.Util;
using Chadder.Client.Util;
using Chadder.Client.Data;

namespace Chadder.Droid.Adapters
{

    class DevicesAdapter : ObservableCollectionAdapter<ChadderUserDevice>
    {
        public DevicesAdapter(Activity context, ChadderObservableCollection<ChadderUserDevice> items)
            : base(context, Resource.Layout.item_device, items)
        {
        }

        protected override void InitializeNewView(View view, int type)
        {
            var Holder = new DevicesHolder();

            Holder.DeviceName = view.FindViewById<TextView>(Resource.Id.device_name);
            Holder.KeyStatus = view.FindViewById<ImageView>(Resource.Id.device_icon);
            Holder.Current = view.FindViewById(Resource.Id.device_current);

            view.Tag = Holder;
        }

        protected override void PrepareView(ChadderUserDevice item, View view, int position)
        {
            var Holder = view.Tag as DevicesHolder;

            Holder.DeviceName.Text = item.Name;
            if (item.CurrentDevice)
                Holder.Current.Visibility = ViewStates.Visible;
            else
                Holder.Current.Visibility = ViewStates.Invisible;
        }
    }


    internal class DevicesHolder : Java.Lang.Object
    {
        public TextView DeviceName;
        public TextView DeviceId;
        public ImageView KeyStatus;
        public View Current;
    }
}