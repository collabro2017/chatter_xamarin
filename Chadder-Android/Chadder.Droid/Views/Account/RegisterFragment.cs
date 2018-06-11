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

namespace Chadder.Droid.Views.Account
{
    public class RegisterFragment : BaseFragment
    {
        private EditText _etUsername, _etPassword, _etPasswordRepeat;
        private Button _btRegister;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.register_fragment, container, false);

            _etUsername = view.FindViewById<EditText>(Resource.Id.username);
            _etPassword = view.FindViewById<EditText>(Resource.Id.Password);
            _etPasswordRepeat = view.FindViewById<EditText>(Resource.Id.PasswordRepeat);

            _btRegister = view.FindViewById<Button>(Resource.Id.Register);
            _btRegister.Click += submitClick;

            return view;
        }

        async protected void submitClick(Object sender, System.EventArgs ea)
        {
            var result = await ChadderUI.CreateUser(_etUsername.Text, _etPassword.Text, _etPasswordRepeat.Text);
            if (result)
            {
                Intent i = new Intent(Activity, typeof(MainActivity));
                i.PutExtra("new_user", true);
                i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(i);
                Activity.Finish();
            }
        }
    }
}