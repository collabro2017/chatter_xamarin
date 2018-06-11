using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;


using Chadder.Droid.Views;
using Android.Support.V7.App;
using Chadder.Droid.Views.Contacts;
using System.ComponentModel;
using Xamarin.Facebook;
using Chadder.Data.Util;

namespace Chadder.Droid.Views
{
    public abstract class BaseFragment : Fragment
    {
        //++protected UiLifecycleHelper uiHelper;
        public virtual bool CustomActionBar { get { return true; } }
        public virtual bool HidesActionBar { get { return false; } }
        public virtual bool IsDrawerFragment { get { return false; } }
        public virtual bool DrawerEnabled { get { return false; } }

        protected Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> EventHandlers = new Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler>();
        public override void OnResume()
        {
            base.OnResume();
            try
            {
                //++uiHelper.OnResume();
            }
            catch { }
            if (Activity is MainActivity && IsDrawerFragment == false)
            {
                (Activity as MainActivity).SetDrawerEnabled(DrawerEnabled);
                if (DrawerEnabled == false)
                    (Activity as MainActivity).SupportActionBar.SetDisplayShowHomeEnabled(Activity.FragmentManager.BackStackEntryCount > 0);
            }
            if (HidesActionBar)
                (Activity as AppCompatActivity).SupportActionBar.Hide();
            else
                (Activity as AppCompatActivity).SupportActionBar.Show();

            foreach (var pair in EventHandlers)
            {
                pair.Key.PropertyChanged += pair.Value;
            }

            InvalidateData();
        }

        public override void OnPause()
        {
            base.OnPause();
            try
            {
                //++uiHelper.OnPause();
            }
            catch { }
            if (CustomActionBar == false)
                (Activity as AppCompatActivity).SupportActionBar.Title = "Chadder";

            foreach (var pair in EventHandlers)
            {
                pair.Key.PropertyChanged -= pair.Value;
            }
        }


        protected virtual void InvalidateData() { }

        public void DisplayToast(string error)
        {
            Toast.MakeText(Activity, error, ToastLength.Long).Show();
        }

        public FragmentManager SupportFragmentManager
        {
            get
            {
                return (Activity as Android.Support.V4.App.FragmentActivity).SupportFragmentManager;
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            try
            {
                //++uiHelper = new UiLifecycleHelper(Activity, null);
                //++uiHelper.OnCreate(savedInstanceState);
            }
            catch { }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            try
            {
                //++uiHelper.OnSaveInstanceState(outState);
            }
            catch { }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                //++uiHelper.OnDestroy();
            }
            catch { }
        }
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            try
            {
                //++uiHelper.OnActivityResult(requestCode, resultCode, data);
            }
            catch { }
        }
        public ChadderUIHelper ChadderUI
        {
            get
            {
                ChadderApp.UIHelper.ActivityContext = Activity;
                return ChadderApp.UIHelper;
            }
        }
    }
}