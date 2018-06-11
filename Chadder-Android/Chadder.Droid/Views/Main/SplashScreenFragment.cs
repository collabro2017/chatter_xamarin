using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Gcm.Client;
using Android.Gms.Common;
using System.Threading.Tasks;
using System.Threading;
using Android.Net;
using Chadder.Droid.Util;
using Chadder.Droid.Views.Account;
using Android.Support.V4.App;
using Chadder.Data.Util;

namespace Chadder.Droid.Views.Main
{
    public class SplashScreenFragment : BaseFragment
    {

        public override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GCMInit();
            var status = await ChadderUI.SplashScreenLoad();
            // Create your fragment here
            if (status)
            {
                (Activity as OfflineActivity).GoToMain();
            }
            else
            {
                var newFragment = new MainFeatureHighlightFragment();
                FragmentTransaction transaction = Activity.SupportFragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.content_frame, newFragment);
                transaction.Commit();
            }
        }
        protected void GCMInit()
        {
            try
            {
                PackageInfo pInfo = this.Activity.PackageManager.GetPackageInfo(this.Activity.PackageName, 0);
                var shared = this.Activity.GetSharedPreferences();
                var version = shared.GetInt(GetString(Resource.String.shared_preferences_version), 0);
                var temp = shared.GetString(GetString(Resource.String.shared_preferences_gcm_handle), null);
                if (temp != null)
                {
                    GcmClient.UnRegister(Activity);
                }
                // If already registered for GCM, or if not connected to the internet proceed to login
                if (isNetworkAvailable())
                {
                    // Check for Google GCM
                    int errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this.Activity);
                    if (errorCode == ConnectionResult.Success)
                    {
                        GcmClient.Register(this.Activity, GcmBroadcastReceiver.SENDER_IDS);
                        Insight.Track("GcmClient.Register");
                    }
                    else
                    {
                        const int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;
                        Android.App.Dialog dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this.Activity, PLAY_SERVICES_RESOLUTION_REQUEST);
                        dialog.DismissEvent += delegate
                        {
                            this.Activity.Finish();
                        };

                        dialog.Show();
                    }
                }

            }
            catch (Exception e)
            {
                Insight.Report(e);
            }

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.splashscreen_fragment, container, false);
            return view;
        }
        private bool isNetworkAvailable()
        {
            var connectivityManager
                  = (ConnectivityManager)this.Activity.GetSystemService(Context.ConnectivityService);
            NetworkInfo activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            return activeNetworkInfo != null && activeNetworkInfo.IsConnected;
        }
    }
}