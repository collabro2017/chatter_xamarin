using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using System.Threading;
using System.Threading.Tasks;
using Chadder.Droid.Util;
using Chadder.Droid;
using Chadder.Droid.Views.Main;
using Chadder.Data.Util;

namespace Chadder.Droid.Views.Account
{
    public class LoginFragment : BaseFragment
    {
        TextView tUsername;
        TextView tPwd;
        Button bSubmit;
        View view;
        Context cont;
        public enum WrongPasswordOption { TRYAGAIN, DELETE, SKIP };
        static public Task<WrongPasswordOption> wrongPasswordAsync()
        {
            var completion = new TaskCompletionSource<WrongPasswordOption>();
            AlertDialog.Builder dialog = new AlertDialog.Builder(ActivitiesManager.Instance.current);
            dialog.SetTitle("Wrong Password");
            dialog.SetMessage("Wrong Password");

            dialog.SetPositiveButton("Try again", (sender, e) =>
            {
                completion.SetResult(WrongPasswordOption.TRYAGAIN);
            });
            dialog.SetNegativeButton("Delete Device", (sender, e) =>
            {
                completion.SetResult(WrongPasswordOption.DELETE);
            });
            dialog.SetNeutralButton("Skip", (sender, e) =>
            {
                completion.SetResult(WrongPasswordOption.SKIP);
            });
            dialog.Create().Show();
            return completion.Task;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.login_fragment, container, false);
            cont = this.Activity;
            loadInterfaceElements();
            return view;
        }

        private void loadInterfaceElements()
        {

            tUsername = (TextView)view.FindViewById(Resource.Id.login_username);
            tPwd = (TextView)view.FindViewById(Resource.Id.login_pwd);

            bSubmit = (Button)view.FindViewById(Resource.Id.login_submit);

            var forgotPwd = (Button)view.FindViewById(Resource.Id.login_forgot);
            forgotPwd.Click += forgotPwd_Click;
            bSubmit.Click += submitClick;
        }

        private void forgotPwd_Click(object sender, EventArgs e)
        {
            var content = new EditText(cont);
            content.Text = "";
            content.Hint = GetString(Resource.String.LoginForgotPasswordHint);
            content.InputType = Android.Text.InputTypes.TextVariationEmailAddress;

            var dialog = new Util.ConfirmationDialogWithProgress(cont);
            dialog.Title = GetString(Resource.String.LoginForgotPasswordTitle);
            dialog.Positive = GetString(Resource.String.LoginForgotPasswordConfirm);
            dialog.Content = content;

            dialog.OnPositiveAsync += async () =>
            {
                var result = await ChadderUI.ResetPassword(content.Text.Trim());
                if (result != ChadderError.OK)
                    ChadderApp.UIHelper.ShowError(result);
                else
                    Toast.MakeText(cont, Resource.String.LoginForgotPasswordResult, ToastLength.Long).Show();
            };

            dialog.Show();

        }
        private async void submitClick(object sender, System.EventArgs ea)
        {
            var result = await ChadderUI.Login(tUsername.Text, tPwd.Text);
            if (result)
                (Activity as OfflineActivity).GoToMain();
        }
    }
}