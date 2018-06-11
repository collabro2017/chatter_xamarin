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
using Xamarin.Facebook;
using Xamarin.Facebook.Share.Widget;
using Chadder.Data.Util;

namespace Chadder.Droid.Views
{
    public class RegisterEndFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.register_end_fragment, container, false);
            this.Activity.HideKeyboard();
            var start = view.FindViewById<Button>(Resource.Id.register_start);
            start.Click += delegate
            {
                Activity.SupportFragmentManager
                    .BeginTransaction()
                    .Replace(Resource.Id.content_frame, new ConversationListFragment())
                    .Commit();

            };

            var facebook = view.FindViewById<ImageButton>(Resource.Id.register_facebook);
            facebook.Click += delegate
            {
                //++ShareFacebook(Activity, uiHelper);
                ShareFacebook(Activity);
            };

            return view;
        }

        //++public static void ShareFacebook(Activity Activity, UiLifecycleHelper uiHelper)
        public static void ShareFacebook(Activity Activity)
        {
            try
            {
                //if (FacebookDialog.CanPresentShareDialog(Activity.ApplicationContext, FacebookDialog.ShareDialogFeature.ShareDialog))
                //{
                //    FacebookDialog shareDialog = new FacebookDialog.ShareDialogBuilder(Activity)
                //    .SetLink(Activity.GetString(Resource.String.ProfileFacebookShareLink))
                //    .SetName(Activity.GetString(Resource.String.ProfileFacebookShareName))
                //    .Build();
                //    uiHelper.TrackPendingDialogCall(shareDialog.Present());
                //}
                //else
                //{
                //    var param = new Bundle();
                //    param.PutString("name", Activity.GetString(Resource.String.ProfileFacebookShareName));
                //    param.PutString("link", Activity.GetString(Resource.String.ProfileFacebookShareLink));
                //    var feedDialog = new WebDialog.FeedDialogBuilder(Activity, Activity.GetString(Resource.String.app_id), param).Build(); ;
                //    feedDialog.Complete += delegate
                //    {
                //        feedDialog.Dismiss();
                //    };
                //    feedDialog.Show();
                //}
            }
            catch (Exception ex)
            {
                Insight.Report(ex);
                Toast.MakeText(Activity, "Unable to share on facebook", ToastLength.Long).Show();
            }
        }
    }
}