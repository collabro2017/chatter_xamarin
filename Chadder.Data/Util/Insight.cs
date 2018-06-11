using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Util
{
    public enum ChadderError
    {
        RESERVED = 0x00000000, OK,
        CONNECTION_ERROR = 0x00010000, OFFLINE, NOT_AUTHORIZED, NOT_FOUND, INVALID_INPUT, DEVICE_DELETED, ONE_TIME_NOT_FOUND, INVALID_USERNAME, INVALID_NAME,
        GENERAL_EXCEPTION = 0x00020000, INVALID_PASSWORD, INVALID_CREDENTIALS, EMAIL_ALREADY_IN_USE, INVALID_EMAIL, INVALID_USER, INVALID_PICTURE, USERNAME_ALREADY_IN_USE, ///------ NEXT LINE
        PHONE_NUMBER_ALREADY_IN_USE, INVALID_PHONE_NUMBER,
        DATABASE_EXCEPTION = 0x00030000, DB_FAILED_OPEN,
        SCAN_INVALID = 0x00040000, SCAN_NOT_FOUND, SCAN_WRONG_USER,
        SERVER_SIDE_EXCEPTION = 0x00050000, INVALID_DEVICE,
    };

    public interface IInsight
    {
        void Report(Exception ex);
        void Track(string e);
        object StartTimer(string e);
        void StopTimer(object h);
    }

    public class Insight
    {
        static public IInsight Instance;
        static public void Report(Exception e)
        {
            if (Instance != null)
                Instance.Report(e);
        }
        static public void Track(string e, ChadderError error)
        {
            Track(string.Format("{0} - {1}", e, error));
        }
        static public void Track(string e)
        {
            if (Instance != null)
                Instance.Track(e);
        }
        static public void Track(string format, params object[] args)
        {
            Track(string.Format(format, args));
        } 

        static public object StartTimer(string e)
        {
            if (Instance != null)
                return Instance.StartTimer(e);
            return null;
        }
        static public void StopTimer(object h)
        {
            if (Instance != null)
                Instance.StopTimer(h);
        }
    }
}
