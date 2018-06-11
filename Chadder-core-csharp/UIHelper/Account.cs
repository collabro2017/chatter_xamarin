using Chadder.Client.Data;
using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chadder.Droid;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
#if WINDOWS_APP
        static public Chadder.DosRT.Views.BasePage CurrentPage;
        static public void ShowLoading()
        {
            CurrentPage.ShowLoading();
        }
        static public void HideLoading()
        {
            CurrentPage.HideLoading();
        }
#endif
        public async Task<bool> CreateUser(string username, string password, string repeat)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ChadderApp.UIHelper.ShowError("Username is required!");
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
                password = null;
            else if (password != repeat)
            {
                ChadderApp.UIHelper.ShowError("Password's don't match");
                return false;
            }
            ChadderApp.UIHelper.HideKeyboard();
            ChadderApp.UIHelper.ShowLoading();
            if (await Source.DeviceExists() == false)
            {
                var temp = await Source.CreateDevice();
                if (temp != ChadderError.OK)
                {
                    ChadderApp.UIHelper.ShowError(temp);
                    ChadderApp.UIHelper.HideLoading();
                    return false;
                }
            }
            var result = await Source.CreateUser(username, username, password);
            ChadderApp.UIHelper.HideLoading();
            if (result == ChadderError.OK)
                return true;
            ChadderApp.UIHelper.ShowError(result);
            return false;
        }
        public async Task<bool> Login(string username, string password)
        {
            ChadderApp.UIHelper.ShowLoading();
            var initTimer = Insight.StartTimer("Login load");
            var result = await Source.Login(username, password);
            Insight.StopTimer(initTimer);
            ChadderApp.UIHelper.HideLoading();
            return ShowErrorIfNotOk(result);
        }

        public async Task Logout()
        {
            ChadderApp.UIHelper.ShowLoading();
            await Source.Logout();
            ChadderApp.UIHelper.HideLoading();
        }

        public async Task<bool> CreateNewKey()
        {
            ChadderApp.UIHelper.ShowLoading();
            var result = await Source.CreateNewKey();
            ChadderApp.UIHelper.HideLoading();
            return ShowErrorIfNotOk(result);
        }
        public async Task<ChadderError> ResetPassword(string email)
        {
            return await Source.RequestPasswordReset(email);
        }

        public async Task<ChadderContact> ScanFingerprint(string text)
        {
            return Source.ProcessScannedId(text);
        }

        public async Task<string> GetFingerprint()
        {
            return Source.GetMyScannableId();
        }
    }
}