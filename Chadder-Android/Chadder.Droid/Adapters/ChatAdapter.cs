using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Widget;
using Android.Views;

using Chadder.Droid.Adapters;
using System.Collections.ObjectModel;
using Android.Content;
using Chadder.Droid.Views;
using Chadder.Droid.Views.Media;
using Chadder.Droid.Util;
using Chadder.Droid.Views.Contacts;
using Chadder.Client.Data;


namespace Chadder.Droid.Adapters
{
    class ChatAdapter : ObservableCollectionAdapter<ChadderMessage>
    {
        const int MY_MESSAGE_LAYOUT = 0;
        const int CONTACT_MESSAGE_LAYOUT = 1;
        const int SYSTEM_MESSAGE_LAYOUT = 2;
        const int CONTACT_MESSAGE_GHOST_LAYOUT = 3;
        const int MY_MESSAGE_GHOST_LAYOUT = 4;

        private ChadderConversation _conversation;

        public override int ViewTypeCount
        {
            get
            {
                return 5;
            }
        }
        private ListView lstView;
        public ChatAdapter(Activity context, ChadderConversation conversation, ListView listView)
            : base(context, 0, conversation.Messages)
        {
            _conversation = conversation;
            this.lstView = listView;
        }

        protected override void InitializeNewView(View view, int layoutResource)
        {
            var Holder = new MessageHolder();
            try
            {
                Holder.ProfilePic = view.FindViewById<ImageViewEx>(Resource.Id.chat_msg_profilePic);
            }
            catch (Exception ex)
            {

            }

            Holder.Body = view.FindViewById<TextView>(Resource.Id.chat_msg_textview);
            Holder.Time = view.FindViewById<TextView>(Resource.Id.chat_msg_time);
            Holder.Name = view.FindViewById<TextView>(Resource.Id.chat_msg_name);
            Holder.Status = view.FindViewById<ImageView>(Resource.Id.chat_msg_status);

            try
            {
                Holder.Media = view.FindViewById<ImageView>(Resource.Id.chat_msg_media);
            }
            catch (Exception ex)
            {

            }

            Holder.MediaLayer = view.FindViewById<RelativeLayout>(Resource.Id.chat_media_layer);
            Holder.LoadingIndicator = view.FindViewById<ProgressBar>(Resource.Id.chat_media_loading);
            if (Holder.ProfilePic != null)
            {
                Holder.ProfilePic.IsThumbnail = true;
                Holder.ProfilePic.IsCircle = true;
                Holder.ProfilePic.Click += OnClickProfilePic;
            }
            if (Holder.Media != null)
            {
                Holder.Media.Click += OnClickMedia;
            }

            view.Tag = Holder;
        }

        protected override void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(sender, e);
            lstView.SmoothScrollToPosition(lstView.Count - 1);
        }

        public override int GetItemViewType(int position)
        {
            var item = this[position];
            if (item.Type == ChadderMessage.MESSAGE_TYPE.SYSTEM)
                return SYSTEM_MESSAGE_LAYOUT;

            return (item.MyMessage) ? MY_MESSAGE_LAYOUT : CONTACT_MESSAGE_LAYOUT;
        }

        protected override int GetResourceByItemType(int itemType)
        {
            switch (itemType)
            {
                case MY_MESSAGE_LAYOUT:
                    return Resource.Layout.chat_my_message;
                case CONTACT_MESSAGE_LAYOUT:
                    return Resource.Layout.chat_contact_message;
                case MY_MESSAGE_GHOST_LAYOUT:
                    return Resource.Layout.chat_my_message_ghost;
                case CONTACT_MESSAGE_GHOST_LAYOUT:
                    return Resource.Layout.chat_contact_message_ghost;
                default:
                    return Resource.Layout.chat_system_message;
            }
        }

        protected void DisplayText(MessageHolder Holder, string text)
        {
            Holder.Body.Text = text;
            Holder.Body.Visibility = ViewStates.Visible;
            if (Holder.MediaLayer != null)
                Holder.MediaLayer.Visibility = ViewStates.Gone;
        }

        protected override void PrepareView(ChadderMessage item, View view, int position)
        {
            var Holder = view.Tag as MessageHolder;
            Holder.message = item;

            if (Holder.Status != null)
            {
                if (item.Status == ChadderMessage.MESSAGE_STATUS.SENT)
                    Holder.Status.Visibility = ViewStates.Gone;
                else
                    Holder.Status.Visibility = ViewStates.Visible;
            }

            if (item.Type != ChadderMessage.MESSAGE_TYPE.SYSTEM && Holder.Name != null)
            {
                Holder.Name.Text = item.Sender.DisplayName;
            }

            if (Holder.ProfilePic != null)
            {
                Holder.ProfilePic.Tag = new ExtraInfo<string>(item.UserId);
                Holder.ProfilePic.Picture = item.Sender.Picture;
            }

            switch (item.Type)
            {
                case ChadderMessage.MESSAGE_TYPE.SYSTEM:
                case ChadderMessage.MESSAGE_TYPE.TEXT:
                    DisplayText(Holder, item.Body);
                    break;
                case ChadderMessage.MESSAGE_TYPE.PICTURE:
                    if (item.Picture.IsAvailableOffline)
                        try
                        {
                            //++todo need fix
                            Holder.Media.SetImageBitmap(item.Picture.Thumbnail);
                        }
                        catch (Exception ex)
                        {

                        }
                    else
                        Holder.Media.SetImageResource(Resource.Drawable.ic_download_picture);
                    try
                    {
                        Holder.Media.Tag = Holder;
                    }
                    catch (Exception ex)
                    {

                    }
                    Holder.LoadingIndicator.Visibility = ViewStates.Gone;

                    Holder.MediaLayer.Visibility = ViewStates.Visible;
                    Holder.Body.Visibility = ViewStates.Gone;
                    break;
            }

            if (Holder.Time != null)
                Holder.Time.Text = item.TimeDisplay;
        }
        private async void OnClickMedia(object sender, EventArgs e)
        {
            View view = sender as View;
            var Holder = view.Tag as MessageHolder;
            Holder.LoadingIndicator.Visibility = ViewStates.Visible;
            if (Holder.message.Picture.IsAvailableOffline)
            {
                await Holder.message.Picture.LoadPictureAsync(false);
                ChadderApp.UIHelper.HideKeyboard();
                (Context as MainActivity).SupportFragmentManager.BeginTransaction()
                       .Replace(Resource.Id.content_frame, MediaViewerFragment.Create(Holder.message.Picture))
                       .AddToBackStack(null)
                       .Commit();
            }
            else
                await Holder.message.Picture.LoadPictureAsync(true);
            Holder.LoadingIndicator.Visibility = ViewStates.Gone;
        }
        void OnClickProfilePic(object sender, EventArgs e)
        {
            View view = sender as View;
            var extra = (ExtraInfo<string>)view.Tag;
            var contactId = extra.Info;

            (Context as MainActivity).SupportFragmentManager.BeginTransaction()
                   .Replace(Resource.Id.content_frame, ViewProfileFragment.ViewProfile(contactId))
                   .AddToBackStack(null)
                   .Commit();
        }


    }

    internal class MessageHolder : Java.Lang.Object
    {
        public ImageViewEx ProfilePic;
        public TextView Body;
        public TextView Time;
        public TextView Name;
        public ImageView Status;
        public ImageView Media;
        public RelativeLayout MediaLayer;
        public ProgressBar LoadingIndicator;

        public ChadderMessage message;
    }
}