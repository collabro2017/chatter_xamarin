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
using Android.Text;
using ChadderLib.Util;
using Chadder.Droid.Util;
using Chadder.Data.Util;

namespace Chadder.Droid.Views.Main
{
    public class NewKeySplashScreenFragment : BaseFragment
    {
        public override bool DrawerEnabled
        {
            get
            {
                return true;
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.no_key_splashscreen_fragment, container, false);

            var txt = view.FindViewById<TextView>(Resource.Id.NoKeyMessage);
            txt.TextFormatted = Html.FromHtml(string.Format("DeviceNoKeyMessage".t(), string.Format("<b>{0}</b>", "DeviceNoKeyMessageHighlighted".t())));

            view.Click += async (s, e) => await ChadderUI.CreateNewKey();

            EventHandlers.Add(ChadderUI.Source.db.LocalUser, profile_PropertyChanged);
            return view;
        }

        private void profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ChadderUI.Source.HasUserKey)
            {
                SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, new ConversationListFragment())
                .Commit();
            }
        }
    }
}