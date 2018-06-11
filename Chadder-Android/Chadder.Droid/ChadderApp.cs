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
using Chadder.Droid.Views;

using Android.Support.V4.App;


namespace Chadder.Droid
{
    using TaskStackCompat = Android.Support.V4.App.TaskStackBuilder;
    using System.Threading.Tasks;
    using Chadder.Droid.Util;
    using Chadder.Droid.Views.Main;
    using Android.Content.PM;
    using Chadder.Data.Util;
    using Chadder.Client.Util;

    [Application(
        Theme = "@style/Theme.AppTheme",
#if DEBUG
 Debuggable = true
#else
 Debuggable = false
#endif
)]

    class ChadderApp : Application
    {

        public static readonly int NotificationID = 0x80;
        static DateTime lastNotification;
        static readonly long[] vibrationPattern = new long[] { 200, 200, 200 };

        public static ChadderUIHelperDroid UIHelper;

        public override void OnCreate()
        {
            base.OnCreate();
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            //++ChadderUIHelper.OnInitialize();
            Chadder.Client.Data.ChadderPicture.defaultImage = Android.Graphics.BitmapFactory.DecodeResource(Resources, ChadderLib.Droid.Resource.Drawable.ic_default_face);
            UIHelper = new ChadderUIHelperDroid();
            // TODO On User Not Authorized go back to splashscreen
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            Insight.Report(e.Exception);
        }
        public ChadderApp(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }


        public void OnUserNotAuthorized()
        {
            InputDialog.Popup(GetString(Resource.String.NotAuthorizedMessage), GetString(Resource.String.NOT_AUTHORIZED), () =>
            {
                Intent i = new Intent(this, typeof(OfflineActivity));
                i.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask | ActivityFlags.ClearTop);
                StartActivity(i);
            });
        }

        static public void ClearAllNotifications(Context context)
        {
            var service = (NotificationManager)context.GetSystemService(NotificationService);
            service.CancelAll(); // Clear previous notifications (avoid duplicates)
        }

        public static void GenerateNotification(string msg, string tag, Context context, Bundle extras = null)
        {
            lastNotification = DateTime.Now;

            Intent resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.AddFlags(ActivityFlags.NewTask);
            if (extras != null)
                resultIntent.PutExtras(extras);

            TaskStackCompat stackBuilder = TaskStackCompat.Create(context);

            var classType = Java.Lang.Class.FromType(typeof(MainActivity));
            stackBuilder.AddParentStack(classType);
            stackBuilder.AddNextIntent(resultIntent);

            PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);


            NotificationCompat.Builder mBuilder =
                new NotificationCompat.Builder(context)
                    .SetSmallIcon(Resource.Drawable.ic_launcher)
                    .SetContentTitle("Chadder")
                    .SetContentText(msg)
                    .SetLights(255, 255, 255)
                    .SetVibrate(vibrationPattern)
                    .SetContentIntent(resultPendingIntent)
                    .SetAutoCancel(true)
                    .SetSound(Android.Net.Uri.Parse("android.resource://" +
                context.PackageName +
                "/" +
                Resource.Raw.ChadderSound));

            var service = (NotificationManager)context.GetSystemService(NotificationService);

            service.CancelAll(); // Clear previous notifications (avoid duplicates)
            service.Notify(tag, NotificationID, mBuilder.Build());
        }

        public static void GenerateNewMessageNotification(long conversation, string user, Context context)
        {
            var sLastNotification = (long)(DateTime.Now - lastNotification).TotalSeconds;

            if (sLastNotification > 5 && ActivitiesManager.Instance.current == null)
            {
                lastNotification = DateTime.Now;
                Bundle extras = new Bundle();
                extras.PutLong("OpenChatId", conversation);

                GenerateNotification("You have new messages", "chatNotif" + conversation.ToString(), context, extras);
            }
        }


        public static void ProcessNotification(Bundle extra, Context context)
        {
            //++if (UIHelper != null)
            //++    UIHelper.OnNotificationReceived();
            long convId = -1;
            string user = null;
            string message = null;
            long last_update = 0;
            foreach (var key in extra.KeySet())
            {
                switch (key.ToLower())
                {
                    case "conversation":
                        convId = long.Parse((string)extra.Get(key));
                        break;
                    case "user":
                        user = extra.GetString(key);
                        break;
                    case "message":
                        message = extra.GetString(key);
                        break;
                    case "nupdate":
                        last_update = long.Parse((string)extra.Get(key));
                        break;
                }


            }
            if (last_update > 0)
            {
                var shared = ChadderApp.Context.GetSharedPreferences();
                var last = shared.GetLong(ChadderApp.Context.GetString(Resource.String.shared_preferences_last_update), 0);
                if (last > last_update)
                {
                    Insight.Track("Older push notification ignored");
                    return;
                }
            }
            if (convId != -1)
                GenerateNewMessageNotification(convId, user, context);
            if (message != null)
                GenerateNotification(message, "message", context);
        }
    }
}