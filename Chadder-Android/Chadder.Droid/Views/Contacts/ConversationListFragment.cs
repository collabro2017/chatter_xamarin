using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Chadder.Droid.Adapters;
using Android.Support.V4.View;
using Chadder.Droid.Views.Main;
using Chadder.Droid.Util;
using Chadder.Droid.Views.Contacts;
using Chadder.Client.Util;

namespace Chadder.Droid.Views
{
    public class ConversationListFragment : BaseFragment
    {
        ConversationsAdapter _adapter;

        static readonly int FRAGMENT_GROUP_ID = 0x10;
        static readonly int MENU_CONVERSATION_HIDE = 0x0;
        static readonly int MENU_DELETE_ALL = 0x1;
        static readonly int MENU_TAKEBACK_ALL = 0x2;

        public override bool DrawerEnabled { get { return true; } }



        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_conversation_list, container, false);

            var listView = view.FindViewById<ListView>(Resource.Id.conversation_list);

            var items = new ConversationsFiltered(ChadderUI.Source.db.Conversations);

            _adapter = new ConversationsAdapter(
                this.Activity,
                Resource.Layout.item_conversation,
                items);

            var empty = view.FindViewById<TextView>(Resource.Id.no_chat_textbox);
            empty.Text = "To start a Chat you can click on the upper-right corner button and select a contact.";
            _adapter.EmptyItem = empty;

            listView.ItemClick += OnListItemClick;

            listView.Adapter = _adapter;

            HasOptionsMenu = true;

            RegisterForContextMenu(listView);
            return view;
        }
        public override void OnPause()
        {
            base.OnPause();
            _adapter.OnPause();
        }
        public override void OnResume()
        {
            base.OnResume();
            _adapter.OnResume();
        }



        protected void OnListItemClick(object sender, AdapterView.ItemClickEventArgs args)
        {
            SupportFragmentManager.BeginTransaction()
                            .Replace(Resource.Id.content_frame, ChatFragment.OpenChat(_adapter[args.Position]))
                            .AddToBackStack(null)
                            .Commit();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.action_bar, menu);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            var conversation = _adapter[info.Position];

            menu.SetHeaderTitle(conversation.DisplayName);
            menu.Add(FRAGMENT_GROUP_ID, MENU_CONVERSATION_HIDE, MENU_CONVERSATION_HIDE, "ConversationHide".t());
            menu.Add(FRAGMENT_GROUP_ID, MENU_DELETE_ALL, MENU_DELETE_ALL, "ConversationClearAllMessages".t());
            menu.Add(FRAGMENT_GROUP_ID, MENU_TAKEBACK_ALL, MENU_TAKEBACK_ALL, "ConversationTakeBackAllMessages".t());
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.addPeople)
            {
                var newFragment = new ContactListFragment();
                Android.Support.V4.App.FragmentTransaction transaction = Activity.SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.content_frame, newFragment);
                transaction.AddToBackStack(null);
                transaction.Commit();
            }
            return base.OnOptionsItemSelected(item);
        }

        public async void OnContextItemSelectedAsync(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var conversation = _adapter[info.Position];

            if (item.ItemId == MENU_CONVERSATION_HIDE)
                await ChadderUI.SetHidden(conversation);
            else if (item.ItemId == MENU_DELETE_ALL)
                await ChadderUI.DeleteAllMessages(conversation);
            else if (item.ItemId == MENU_TAKEBACK_ALL)
                await ChadderUI.TakeAllBack(conversation);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            if (item.GroupId != FRAGMENT_GROUP_ID)
                return base.OnContextItemSelected(item);

            OnContextItemSelectedAsync(item);
            return true;
        }


    }
}