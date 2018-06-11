using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.App;
using Chadder.Droid.Util;
using Android.Text;
using Chadder.Data.Util;
using Chadder.Client.Data;

namespace Chadder.Droid.Views.Contacts
{
    public class ViewProfileFragment : BaseFragment
    {
        private static string EXTRA_CONTACT_ID = "EXTRA_CONTACT_ID";
        public static ChadderContact TempProfile;
        public ChadderContact Profile;
        public static bool Scanning = false;
        public TextView txtFingerprintStatus;
        ImageView imgProfile;
        TextView profileName, username, keyFingerprint;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var id = Arguments.GetString(EXTRA_CONTACT_ID, null);
            if (id == null)
            {
                Profile = TempProfile;
            }
            else
                Profile = ChadderUI.Source.db.Contacts.FirstOrDefault(i => i.UserId == id);
            if (Profile == null)
                SupportFragmentManager.PopBackStack();
        }
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            HasOptionsMenu = true; // It must be true in order to toggleAction call onItemsSelected()
        }

        static public ViewProfileFragment ViewProfile(string userId)
        {

            var result = new ViewProfileFragment();
            result.Arguments = new Bundle();
            result.Arguments.PutString(EXTRA_CONTACT_ID, userId);
            return result;
        }
        static public ViewProfileFragment ViewProfile(ChadderContact Profile)
        {
            if (Profile.IsTemporary)
            {
                TempProfile = Profile;
                return ViewProfile((string)null);
            }
            else
                return ViewProfile(Profile.UserId);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.contact_profile_fragment, container, false);

            Activity.HideKeyboard();
            imgProfile = view.FindViewById<ImageView>(Resource.Id.profile_picture);
            profileName = view.FindViewById<TextView>(Resource.Id.profile_name_label);
            username = view.FindViewById<TextView>(Resource.Id.profile_username_label);
            keyFingerprint = view.FindViewById<TextView>(Resource.Id.profile_fingerprint);
            var btnAdd = view.FindViewById<Button>(Resource.Id.profile_add_btn);
            var addLayer = view.FindViewById<LinearLayout>(Resource.Id.profile_add_layer);
            txtFingerprintStatus = view.FindViewById<TextView>(Resource.Id.profile_fingerprint_status);

            if (Profile != null)
            {
                Profile.PropertyChanged += Profile_PropertyChanged;

                if (Profile.IsTemporary == false)
                    addLayer.Visibility = ViewStates.Gone;
                else
                    btnAdd.Click += async delegate
                    {
                        var result = await ChadderUI.AddContact(Profile.UserId);
                        if (result)
                        {
                            SupportFragmentManager.PopBackStack();
                            SupportFragmentManager.PopBackStack();
                        }
                    };
            }
            return view;
        }

        protected override void InvalidateData()
        {
            base.InvalidateData();
            if (Profile == null)
                return;
            try
            {
                username.Text = Profile.Username;
                profileName.Text = Profile.Name;
                keyFingerprint.Text = Profile.PublicKeyBook.GetMaster().FingerprintEncodedClipped;
                imgProfile.SetImageDrawable(new CircleDrawable(Profile.Picture.Image));

                if (Profile.KeyStatus == Data.PublicKeyStatus.VERIFIED)
                    txtFingerprintStatus.Text = "Verified";
                else if (Profile.KeyStatus == Data.PublicKeyStatus.NOT_VERIFIED)
                    txtFingerprintStatus.Text = "Not Verified";
                else
                    txtFingerprintStatus.TextFormatted = Html.FromHtml("<b>Wrong</b>");


                Activity.InvalidateOptionsMenu();
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
            }
        }


        public override void OnResume()
        {
            base.OnResume();
            if (Profile != null)
                Profile.PropertyChanged += Profile_PropertyChanged;
        }

        public override void OnPause()
        {
            base.OnPause();
            if (Profile != null)
                Profile.PropertyChanged -= Profile_PropertyChanged;
        }

        private void Profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvalidateData();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.profile_key_status)
            {
                //++ChadderUI.KeyStatusWarning(Profile);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.contact_profile_menu, menu);
            var status = menu.FindItem(Resource.Id.profile_key_status);

            if (Profile.KeyStatus == Data.PublicKeyStatus.CHANGED)
                status.SetIcon(Resource.Drawable.key_invalid);
            else
                status.SetVisible(false);

            base.OnCreateOptionsMenu(menu, inflater);
        }
    }
}