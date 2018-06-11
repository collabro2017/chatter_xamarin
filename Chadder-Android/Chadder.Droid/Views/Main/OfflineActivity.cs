using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using System.Threading.Tasks;
using Android.Content.PM;

namespace Chadder.Droid.Views.Main
{
    [Android.App.Activity(Label = "@string/app_name", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenSize, MainLauncher = true, Theme = "@style/Theme.NoTitleBar", WindowSoftInputMode = SoftInput.StateHidden)]
    public class OfflineActivity : BaseActionBarActivity
    {
        public void GoToMain()
        {
            Intent i = new Intent(this, typeof(MainActivity));
            i.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            this.StartActivity(i);
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.offline_activity);
            SupportActionBar.Hide();

            if (ChadderApp.UIHelper.Source.IsOnline == false)
            {
                SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.content_frame, new SplashScreenFragment())
                    .Commit();
            }
            else
            {
                GoToMain();
            }
        }
    }
}