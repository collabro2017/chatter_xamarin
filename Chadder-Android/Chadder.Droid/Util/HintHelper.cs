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
using System.Threading.Tasks;
using Chadder.Droid.Views;

namespace Chadder.Droid.Util
{
    public class HintHelper
    {
        public static string Hint_TakeBack = "HINT_TAKEBACK";

        /// <summary>
        /// Displays a hint if it hasn't been displayed before. Time is i x 3s
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hint"></param>
        /// <param name="txt"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="time"></param>
        public static bool DisplayHintIfNeverDisplayed(BaseFragment fragment, string hint, string txt, View v, int time)
        {
            var shared = fragment.Activity.GetSharedPreferences();
            if (shared.Contains(hint) == false)
            {
                var edit = shared.Edit();
                edit.PutInt(hint, 1);
                edit.Apply();
                fragment.SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.content_frame, new Chadder.Droid.Views.Main.HintFragment(v, txt))
                    .AddToBackStack(null).Commit();
                return true;
            }
            return false;
        }
    }
}