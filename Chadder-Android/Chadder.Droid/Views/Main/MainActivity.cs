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
using Chadder.Droid.Views.Main;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Chadder.Droid.Util;
using Chadder.Droid.Views.Account;
using Android.Content.PM;
using Chadder.Droid.Views.Contacts;

namespace Chadder.Droid.Views
{
    [Activity(Label = "@string/app_name", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenSize, WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class MainActivity : BaseActionBarActivity
    {
        DrawerLayout mDrawerLayout;
        Android.Support.V4.App.Fragment fragmentNavigater = null;
        Android.Support.V7.App.ActionBarDrawerToggle mDrawerToggle;
        public void SetDrawerEnabled(bool drawer)
        {
            if (drawer)
            {
                mDrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                mDrawerToggle.DrawerIndicatorEnabled = true;
            }
            else
            {
                mDrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
                mDrawerToggle.DrawerIndicatorEnabled = false;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            this.HideKeyboard();

            SetContentView(Resource.Layout.main_activity);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.myDrawer);


            mDrawerToggle = new Android.Support.V7.App.ActionBarDrawerToggle(this, mDrawerLayout, Resource.String.open_drawer, Resource.String.close_drawer);
            mDrawerToggle.DrawerIndicatorEnabled = true;

            mDrawerLayout.SetDrawerListener(mDrawerToggle);
            mDrawerToggle.SyncState();

            if (ChadderApp.UIHelper.Source.IsOnline == false)
            {
                ChadderApp.UIHelper.Source_OnNotAuthorized();
            }
            else
            {
                if (fragmentNavigater == null)
                {
                    fragmentNavigater = new ProfileFragment();
                    SupportFragmentManager.BeginTransaction().Replace(Resource.Id.navigation_drawer, fragmentNavigater).Commit();
                }
                if (this.Intent.GetBooleanExtra("new_user", false))
                {
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.content_frame, new RegisterNameFragment())
                        .Commit();
                }
                else if (ChadderApp.UIHelper.Source.HasUserKey == false)
                {
                    var newFragment = new NewKeySplashScreenFragment();
                    SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, newFragment)
                    .Commit();
                }
                else
                {

                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.content_frame, new ConversationListFragment())
                        .Commit();

                }
            }
        }


        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);
            mDrawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            mDrawerToggle.OnConfigurationChanged(newConfig);
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (mDrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                this.HideKeyboard();
            }
            return base.OnOptionsItemSelected(item);
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }

        public static async void ScanFingerprint(MainActivity context)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner(context);
            var scanResult = await scanner.Scan();
            if (scanResult != null)
            {
                var result = await ChadderApp.UIHelper.ScanFingerprint(scanResult.Text);
                if (result != null)
                {
                    context.SupportFragmentManager.BeginTransaction()
                           .Replace(Resource.Id.content_frame, ViewProfileFragment.ViewProfile(result))
                           .AddToBackStack(null)
                           .CommitAllowingStateLoss();
                }
            }
        }
    }
}