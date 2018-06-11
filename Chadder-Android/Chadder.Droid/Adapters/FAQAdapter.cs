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

namespace Chadder.Droid
{
    public class FAQAdapter : ArrayAdapter<Tuple<string, string>>
    {
        Activity _context;
        int _resource;

        public FAQAdapter(Activity context, int resource, Tuple<string, string>[] items)
            : base(context, resource, items)

        {
            _context = context;
            _resource = resource;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            FAQHolder Holder;

            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(_resource, null);
                view.Click += Marquee_Click;

                Holder = new FAQHolder();
                Holder.Question = view.FindViewById<TextView>(Resource.Id.faq_question);
                Holder.Answer = view.FindViewById<TextView>(Resource.Id.faq_answer);
                Holder.ExpandImage = view.FindViewById<ImageView>(Resource.Id.faq_expand_marquee);

                view.Tag = Holder;
            }
            else
                Holder = (FAQHolder)view.Tag;

            var faq = GetItem(position);

            Holder.Question.Text = faq.Item1;
            Holder.Answer.Text = faq.Item2;


            Holder.ExpandImage.SetImageResource(Resource.Drawable.ic_faq_more);
            Holder.Answer.Visibility = ViewStates.Gone;


            return view;


        }

        void Marquee_Click(object sender, EventArgs e)
        {
            var parent = (RelativeLayout)sender;
            var marquee = parent.FindViewById<ImageView>(Resource.Id.faq_expand_marquee);

            var answer = parent.FindViewById<TextView>(Resource.Id.faq_answer);

            if (answer.Visibility == ViewStates.Gone)
            {
                answer.Visibility = ViewStates.Visible;
                marquee.SetImageResource(Resource.Drawable.ic_faq_less);
            }
            else
            {
                answer.Visibility = ViewStates.Gone;
                marquee.SetImageResource(Resource.Drawable.ic_faq_more);
            }
        }


    }

    internal class FAQHolder : Java.Lang.Object
    {
        public TextView Question, Answer;
        public ImageView ExpandImage;
    }
}