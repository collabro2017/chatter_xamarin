using Chadder.Client.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Chadder.Droid;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
        public async void ChangeDeviceName(ChadderUserDevice device)
        {
            //++var name = await TextInputDialog(device.Name, "Change Name", "Change");
            //if (name != null)
            //{
            ChadderApp.UIHelper.ShowLoading();
            //    var result = await Source.ChangeDeviceName(device, name);
            ChadderApp.UIHelper.HideLoading();
            //    ShowErrorIfNotOk(result);
            //}
        }

        public async void PairDevice(ChadderUserDevice device)
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.PairDevice(device);
            ChadderApp.UIHelper.HideLoading();
            ShowErrorIfNotOk(result);
            //var dialog = new ConfirmationDialogWithProgress(Activity);
            //dialog.Title = GetString(Resource.String.DevicePairTitle);
            //dialog.Message = string.Format(GetString(Resource.String.DevicePairMessage), device.Public.FingerprintEncodedClipped);

            //dialog.Positive = GetString(Resource.String.DevicePairConfirm);

            //dialog.OnPositiveAsync += async () =>
            //{
            //    var result = await ChadderDataSource.source.pairDevice(device);
            //    if (result != null)
            //        Toast.MakeText(Activity, result, ToastLength.Long).Show();
            //};

            //dialog.Show();
        }

        public async void DeleteDevice(ChadderUserDevice device)
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.DeleteDevice(device);
            ChadderApp.UIHelper.HideLoading();
            ShowErrorIfNotOk(result);
            //var dialog = new ConfirmationDialogWithProgress(Activity);
            //dialog.Title = "Delete Device";
            //dialog.Message = "Are you sure you want to delete this device?";

            //dialog.Positive = "Delete";

            //dialog.OnPositiveAsync += async () =>
            //{
            //    var result = await ChadderDataSource.source.deleteDevice(device);
            //    if (result != null)
            //        Toast.MakeText(Activity, result, ToastLength.Long).Show();
            //};
            //dialog.Show();
        }
    }
}