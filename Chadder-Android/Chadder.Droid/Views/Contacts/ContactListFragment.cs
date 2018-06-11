using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Chadder.Droid.Adapters;
using Android.Widget;
using Android.App;

using Chadder.Droid.Util;
using System.Collections.ObjectModel;
using ChadderLib.Util;
using Chadder.Droid.Views.Contacts;
using System.Threading.Tasks;
using Android.Text;
using Chadder.Client.Data;
using Chadder.Data.Util;



namespace Chadder.Droid.Views
{
    public class ContactListFragment : BaseFragment
    {
        Android.Widget.ListView _contactListView;
        EditText searchText;
        TextView _noEntryText;
        FindFriendAdapter _adapter;

        private ContextMenuManager<ChadderContact> _menuManager;

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            HasOptionsMenu = true; // It must be true in order to toggleAction call onItemsSelected()
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_contact_list, container, false);
            try
            {
                _contactListView = view.FindViewById<Android.Widget.ListView>(Resource.Id.contact_list);

                searchText = view.FindViewById<EditText>(Resource.Id.contact_list_searchview);

                _noEntryText = view.FindViewById<TextView>(Resource.Id.no_friends_textview);
                _noEntryText.TextFormatted = Html.FromHtml("To add friends you can type their name in the search bar and if they have enabled </br>\"Public Name\" they will appear here");

                searchText.TextChanged += (s, e) =>
                {
                    _adapter.Collection.SetSearch(searchText.Text);
                };

                _adapter = new FindFriendAdapter(this.Activity, _noEntryText, new UIHelper.FindContactObservableCollection(ChadderApp.UIHelper.Source));
                _contactListView.Adapter = _adapter;
                _contactListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
                {
                    var contact = _adapter[e.Position];
                    if (contact == null)
                        return;
                    if (contact.IsTemporary)
                    {
                        SupportFragmentManager.BeginTransaction()
                            .Replace(Resource.Id.content_frame, ViewProfileFragment.ViewProfile(contact))
                            .AddToBackStack(null)
                            .Commit();
                    }
                    else
                    {
                        SupportFragmentManager.PopBackStack();
                        SupportFragmentManager.BeginTransaction()
                            .Replace(Resource.Id.content_frame, ChatFragment.OpenChat(contact))
                            .AddToBackStack(null)
                            .Commit();
                    }
                };

                _menuManager = new ContextMenuManager<ChadderContact>(this, _contactListView);

                _menuManager.InsertItem("View profile", (ChadderContact contact) =>
                {
                    Android.Support.V4.App.FragmentTransaction transaction = this.Activity.SupportFragmentManager.BeginTransaction();
                    transaction.Replace(Resource.Id.content_frame, ViewProfileFragment.ViewProfile(contact));
                    transaction.AddToBackStack(null);
                    transaction.Commit();
                });

                _menuManager.InsertItem(c => c.Type == Chadder.Data.RelationshipType.BLOCKED ? "Unblock" : "Block",
                    (ChadderContact contact) => ChadderUI.ToggleBlock(contact));

                _menuManager.InsertItem("Report",
                    (ChadderContact contact) => ChadderUI.ReportContact(contact));

            }
            catch (Exception e)
            {
                Insight.Report(e);
            }

            return view;
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            var contact = _adapter[info.Position];
            _menuManager.CreateMenu(menu, contact);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            if (_menuManager.Selected(item) == false)
                return base.OnContextItemSelected(item);
            return true;
        }

    }

}