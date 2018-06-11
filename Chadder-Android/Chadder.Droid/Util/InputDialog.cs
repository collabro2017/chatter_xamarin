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
using ChadderLib.Util;
using Chadder.Droid.Views;
using Chadder.Data.Util;

namespace Chadder.Droid.Util
{
    public class InputDialog
    {
        static private ProgressDialog ProgressDialog;

        public delegate Task TaskDelegate();

        static public void ShowProgressDialog(Context context, string title = "Loading", string msg = "Please wait")
        {
            if (ProgressDialog == null)
                ProgressDialog = ProgressDialog.Show(context, title, title);
        }

        static public void ToggleProgressDialog()
        {
            if (ProgressDialog != null)
            {
                if (ProgressDialog.IsShowing)
                    ProgressDialog.Show();
                else
                    ProgressDialog.Hide();
            }
        }

        static public bool DismissProgressDialog()
        {
            if (ProgressDialog != null)
            {
                ProgressDialog.Dismiss();
                ProgressDialog = null;
                return true;
            }
            return false;
        }
        static public Task TaskWithProgressDialog(Context context, TaskDelegate action, string title = "Loading")
        {
            return TaskWithProgressDialog(context, title, action, title);
        }
        static public async Task TaskWithProgressDialog(Context context, string text, TaskDelegate action, string title = "Loading")
        {
            Util.InputDialog.ShowProgressDialog(context, title, text);
            await action();
            Util.InputDialog.DismissProgressDialog();
        }
        public delegate Task<ChadderError> TaskErrorDelegate();

        static public void Popup(string msg, string title)
        {
            Popup(msg, title, null);
        }

        static public void Popup(string msg, string title, Action action)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(ActivitiesManager.Instance.current);
            dialog.SetTitle(title);
            dialog.SetMessage(msg);
            dialog.SetNeutralButton("Ok", (sender, e) =>
            {
                if (action != null)
                    action();
            });
            dialog.Create().Show();
        }
    }
}