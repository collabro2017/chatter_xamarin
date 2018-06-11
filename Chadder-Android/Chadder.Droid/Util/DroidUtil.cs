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
using Android.Database;
using Android.Views.InputMethods;
using Android.Content.Res;
using System.IO;

namespace Chadder
{
    using DroidEnviroment = Android.OS.Environment;
    using Java.Text;
    using Java.Util;
    using Chadder.Droid.Views;
    using Chadder.Droid;

    public static class DroidUtil
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
        public enum MediaType { IMAGE, VIDEO };

        public static ISharedPreferences GetSharedPreferences(this Context context)
        {
            return context.GetSharedPreferences(context.GetString(Resource.String.shared_preferences_file), FileCreationMode.Private);
        }
        public static string GetRealPathFromURI(Android.Net.Uri uri, Activity activity)
        {
            string doc_id = "";
            using (var c1 = activity.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = activity.ManagedQuery(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }

        public static void HideKeyboard(this Activity activity)
        {
            View view = activity.CurrentFocus;
            var manager = ((InputMethodManager)ChadderApp.Context.GetSystemService(ChadderApp.InputMethodService));
            if (view != null)
                manager.HideSoftInputFromWindow(view.WindowToken, 0);
            else
                manager.HideSoftInputFromWindow(new View(activity).WindowToken, 0);
        }

        public static void ShowKeyboard(this Activity activity)
        {
            View view = activity.CurrentFocus;
            var manager = ((InputMethodManager)ChadderApp.Context.GetSystemService(ChadderApp.InputMethodService));
            manager.ShowSoftInput(view, ShowFlags.Forced);
        }

        public static void AddShadow(this TextView view)
        {
            view.SetShadowLayer(1, 0, 1, Android.Graphics.Color.Black);
        }

        public static string GetChadderMediaPath(MediaType type)
        {
            var ChadderDir = System.IO.Path.Combine(DroidEnviroment.ExternalStorageDirectory.Path,
                                 "Chadder");

            string typeDir = "";

            switch (type)
            {
                case MediaType.IMAGE:
                    typeDir = "Chadder Images";
                    break;
                case MediaType.VIDEO:
                    typeDir = "Chadder Videos";
                    break;

                default:
                    throw new Exception("Unknown MediaType");
            }

            if (!File.Exists(ChadderDir))
                Directory.CreateDirectory(ChadderDir);

            typeDir = System.IO.Path.Combine(ChadderDir, typeDir);
            if (!File.Exists(typeDir))
                Directory.CreateDirectory(typeDir);

            return typeDir;

        }

        public static string GetChadderTempFile()
        {
            var timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Date());
            var file = string.Format("chadder_{0}.jpg", timeStamp);
            return System.IO.Path.Combine(DroidUtil.GetChadderMediaPath(DroidUtil.MediaType.IMAGE), file);
        }


        /*public static int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int) ((pixelValue)/Resources.DisplayMetrics.Density);
            return dp;
        }
        public static float ConvertDpToPixels(int dpValue) {
            var pixel = (float)(dpValue * Resources.DisplayMetrics.Density);
            return pixel;
        }*/
    }
}