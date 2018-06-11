using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using ChadderLib.Util;
using Chadder.Droid.Util;
using Android.Graphics;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Android.Content.PM;
using Chadder.Droid.Views.Main;
using Chadder.Droid.Adapters;
using Chadder.Droid.Views.Media;
using Chadder.Data.Util;
using Chadder.Client.Data;

namespace Chadder.Droid.Views.Account
{
    public class MyGridView : GridView
    {
        public MyGridView(Context c)
            : base(c) { }
        public MyGridView(Context c, IAttributeSet a)
            : base(c, a) { }
        public MyGridView(Context c, IAttributeSet a, int d)
            : base(c, a, d) { }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (LayoutParameters.Height == LayoutParams.WrapContent)
            {
                var heightSpec = MeasureSpec.MakeMeasureSpec(9999, MeasureSpecMode.AtMost);
                base.OnMeasure(widthMeasureSpec, heightSpec);
            }
            else
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
    public class ProfileFragment : BaseFragment
    {
        private ImageButton _btnPicture;
        private ContextMenuManager<ChadderUserDevice> _menuManager;

        private TextView _tvName;
        private TextView _tvEmail;
        private TextView _tvPhone;
        private TextView _tvShareName;
        private TextView _tvContactBook;

        private Button _btnFingerprint;
        private DevicesAdapter _adapter;
        private GridView grid;
        public override bool IsDrawerFragment { get { return true; } }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.profile_fragment, container, false);

            grid = view.FindViewById<GridView>(Resource.Id.profile_devices);

            _adapter = new DevicesAdapter(Activity, ChadderUI.Source.db.LocalUser.Devices);

            grid.Adapter = _adapter;
            grid.ItemClick += grid_ItemClick;
            SetupMenu();

            _btnPicture = view.FindViewById<ImageButton>(Resource.Id.profile_picture);
            _btnPicture.Click += (sender, e) =>
            {
                ChadderImagePicker.MultiSourcePick(this);
            };

            var btnName = view.FindViewById<Button>(Resource.Id.profile_name_btn);
            _tvName = view.FindViewById<TextView>(Resource.Id.profile_name_label);

            btnName.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangeName();

            var btnEmail = view.FindViewById<Button>(Resource.Id.profile_email_btn);
            _tvEmail = view.FindViewById<TextView>(Resource.Id.profile_email_label);

            btnEmail.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangeEmail();

            var btnPhone = view.FindViewById<Button>(Resource.Id.profile_phone_btn);
            _tvPhone = view.FindViewById<TextView>(Resource.Id.profile_phone_label);
            btnPhone.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangePhone();

            var btnShareName = view.FindViewById<Button>(Resource.Id.profile_sharename_btn);
            _tvShareName = view.FindViewById<TextView>(Resource.Id.profile_sharename_label);

            btnShareName.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangeShareName();

            var btnFacebook = view.FindViewById<Button>(Resource.Id.profile_facebook_btn);
            btnFacebook.Click += (object sender, EventArgs e) =>
                //++RegisterEndFragment.ShareFacebook(Activity, uiHelper);
                RegisterEndFragment.ShareFacebook(Activity);

            var btnDevicePassword = view.FindViewById<Button>(Resource.Id.profile_device_pwd_btn);

            btnDevicePassword.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangeDevicePassword();

            var btnAccPassword = view.FindViewById<Button>(Resource.Id.profile_account_pwd_btn);

            btnAccPassword.Click += async (object sender, EventArgs e) =>
                await ChadderUI.ChangeAccountPassword();

            var btnLogout = view.FindViewById<Button>(Resource.Id.profile_logout_btn);

            btnLogout.Click += async (object sender, EventArgs e) =>
            {
                await ChadderUI.Logout();
                ChadderApp.UIHelper.Source_OnNotAuthorized();
            };

            view.FindViewById<Button>(Resource.Id.profile_scan_btn)
                .Click += (object sender, EventArgs e) => MainActivity.ScanFingerprint(Activity as MainActivity);

