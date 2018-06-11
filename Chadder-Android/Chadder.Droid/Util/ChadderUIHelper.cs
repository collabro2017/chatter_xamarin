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
using Chadder.Droid.Util;
using Chadder.Data.Util;
using Android.Views.InputMethods;
using Chadder.Droid;
using Chadder.Droid.Views;
using Android.Provider;

namespace Chadder
{
    public class ChadderUIHelperDroid : ChadderUIHelper
    {
        public Activity ActivityContext;
        private ProgressDialog ProgressDialog;
        public List<string> GetContactEmails()
        {
            var uri = Android.Provider.ContactsContract.CommonDataKinds.Email.ContentUri;
            string[] projection = { ContactsContract.CommonDataKinds.Email.Address,
                                  };

            var cursor = ActivityContext.ManagedQuery(uri, projection, null, null, null);
            var emails = new List<string>();
            if (cursor.MoveToFirst())
            {
                do
                {
                    var email = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    if (email != null)
                        emails.Add(email);
                } while (cursor.MoveToNext());
            }
            return emails;
        }
        public List<string> GetContactPhones()
        {
            var uri = Android.Provider.ContactsContract.CommonDataKinds.Phone.ContentUri;
            string[] projection = { ContactsContract.CommonDataKinds.Phone.NormalizedNumber,
                                  };

            var cursor = ActivityContext.ManagedQuery(uri, projection, null, null, null);
            var phones = new List<string>();
            if (cursor.MoveToFirst())
            {
                do
                {
                    var phone = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    if (phone != null)
                        phones.Add(phone);
                } while (cursor.MoveToNext());
            }
            return phones;
        }
        public void ShowLoading()
        {
            ProgressDialog = new ProgressDialog(ActivityContext);
            ProgressDialog.SetMessage("Loading");
            ProgressDialog.SetCancelable(false);
            ProgressDialog.Show();
        }

        public void HideLoading()
        {
            ProgressDialog.Dismiss();
            ProgressDialog = null;
        }
        public void ShowKeyboard(View view)
        {
            view.RequestFocus();
            InputMethodManager imm = (InputMethodManager)ActivityContext.GetSystemService(Context.InputMethodService);
            imm.ToggleSoftInput(ShowFlags.Forced, 0);
        }
        public void HideKeyboard(View view)
        {
            var manager = ((InputMethodManager)ActivityContext.GetSystemService(ChadderApp.InputMethodService));
            manager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
        }
        public void HideKeyboard()
        {
            if (ActivitiesManager.Instance.current == null)
                return;
            View view = ActivitiesManager.Instance.current.CurrentFocus;
            if (view != null)
                HideKeyboard(view);
            else
                HideKeyboard(new View(ActivitiesManager.Instance.current));
        }

