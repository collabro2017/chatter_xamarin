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

namespace ChadderLib.Util
{
    public static class HelperExtensions
    {
        public static string t(this string aString)
        {
            try
            {
                var context = Application.Context;
                var packageName = context.PackageName;
                int resId = context.Resources.GetIdentifier(aString, "string", packageName);
                return context.GetString(resId);
            }
            catch
            {
                return aString;
            }
        }
    }
}