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
using Gcm.Client;
using Android.Util;

using Chadder.Droid.Views;
using ChadderLib.Util;
using Chadder.Droid.Views.Main;
using Android.Content.PM;
using Chadder.Data.Util;

namespace Chadder.Droid
{
    [BroadcastReceiver()]
    [IntentFilter(new[] { "android.net.conn.CONNECTIVITY_CHANGE" })]
    class ConnectionChanged : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            bool noConnection = intent.GetBooleanExtra(Android.Net.ConnectivityManager.ExtraNoConnectivity, false);

            try
            {
                ChadderApp.UIHelper.OnConnectionStateChanged(noConnection == false);
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
            }
        }
    }
    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE, Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted })]
    class GcmBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        public static string[] SENDER_IDS = new string[] { "268002216691" };
    }

    [Service]
    public class PushHandlerService : GcmServiceBase
    {
        public PushHandlerService() : base(GcmBroadcastReceiver.SENDER_IDS) { }

        protected override void OnMessage(Context context, Intent intent)
        {
            if (intent != null && intent.Extras != null)
            {
                Insight.Track("GCM Message");

                ChadderApp.ProcessNotification(intent.Extras, context);
            }
        }

        protected override void OnError(Context context, string errorId)
        {
            Insight.Track("GCM Error: " + errorId);
            if (ChadderApp.UIHelper != null)
                ChadderApp.UIHelper.SetNotificationHandle(null);
        }

        protected override async void OnRegistered(Context context, string registrationId)
        {
            Insight.Track("GCM Registered");
            PackageInfo pInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            var shared = context.GetSharedPreferences();
            var editor = shared.Edit();
            editor.PutString(context.GetString(Resource.String.shared_preferences_gcm_handle), registrationId);
            editor.PutInt(context.GetString(Resource.String.shared_preferences_version), pInfo.VersionCode);
            editor.Commit();

            if (ChadderApp.UIHelper != null)
                ChadderApp.UIHelper.SetNotificationHandle(registrationId);
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Insight.Track("GCM Unregistered");
            if (ChadderApp.UIHelper != null)
                ChadderApp.UIHelper.SetNotificationHandle(null);
        }
    }

    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    class BootCompletedBroadcastMessageReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                var shared = context.GetSharedPreferences();
                var editor = shared.Edit();
                editor.Remove(context.GetString(Resource.String.shared_preferences_gcm_handle));
                editor.Commit();
                GcmClient.Register(context, GcmBroadcastReceiver.SENDER_IDS);
            }
        }
    }

    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionMyPackageReplaced })]
    class MyPackageReplacedBroadcastMessageReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionMyPackageReplaced)
            {
                var shared = context.GetSharedPreferences();
                var editor = shared.Edit();
                editor.Remove(context.GetString(Resource.String.shared_preferences_gcm_handle));
                editor.Commit();
                GcmClient.Register(context, GcmBroadcastReceiver.SENDER_IDS);
            }
        }
    }
}