            _btnFingerprint = view.FindViewById<Button>(Resource.Id.profile_fingerprint_btn);
            _btnFingerprint.Click += async (object sender, EventArgs e) =>
            {
                var layout = new RelativeLayout(Activity);

                var image = new ImageView(Activity);
                image.Focusable = true;
                image.FocusableInTouchMode = true;
                image.SetScaleType(ImageView.ScaleType.FitCenter);
                image.SetBackgroundResource(Android.Resource.Color.White);
                layout.AddView(image, new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent));

                var bottomText = new TextView(Activity);
                var param = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                param.SetMargins(20, 20, 20, 20);
                param.AddRule(LayoutRules.CenterHorizontal);
                param.AddRule(LayoutRules.AlignParentBottom);
                layout.AddView(bottomText, param);
                bottomText.Text = GetString(Resource.String.ProfileFingerprintBottomMessage);

                InputDialog.ShowProgressDialog(Activity);
                var writer = new ZXing.BarcodeWriter { Format = ZXing.BarcodeFormat.QR_CODE };
                var qr = writer.Write(await ChadderUI.GetFingerprint());
                image.SetImageBitmap(qr);
                InputDialog.DismissProgressDialog();

                var wnd = new PopupWindow(layout, RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                wnd.ShowAtLocation(View, GravityFlags.Center, 0, 0);
                image.Touch += (object s, Android.Views.View.TouchEventArgs e2) =>
                {
                    wnd.Dismiss();
                };
            };

            view.FindViewById<Button>(Resource.Id.Erase_All_Msgs_btn)
                .Click += async (object sender, EventArgs e) => await ChadderUI.DeleteAllMessages();

            view.FindViewById<Button>(Resource.Id.profile_contactbook_btn)
                //++.Click += async (s, e) => await ChadderUI.ToggleUploadContactBook();
                .Click += (s, e) => { };
            _tvContactBook = view.FindViewById<TextView>(Resource.Id.profile_contactbook_label);

            return view;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            EventHandlers.Add(ChadderUI.Source.db.LocalUser, (s, e) => InvalidateData());
        }
        void grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            e.View.ShowContextMenu();
        }



        private void SetupMenu()
        {
            _menuManager = new ContextMenuManager<ChadderUserDevice>(this, grid);

            _menuManager.InsertItem("Rename", (ChadderUserDevice device) =>
            {
                ChadderUI.ChangeDeviceName(device);
            });


            _menuManager.InsertItem(GetString(Resource.String.DevicePair), (ChadderUserDevice device) =>
            {
                ChadderUI.PairDevice(device);
            }, (d) => ChadderUI.Source.HasUserKey && d.HasUserKey == false);

            _menuManager.InsertItem("Delete", (ChadderUserDevice device) =>
            {
                ChadderUI.DeleteDevice(device);
            }, (d) => d.CurrentDevice == false);
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

        public override void OnResume()
        {
            base.OnResume();
            InvalidateData();
        }

        protected override void InvalidateData()
        {
            base.InvalidateData();
            try
            {
                var profile = ChadderUI.Source.db.LocalUser;
                _tvName.Text = profile.Name;
                _tvEmail.Text = profile.Email;
                _tvPhone.Text = profile.Phone;
                //++_tvContactBook.Text = ChadderUI.DisplayContactBook();

                _btnPicture.SetImageDrawable(new CircleDrawable(profile.Picture.Image));

                if (profile.IsNamePublic)
                    _tvShareName.Text = "ProfilePublicNameMessageOn".t();
                else
                    _tvShareName.Text = "ProfilePublicNameMessageOff".t();

                if (profile.HasUserKey == false)
                    _btnFingerprint.Visibility = ViewStates.Gone;
                else
                    _btnFingerprint.Visibility = ViewStates.Visible;
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
            }

        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Bitmap result = ChadderImagePicker.OnPickImageResult(requestCode, resultCode, data, Activity);
            if (result != null)
            {

                result = await CropImageFragment.CropImage(Activity as BaseActionBarActivity, result);

                await ChadderUI.ChangePicture(result.CompressToJPEG());
            }
        }



    }
}