        public Task<int> ConfirmTriple(string msg, string Title, string Yes = "Ok", string No = "Cancel", bool cancellable = true, string Neutral = "")
        {
            var task = new TaskCompletionSource<int>();
            var dialog = new ConfirmationDialog(ActivityContext);
            dialog.Title = Title;
            dialog.Message = msg;
            dialog.Positive = Yes;
            dialog.Negative = No;
            dialog.Neutral = Neutral;
            dialog.IsCancellable = cancellable;
            dialog.OnNegative += () => task.TrySetResult(0);
            dialog.OnPositive += () => task.TrySetResult(1);
            dialog.OnNeutral += () => task.TrySetResult(2);

            dialog.Show();
            return task.Task;
        }
        public void ShowError(ChadderError error)
        {
            ShowError(GetString(error));
        }
        public void ShowError(string error)
        {
            Toast.MakeText(ActivityContext, error, ToastLength.Long).Show();
        }
        public Task<string> PasswordInputDialog(string msg, string title, string hint, string ok = "Ok", string cancel = "Cancel")
        {
            var completion = new TaskCompletionSource<string>();
            AlertDialog.Builder dialog = new AlertDialog.Builder(ActivityContext);
            dialog.SetTitle(title);
            dialog.SetMessage(msg);
            var codeText = new EditText(ActivityContext);
            codeText.Text = "";
            codeText.Hint = hint;
            codeText.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
            dialog.SetView(codeText);

            dialog.SetPositiveButton(ok, (sender, e) =>
            {
                HideKeyboard(codeText);
                completion.TrySetResult(codeText.Text);
            });
            dialog.SetNegativeButton(cancel, (sender, e) =>
            {
                HideKeyboard(codeText);
                completion.TrySetResult(null);
            });
            dialog.SetCancelable(false);
            dialog.Create().Show();
            ShowKeyboard(codeText);

            return completion.Task;
        }
        public Task<Tuple<int, string>> TextInputDialogTriple(string message, string title, string Ok, string cancel = "Cancel", string initialValue = "", string Neutral = "")
        {
            var task = new TaskCompletionSource<Tuple<int, string>>();
            var content = new EditText(ActivityContext);
            content.Text = initialValue;

            var dialog = new ConfirmationDialog(ActivityContext);
            dialog.Title = title;
            dialog.Message = message;
            dialog.Positive = Ok;
            dialog.Neutral = Neutral;
            dialog.Content = content;

            dialog.OnPositive += () =>
            {
                HideKeyboard(content);
                task.TrySetResult(new Tuple<int, string>(1, content.Text));
            };
            dialog.OnNegative += () =>
            {
                HideKeyboard(content);
                task.TrySetResult(new Tuple<int, string>(0, null));
            };
            dialog.OnNeutral += () =>
            {
                HideKeyboard(content);
                task.TrySetResult(new Tuple<int, string>(2, null));
            };
            dialog.Show();
            ShowKeyboard(content);
            return task.Task;
        }
        public Task<Tuple<string, string>> NewPasswordInputDialog(string msgContent, bool hasOld, string oldPwdContent = null, string newPwdContent = null)
        {
            var complete = new TaskCompletionSource<Tuple<string, string>>();
            var content = new LinearLayout(ActivityContext);
            content.Orientation = Orientation.Vertical;

            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);

            layoutParams.SetMargins(20, 5, 20, 5);

            var msg = new TextView(ActivityContext);
            msg.Text = msgContent;
            content.AddView(msg, layoutParams);

            EditText oldPassword = null;
            if (hasOld)
            {
                oldPassword = new EditText(ActivityContext);
                oldPassword.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
                oldPassword.Hint = "Old Password";
                oldPassword.Text = oldPwdContent;
                content.AddView(oldPassword, layoutParams);
            }

            var newPwd = new EditText(ActivityContext);
            newPwd.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
            newPwd.Hint = "New Password";
            newPwd.Text = newPwdContent;
            content.AddView(newPwd, layoutParams);

            var repeatPwd = new EditText(ActivityContext);
            repeatPwd.InputType = newPwd.InputType;
            repeatPwd.Hint = "Repeat Password";
            repeatPwd.Text = newPwdContent;
            content.AddView(repeatPwd, layoutParams);

            var dialog = new ConfirmationDialog(ActivityContext);
            dialog.Title = "Change Password";
            dialog.Positive = "Change";
            dialog.Content = content;

            dialog.OnPositive += async () =>
            {
                await Task.Delay(150);
                if (newPwd.Text == repeatPwd.Text)
                {
                    HideKeyboard();
                    if (hasOld)
                        complete.SetResult(new Tuple<string, string>(oldPassword.Text, newPwd.Text));
                    else
                        complete.SetResult(new Tuple<string, string>(null, newPwd.Text));
                }
                else
                {
                    msg.Text = "Passwords don't match!";
                    dialog.Reshow();
                }
            };

            dialog.OnNegative += () =>
            {
                HideKeyboard();
                complete.SetResult(null);
            };

            dialog.Show();

            return complete.Task;
        }

        public string T(string str)
        {
            return str.t();
        }
        public void Source_OnNotAuthorized()
        {
            Intent i = new Intent(ActivityContext, typeof(Chadder.Droid.Views.Main.OfflineActivity));
            i.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
            ActivityContext.StartActivity(i);
        }
        public static string GetString(ChadderError error)
        {
            try
            {
                return error.ToString().t();
            }
            catch
            {
                return error.ToString();
            }
        }
    }
}