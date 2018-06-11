using Chadder.Data.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chadder
{
    public partial class ChadderUIHelper
    {
        public async Task<bool> SplashScreenLoad()
        {
            if (Source.IsOnline)
            {
                Insight.Track("SplashScreenLoad already online");
                return true;
            }
            var timer = Insight.StartTimer("SplashScreenLoad");
            if (await Source.DeviceExists() == false)
                await Source.CreateDevice();

            var user = await Source.GetDefaultUser();
            if (user != null)
            {
                var result = await Source.LoadUser(user);
                Insight.StopTimer(timer);
                Insight.Track("SplashScreenLoad With user", result);
                return ShowErrorIfNotOk(result);
            }
            Insight.StopTimer(timer);
            Insight.Track("SplashScreenLoad no user");
            return false;
        }
    }
}