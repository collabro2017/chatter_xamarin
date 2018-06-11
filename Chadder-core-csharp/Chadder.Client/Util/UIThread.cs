using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Client.Util
{
#if WINDOWS_PHONE_APP
    using VoidDelegate = Windows.UI.Core.DispatchedHandler;
#elif __ANDROID__ || WINDOWS_PHONE || __IOS__
    using VoidDelegate = Action;
#endif
    public class UIThread
    {
        public delegate Task TaskDelegate();
#if !WINDOWS_PHONE_APP && !__ANDROID__ && !__IOS__
        public delegate void VoidDelegate();
#endif
#if WINDOWS_PHONE_APP
        public static Windows.UI.Core.CoreDispatcher Dispatcher { get; set; }
#elif __ANDROID__
        public static Android.App.Activity Activity { get; set; }
#elif __IOS__
        public static Foundation.NSObject uiObject;
#elif WINDOWS_DESKTOP
        public static System.Windows.Threading.Dispatcher Dispatcher { get; set; }
#endif
#pragma warning disable 1998
        static public async void Run(VoidDelegate action)
        {
#if WINDOWS_DESKTOP
            if (Dispatcher == null)
                action();
            else
                await Dispatcher.BeginInvoke(action);
#elif WINDOWS_PHONE_APP
            if (Dispatcher != null)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
#elif WINDOWS_PHONE
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(action);
#elif __ANDROID__
            if (Activity != null)
            {
                Activity.RunOnUiThread(new Java.Lang.Runnable(() =>
                {
                    action();
                }));
            }
#elif __IOS__
            if(uiObject != null)
                uiObject.InvokeOnMainThread(action);
#endif
        }
#pragma warning restore 1998
        static public async Task RunAsync(VoidDelegate action)
        {
#if WINDOWS_DESKTOP
            if (Dispatcher == null)
                action();
            else
                await Dispatcher.BeginInvoke(action);
#elif WINDOWS_PHONE_APP
            if (Dispatcher != null)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
#elif WINDOWS_PHONE
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(action);
#elif __ANDROID__
            if (Activity != null)
            {
                var v = new TaskCompletionSource<object>();
                Activity.RunOnUiThread(new Java.Lang.Runnable(() =>
                {
                    action();
                    v.SetResult(null);
                }));
                await v.Task;
            }
#elif __IOS__
            if (uiObject != null)
                uiObject.InvokeOnMainThread(action);
#endif
        }

        static public async Task RunTask(TaskDelegate action)
        {
#if WINDOWS_DESKTOP
            if (Dispatcher == null)
                await action();
            else
                await Dispatcher.BeginInvoke(action);
#elif WINDOWS_PHONE_APP
            if (Dispatcher != null)
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
#elif WINDOWS_PHONE
            var v = new TaskCompletionSource<object>();
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(async () =>
            {
                await action();
                v.SetResult(null);
            });
            await v.Task;
#elif __ANDROID__
            if (Activity != null)
            {
                var v = new TaskCompletionSource<object>();
                Activity.RunOnUiThread(new Java.Lang.Runnable(async () =>
                {
                    await action();
                    v.SetResult(null);
                }));
                await v.Task;
            }
#elif __IOS__
            if (uiObject != null)
            {
                var v = new TaskCompletionSource<object>();
                uiObject.InvokeOnMainThread(async () => {
                    await action();
                    v.SetResult(null);
                });
                await v.Task;
            }
#endif
        }
    }
}
