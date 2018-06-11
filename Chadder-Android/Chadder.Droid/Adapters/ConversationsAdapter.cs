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
using System.Collections.ObjectModel;
using Chadder.Droid.Util;
using Chadder.Client.Data;
using Chadder.Client.Util;

namespace Chadder.Droid.Adapters
{
    class ConversationsAdapter : ObservableCollectionAdapter<ChadderConversation>
    {
        Activity _context;
        int _itemResource;
        ChadderObservableCollection<ChadderConversation> _items;

        public ConversationsAdapter(Activity context, int resource, ChadderObservableCollection<ChadderConversation> items)
            : base(context, resource, items)
        {
            _context = context;
            _itemResource = resource;
            _items = items;
        }

        protected override void InitializeNewView(View view, int type)
        {
            var Holder = new ConversationHolder();

            try
            {
                var im = view.FindViewById<ImageViewEx>(Resource.Id.conversationPic);
                Holder.ConversationPic = im;
                Holder.ConversationPic.IsThumbnail = true;
                Holder.ConversationPic.IsCircle = true;
            }
            catch (Exception ex)
            {

            }

            Holder.DisplayName = view.FindViewById<TextView>(Resource.Id.display_name);
            Holder.LastMessage = view.FindViewById<TextView>(Resource.Id.conversation_lastmsg);
            Holder.Timestamp = view.FindViewById<TextView>(Resource.Id.conversation_time);
            Holder.NewMessage = view.FindViewById<ImageView>(Resource.Id.conversation_new);
            view.Tag = Holder;
        }

        protected override void PrepareView(ChadderConversation item, View view, int position)
        {
            var Holder = view.Tag as ConversationHolder;
            Holder.DisplayName.Text = item.DisplayName;

            var lastMessage = item.Messages.LastOrDefault();
            Holder.LastMessage.Text = lastMessage == null ? "" : lastMessage.Preview;
            Holder.Timestamp.Text = lastMessage == null ? "" : lastMessage.TimeDisplayCompact;
            try
            {
                //++todo need fix
                Holder.ConversationPic.Picture = item.Picture;
            }
            catch (Exception ex)
            {

            }

            if (item.NumNewMessages > 0)
                Holder.NewMessage.Visibility = ViewStates.Visible;
            else
                Holder.NewMessage.Visibility = ViewStates.Gone;


        }
    }

    class ConversationHolder : Java.Lang.Object
    {
        public ImageViewEx ConversationPic { get; set; }
        public TextView DisplayName { get; set; }
        public TextView LastMessage { get; set; }
        public TextView Timestamp { get; set; }
        public ImageView NewMessage { get; set; }
    }
}