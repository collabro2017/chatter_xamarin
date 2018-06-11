using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chadder.Droid;
using ChadderLib.Util;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
        public async Task ChangeAccountPassword()
        {
            //bool changed = false;
            //await InputDialog.TaskWithProgressDialog(this.Activity, async () =>
            //{
            //    var pwds = new Tuple<string, string>("", "");
            //    var msg = "";
            //    do
            //    {
            //        pwds = await InputDialog.GetNewPasswordDialog(this.Activity, msg, true, pwds.Item1, pwds.Item2);
            //        if (pwds == null)
            //            return;
            //        msg = await ChadderDataSource.source.ChangeAccountPassword(pwds.Item1, pwds.Item2);
            //    } while (msg != null);
            //    changed = true;
            //});
            //if (changed)
            //    Toast.MakeText(Activity, "Password changed", ToastLength.Long).Show();
        }
        public async Task ChangeDevicePassword()
        {
            //ChadderError result = ChadderError.OK;
            //await InputDialog.TaskWithProgressDialog(this.Activity, async delegate()
            //{
            //    Tuple<string, string> data = null;
            //    if (ChadderDataSource.source.currentAccount.HasDevicePassword)
            //        data = await InputDialog.GetNewPasswordDialog(this.Activity, "DevicePasswordChangeMessageWithPrevious".t(), true);
            //    else
            //        data = await InputDialog.GetNewPasswordDialog(this.Activity, "DevicePasswordChangeMessageNoPrevious".t(), false);
            //    if (data != null)
            //        result = await ChadderDataSource.source.ChangeDevicePassword(data.Item1, data.Item2);
            //});
            //if (result != ChadderError.OK)
            //    Toast.MakeText(this.Activity, ChadderDataSource.GetString(result), ToastLength.Long).Show();
        }
        public async Task<bool> ChangeShareName()
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.ChangeShareName(!Source.db.LocalUser.IsNamePublic);
            ChadderApp.UIHelper.HideLoading();
            return ShowErrorIfNotOk(result);
        }
        public async Task<bool> ChangeEmail()
        {
            var email = (await ChadderApp.UIHelper.TextInputDialogTriple(Source.db.LocalUser.Email, "Change Email", "Change")).Item2;
            if (email != null)
            {
                ChadderApp.UIHelper.ShowLoading();
                var result = await Source.ChangeEmail(email);
                ChadderApp.UIHelper.HideLoading();
                return ShowErrorIfNotOk(result);
            }

            return false;
        }

        public async Task<bool> ChangePhone()
        {
            return false;
        }

        public async Task<bool> ChangeName(string name = null)
        {
            if (name == null)
                name = (await ChadderApp.UIHelper.TextInputDialogTriple(Source.db.LocalUser.Name, "Change Name", "Change")).Item2;
            if (name != null)
            {
                ChadderApp.UIHelper.ShowLoading();
                var result = await Source.ChangeName(name);
                ChadderApp.UIHelper.HideLoading();
                return ShowErrorIfNotOk(result);
            }
            return false;
        }

        public async Task DeleteAllMessages()
        {
            var option = await ChadderApp.UIHelper.ConfirmTriple("ProfileDeleteAllMessagesMsg".t(), "ProfileDeleteAllMessagesOkay".t());

            ChadderApp.UIHelper.ShowLoading();
            if (option == 1)
                await Source.DeleteAllMessages();
            ChadderApp.UIHelper.HideLoading();
        }

        public async Task<bool> ChangePicture(byte[] data)
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.ChangePicture(data);
            ChadderApp.UIHelper.HideLoading();
            return ShowErrorIfNotOk(result);
        }
    }
}