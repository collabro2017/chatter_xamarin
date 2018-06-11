using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using ChadderLib.Util;

namespace Chadder.Droid.Views.Main
{

    public class FaqFragment : Android.Support.V4.App.Fragment
    {
        View view;
        Context cont;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.faq_fragment, container, false);
            cont = this.Activity;

            var faqList = view.FindViewById<ListView>(Resource.Id.faq_list);


            var numFaq = int.Parse("faq_number".t());
            Tuple<string, string>[] faq = new Tuple<string, string>[numFaq];

            for (int i = 0; i < numFaq; ++i)
            {
                var question = ("faq_question_" + i.ToString()).t();
                var answer = ("faq_answer_" + i.ToString()).t();
                faq[i] = new Tuple<string, string>(question, answer);
            }

            faqList.Adapter = new FAQAdapter(this.Activity, Resource.Layout.faq_list_item, faq);
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            (Activity as MainActivity).SetDrawerEnabled(false);
        }



    }
}