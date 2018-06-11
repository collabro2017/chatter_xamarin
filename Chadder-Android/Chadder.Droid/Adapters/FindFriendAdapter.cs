using Android.App;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Views;
using Chadder.Droid.Util;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using UIHelper;
using Chadder.Client.Data;

namespace Chadder.Droid.Adapters
{
    public class FindContactHolder : BasicHolder
    {
        public TextView Username { get; set; }
        public ImageViewEx ProfilePic { get; set; }
        public ImageView blockedMark { get; set; }
        public TextView Name { get; set; }
    }
    class FindFriendAdapter : ObservableCollectionAdapter<ChadderContact>
    {
        public FindContactObservableCollection Collection;
        public FindFriendAdapter(Activity context, View hideUnlessEmpty, FindContactObservableCollection collection)
            : base(context, 0, collection)
        {
            EmptyItem = hideUnlessEmpty;
            Collection = collection;
        }

        public override int ViewTypeCount
        {
            get
            {
                return 2;
            }
        }
        public override int GetItemViewType(int position)
        {
            //++if (this[position] is ContactGroupSeparator)
            //++return 1;
            return 0;
        }
        protected override int GetResourceByItemType(int type)
        {
            if (type == 0)
                return Resource.Layout.item_contact_find;
            return Resource.Layout.item_contact_mark;
        }
        protected override void InitializeNewView(View view, int itemType)
        {
            var Holder = new FindContactHolder();
            Holder.ProfilePic = view.FindViewById<ImageViewEx>(Resource.Id.profilePic);
            if (Holder.ProfilePic != null)
            {
                Holder.ProfilePic.IsThumbnail = true;
                Holder.ProfilePic.IsCircle = true;
            }
            Holder.Name = view.FindViewById<TextView>(Resource.Id.contacts_name);
            Holder.Username = view.FindViewById<TextView>(Resource.Id.contacts_username);
            Holder.blockedMark = view.FindViewById<ImageView>(Resource.Id.contact_blocked);
            view.Tag = Holder;
        }
        protected override void PrepareView(ChadderContact item, View view, int position)
        {
            var Holder = view.Tag as FindContactHolder;
            if (Holder != null)
            {
                Holder.Name.Text = item.Name;
                //++if (!(item is ContactGroupSeparator))
                //++{
                Holder.ProfilePic.Picture = item.Picture;
                Holder.Username.Text = item.Username;

                if (item.Type == Data.RelationshipType.BLOCKED)
                    Holder.blockedMark.Visibility = ViewStates.Visible;
                else
                    Holder.blockedMark.Visibility = ViewStates.Invisible;
                //++}
            }
        }
    }
}