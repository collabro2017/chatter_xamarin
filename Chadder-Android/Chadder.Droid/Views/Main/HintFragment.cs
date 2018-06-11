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
using System.Threading.Tasks;

namespace Chadder.Droid.Views.Main
{
    public class HintFragment : BaseFragment
    {
        private View TooltipFor;
        private string Text;
        private int time;
        public HintFragment(View v, string Text, int time = 4000)
        {
            TooltipFor = v;
            this.Text = Text;
            this.time = time;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = new RelativeLayout(Activity);
            view.LayoutParameters = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            var txt = new TextView(Activity);
            txt.SetBackgroundResource(Resource.Drawable.hint_border);
            txt.Text = Text;
            var dp = Resources.DisplayMetrics.Density;
            txt.SetPadding((int)dp * 5, (int)dp * 5, (int)dp * 5, (int)dp * 5);
            var param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            var rect = new Android.Graphics.Rect();
            TooltipFor.GetGlobalVisibleRect(rect);
            var height = Resources.DisplayMetrics.HeightPixels;
            var above = rect.Top > height - rect.Bottom;

            var containerRect = new Android.Graphics.Rect();
            Activity.FindViewById(Resource.Id.content_frame).GetGlobalVisibleRect(containerRect);
            param.AddRule(LayoutRules.AlignParentRight);
            if (above)
            {
                param.AddRule(LayoutRules.AlignParentBottom);
                param.BottomMargin = containerRect.Bottom - rect.Top;
            }
            else
            {
                param.AddRule(LayoutRules.AlignParentTop);
                param.TopMargin = rect.Bottom - containerRect.Top;
            }

            view.AddView(txt, param);
            view.Click += (s, e) => Hide();
            Timer();
            return view;
        }

        private async void Timer()
        {
            await Task.Delay(time);
            Hide();
        }

        private bool Hidden = false;
        private void Hide()
        {
            if (Hidden)
                return;
            Hidden = true;
            SupportFragmentManager.PopBackStack();
        }
    }
}