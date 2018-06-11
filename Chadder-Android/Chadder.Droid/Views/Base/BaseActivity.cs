using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.App;
using Android.Views;
using System.Threading.Tasks;
using ChadderLib.Util;
using System.ComponentModel;
using Android.Widget;
using Chadder.Data.Util;


namespace Chadder.Droid.Views
{
    public class ActivitiesManager
    {
        protected Activity _current;
        public Activity current
        {
            get
            {
                return _current;
            }
            set
            {

                if (value == null && _current != null)
                {
                    LastActivity = DateTime.Now;
                    if (DelayedVerificationTask == null)
                    {
                        DelayedVerificationTask = new Task(Verification);
                        DelayedVerificationTask.Start();
                    }
                }
                if (value != null)
                {
                    if (IsIdle)
                        BackFromIdle();
                    ChadderApp.ClearAllNotifications(value);
                }
                _current = value;
                Chadder.Client.Util.UIThread.Activity = value;
            }
        }

        private DateTime LastActivity { get; set; }
        private Task DelayedVerificationTask { get; set; }
        public bool IsIdle { get; private set; }

        private async void Verification()
        {
            do
            {
                await Task.Delay(1000);
                if (_current != null)
                {
                    DelayedVerificationTask = null;
                    return;
                }
            } while (DateTime.Now.Subtract(LastActivity).TotalSeconds < 5);
            DelayedVerificationTask = null;
            OnIdle();
        }


        public void OnIdle()
        {
            IsIdle = true;
            ChadderApp.UIHelper.OnIdle();
        }

        public async void BackFromIdle()
        {
            IsIdle = false;
            ChadderApp.UIHelper.BackFromIdle();
        }


        static private ActivitiesManager _instance;
        static public ActivitiesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivitiesManager();
                }
                return _instance;
            }
        }
        private ActivitiesManager() { }
    }
    public class BaseActionBarActivity : Android.Support.V7.App.AppCompatActivity
    {
        protected Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler> EventHandlers = new Dictionary<INotifyPropertyChanged, PropertyChangedEventHandler>();
        protected virtual bool HasToBeOnline() { return true; }

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            EventHandlers.Clear();
#if !DEBUG
            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure); // Block screenshots in production mode
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();
            ActivitiesManager.Instance.current = this;

            foreach (var pair in EventHandlers)
            {
                pair.Key.PropertyChanged += pair.Value;
            }

            InvalidateData();
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (ActivitiesManager.Instance.current == this)
                ActivitiesManager.Instance.current = null;
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.HideKeyboard();

            foreach (var pair in EventHandlers)
            {
                if (pair.Key != null)
                    pair.Key.PropertyChanged -= pair.Value;
            }
        }

        public virtual void InvalidateData() { }

        public ChadderUIHelper ChadderUI
        {
            get
            {
                ChadderApp.UIHelper.ActivityContext = this;
                return ChadderApp.UIHelper;
            }
        }
    }
}