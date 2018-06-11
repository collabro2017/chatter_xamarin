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
using Chadder.Droid.Util;
using Chadder.Data.Util;

namespace Chadder.Droid.Views
{
    public class RegisterNameFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.register_name_fragment, container, false);

            var skip = view.FindViewById<Button>(Resource.Id.register_skip);
            skip.Click += delegate
            {
                Proceed();
            };

            var txtName = view.FindViewById<EditText>(Resource.Id.register_name);

            var done = view.FindViewById<Button>(Resource.Id.register_name_done);
            done.Click += async delegate
            {
                var value = txtName.Text.Trim();
                if (value.Length > 0)
                {
                    if (await ChadderUI.ChangeName(value))
                        Proceed();
                }
                else
                    Toast.MakeText(Activity, Resource.String.NameIsRequired, ToastLength.Long).Show();
            };
            txtName.Text = ChadderUI.Source.db.LocalUser.DisplayName;
            return view;
        }

        private void Proceed()
        {
            Activity.SupportFragmentManager
                            .BeginTransaction()
                            .Replace(Resource.Id.content_frame, new RegisterEndFragment())
                            .Commit();
        }
    }
}