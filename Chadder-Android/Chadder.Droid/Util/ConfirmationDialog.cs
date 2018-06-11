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

namespace Chadder.Droid.Util
{
    public class ConfirmationDialogWithProgress : ConfirmationDialog
    {
        public ConfirmationDialogWithProgress(Context context) : base(context)
        {
            LoadingTitle = "Loading";
            LoadingMessage = "Loading";
        }
        public string LoadingTitle { get; set; }
        public string LoadingMessage { get; set; }

        public delegate Task VoidDelegateAsync();
        public event VoidDelegateAsync OnPositiveAsync;

        public override async void OnPositiveProc(object sender, EventArgs arg)
        {
            var loadingDialog = ProgressDialog.Show(_context, LoadingTitle, LoadingMessage);
            try
            {
                if (OnPositiveAsync != null)
                    await OnPositiveAsync();
            }
            catch { }
            loadingDialog.Dismiss();
        }
    }
    public class ConfirmationDialog : Java.Lang.Object, IDialogInterfaceOnCancelListener
    {
        public ConfirmationDialog(Context context)
        {
            Negative = "Cancel";
            _context = context;
        }
        protected Context _context;
        public string Title { get; set; }
        public string Message { get; set; }
        public View Content { get; set; }
        public string Positive { get; set; }
        public string Negative { get; set; }
        public string Neutral { get; set; }
        public bool IsCancellable { get; set; }

        public delegate void VoidDelegate();
        public event VoidDelegate OnPositive;
        public event VoidDelegate OnNegative;
        public event VoidDelegate OnNeutral;

        public virtual void OnPositiveProc(object sender, EventArgs arg)
        {
            if (OnPositive != null)
                OnPositive();
        }

        public virtual void OnNegativeProc(object sender, EventArgs arg)
        {
            if (OnNegative != null)
                OnNegative();
        }
        public virtual void OnNeutralProc(object sender, EventArgs arg)
        {
            if (OnNeutral != null)
                OnNeutral();
        }

        public void OnCancel(IDialogInterface dialog)
        {
            if (OnNegative != null)
                OnNegative();
        }

        private Dialog _dialog;
        public void Show()
        {

            AlertDialog.Builder dialog = new AlertDialog.Builder(_context);
            dialog.SetTitle(Title);
            if (string.IsNullOrWhiteSpace(Message) == false)
                dialog.SetMessage(Message);
            if (Content != null)
                dialog.SetView(Content);
            dialog.SetPositiveButton(Positive, OnPositiveProc);
            dialog.SetNegativeButton(Negative, OnNegativeProc);
            if (string.IsNullOrWhiteSpace(Neutral) == false)
                dialog.SetNeutralButton(Neutral, OnNeutralProc);
            dialog.SetOnCancelListener(this);
            dialog.SetCancelable(IsCancellable);
            _dialog = dialog.Create();
            Reshow();
        }

        public void Reshow()
        {
            _dialog.Show();
        }
    }
}