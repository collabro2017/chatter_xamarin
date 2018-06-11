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
using Chadder.Client.Data;

namespace Chadder.Droid.Views.Media
{
    public class GhostPictureViewFragment : BaseFragment
    {
        public ChadderMessage _msg;

        private ImageView _imageView;
        private TextView _timeLeft;
        public override bool HidesActionBar { get { return true; } }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var view = inflater.Inflate(Resource.Layout.ghost_view_activity, container, false);

            view.FindViewById<RelativeLayout>(Resource.Id.view_ghost_layout).Click += delegate
            {
                SupportFragmentManager.PopBackStack();
            };

            _imageView = view.FindViewById<ImageView>(Resource.Id.ghost_viewer_image);
            _timeLeft = view.FindViewById<TextView>(Resource.Id.ghost_timeleft);

            _timeLeft.AddShadow();
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            _msg.PropertyChanged += Message_PropertyChanged;

            if (_msg.MyMessage == false && _msg.TimeRead == null)
                _msg.TimeRead = DateTime.UtcNow;
            InvalidateData();
        }

        public override void OnPause()
        {
            base.OnPause();
            _msg.PropertyChanged -= Message_PropertyChanged;
        }

        private void Message_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "gone")
            {
                _msg.PropertyChanged -= Message_PropertyChanged;
                SupportFragmentManager.PopBackStack();
            }
        }
    }
}