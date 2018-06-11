using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Source
{
    public class ChadderUrls
    {
#if DEBUG
        //public static string DefaultDomain = "http://suchdesk.student.rit.edu/";
        public static string DefaultDomain = "http://chadder.azurewebsites.net/";
#else
        //public static string DefaultDomain = "https://chadder-service.azurewebsites.net/";
        public static string DefaultDomain = "http://chadder.azurewebsites.net/";
#endif
        public string Domain { get; set; }

        public ChadderUrls()
        {
            Domain = DefaultDomain;
        }

        public string Api { get { return Domain + "/api"; } }
        public string Account { get { return Domain + "/api/Account"; } }
        public string Friend { get { return Domain + "/api/Friend"; } }



        public string CreateDevice { get { return Account + "/CreateDevice"; } }
        public string UpdateNotification { get { return Account + "/UpdateNotification"; } }
        public string CreateUser { get { return Account + "/CreateUser"; } }
        public string GetAccessToken { get { return Account + "/GetAccessToken"; } }
        public string Login { get { return Account + "/Login"; } }
        public string Logout { get { return Account + "/Logout"; } }
        public string DeleteUser { get { return Account + "/DeleteUser"; } }
        public string DeleteDevice { get { return Account + "/DeleteDevice"; } }
        public string CreateNewKey { get { return Account + "/CreateNewKey"; } }
        public string GetPackages { get { return Account + "/GetPackages"; } }
        public string RequestPasswordReset { get { return Account + "/RequestPasswordReset"; } }
        public string PasswordReset { get { return Account + "/PasswordReset"; } }
        public string UploadBlob { get { return Account + "/UploadBlob"; } }
        public string DownloadBlob { get { return Account + "/DownloadBlob"; } }


        public string FindUser { get { return Friend + "/FindUser"; } }
        public string AddUser { get { return Friend + "/AddUser"; } }
        public string GetUpdatedFriends { get { return Friend + "/GetUpdatedFriends"; } }
        public string SendPackageToUser { get { return Friend + "/SendPackageToUser"; } }

    }
}
