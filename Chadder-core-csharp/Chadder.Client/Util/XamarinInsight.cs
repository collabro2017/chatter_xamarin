using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Util
{
    public class XamarinInsight : IInsight
    {
        public XamarinInsight(string key)
        {
            try
            {
#if WINDOWS_DESKTOP
                Xamarin.Insights.Initialize(key, "1", "1");
#elif DEBUG && __ANDROID__
                Xamarin.Insights.Initialize(Xamarin.Insights.DebugModeKey, Android.App.Application.Context);
#elif DEBUG
                Xamarin.Insights.Initialize(Xamarin.Insights.DebugModeKey);
#elif __ANDROID__
            Xamarin.Insights.Initialize(key, Android.App.Application.Context);
#else
            Xamarin.Insights.Initialize(key);
#endif
            }
            catch { }
        }

        public void Report(Exception ex)
        {
            try
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
                Xamarin.Insights.Report(ex);
            }
            catch { }
        }

        public void Track(string e, ChadderError error)
        {
            try
            {
                Track(string.Format("{0} - {1}", e, error.ToString()));
            }
            catch { }
        }

        public void Track(string e)
        {
            try
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                Xamarin.Insights.Track(e);
            }
            catch { }
        }
#if DEBUG
        public class TimerHandle
        {
            public TimerHandle(Xamarin.ITrackHandle h, string s)
            {
                Handle = h;
                Time = DateTime.UtcNow;
                Text = s;
            }
            public Xamarin.ITrackHandle Handle { get; set; }
            public DateTime Time { get; set; }
            public string Text { get; set; }
        }
#endif
        public object StartTimer(string e)
        {
            try
            {
                var handle = Xamarin.Insights.TrackTime(e);
                handle.Start();
#if DEBUG
                Console.WriteLine("StartTimer: " + e);
                return new TimerHandle(handle, e);
#else
                return handle;
#endif
            }
            catch (Exception ex) { return null; }
        }

        public void StopTimer(object h)
        {
            try
            {
#if DEBUG
                var total = h as TimerHandle;
                if (total == null)
                    return;
                Console.WriteLine(string.Format("StopTimer: {0} - {1}ms", total.Text, DateTime.UtcNow.Subtract(total.Time).TotalMilliseconds));
                var handle = total.Handle;
#else
                var handle = h as Xamarin.ITrackHandle;
#endif
                if (handle != null)
                {
                    handle.Stop();
                }
            }
            catch
            {
            }
        }
    }
}
