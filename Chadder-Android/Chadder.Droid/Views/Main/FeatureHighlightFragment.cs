using System;

using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text;
using System.Collections.ObjectModel;
using Chadder.Droid.Adapters;
using Android.Util;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using System.Threading.Tasks;
using Android.Support.V4.View;
using System.Collections.Generic;
using Chadder.Droid.Views.Account;
using Android.Content.PM;
using Xamarin.Facebook;
using ViewPagerIndicator;
namespace Chadder.Droid.Views
{

    public class FeatureHighlightFragment : BaseFragment
    {
        private static string EXTRA_TITLE = "EXTRA_TITLE";
        private static string EXTRA_MESSAGE = "EXTRA_MESSAGE";
        private static string EXTRA_IMAGE = "EXTRA_IMAGE";
        public override bool HidesActionBar { get { return true; } }
        private int m_iTitle, m_iMessage, m_iImage;

        public static FeatureHighlightFragment newInstance(int title, int message, int image)
        {
            var f = new FeatureHighlightFragment();
            Bundle bdl = new Bundle(2);
            bdl.PutInt(EXTRA_TITLE, title);
            bdl.PutInt(EXTRA_MESSAGE, message);
            bdl.PutInt(EXTRA_IMAGE, image);
            f.Arguments = bdl;
            return f;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            m_iTitle = Arguments.GetInt(EXTRA_TITLE);
            m_iMessage = Arguments.GetInt(EXTRA_MESSAGE);
            m_iImage = Arguments.GetInt(EXTRA_IMAGE);
        }
        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            HasOptionsMenu = true; // It must be true in order to toggleAction call onItemsSelected()
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.feature_highlight_fragment, container, false);

            var title = view.FindViewById<TextView>(Resource.Id.featureTitle);
            title.SetText(m_iTitle);

            var content = view.FindViewById<TextView>(Resource.Id.featureContent);
            content.SetText(m_iMessage);

            var image = view.FindViewById<ImageView>(Resource.Id.featureImage);
            image.SetImageResource(m_iImage);

            return view;
        }
    }
    public class MainFeatureHighlightFragment : BaseFragment
    {
        private ViewPager _pager;
        public override bool HidesActionBar { get { return true; } }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.main_feature_highlight_fragment, container, false);

            _pager = view.FindViewById<ViewPager>(Resource.Id.pager);
            var adapter = new GenericFragmentPagerAdaptor(ChildFragmentManager);

            adapter.AddFragment(FeatureHighlightFragment.newInstance(Resource.String.TourChadderTitle, Resource.String.TourChadderMessage, Resource.Drawable.tour_chadder));
            adapter.AddFragment(FeatureHighlightFragment.newInstance(Resource.String.TourEraseTitle, Resource.String.TourEraseMessage, Resource.Drawable.tour_erase));
            adapter.AddFragment(FeatureHighlightFragment.newInstance(Resource.String.TourDevicesTitle, Resource.String.TourDevicesMessage, Resource.Drawable.tour_devices));

            _pager.Adapter = adapter;

            var mIndicator = view.FindViewById<CirclePageIndicator>(Resource.Id.indicator);
            mIndicator.SetViewPager(_pager);

            var btn = view.FindViewById<Button>(Resource.Id.login);
            btn.Click += (object sender, EventArgs e) =>
            {
                var fragment = new LoginFragment();
                var transaction = Activity.SupportFragmentManager.BeginTransaction();
                transaction.SetCustomAnimations(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight, Resource.Animation.SlideInRight, Resource.Animation.SlideOutLeft);
                transaction.Replace(Resource.Id.content_frame, fragment);
                transaction.AddToBackStack(null);
                transaction.Commit();
            };

            btn = view.FindViewById<Button>(Resource.Id.signup);
            btn.Click += (object sender, EventArgs e) =>
            {
                var fragment = new RegisterFragment();
                var transaction = Activity.SupportFragmentManager.BeginTransaction();
                transaction.SetCustomAnimations(Resource.Animation.SlideInRight, Resource.Animation.SlideOutLeft, Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                transaction.Replace(Resource.Id.content_frame, fragment);
                transaction.AddToBackStack(null);
                transaction.Commit();
            };

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

        }
    }
}