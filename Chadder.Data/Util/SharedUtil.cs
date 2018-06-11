using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chadder.Data.Util
{
    public class SharedUtil
    {
        static public Task<byte[]> GetKey(string password, byte[] salt, int count)
        {
            return Task.Run(() =>
            {
                var pgen = new Pkcs12ParametersGenerator(new Sha256Digest());
                pgen.Init(PbeParametersGenerator.Pkcs12PasswordToBytes(password.ToCharArray()), salt, count);
                var key = (KeyParameter)pgen.GenerateDerivedParameters("AES", 32 * 8);
                return key.GetKey();
            });
        }

        static public async Task<string> GetPasswordHash(string password)
        {
            var salt = Encoding.UTF8.GetBytes("My Chadder Password");
            var result = await GetKey(password, salt, 10000);
            return Convert.ToBase64String(result);
        }
        public static string IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;
            if(phone[0] != '+')
                return null;
            try
            {
                PhoneNumber number = PhoneNumberUtil.GetInstance().Parse(phone, "US");
                return PhoneNumberUtil.GetInstance().Format(number, PhoneNumberFormat.INTERNATIONAL);
            }
            catch (Exception ex)
            {
                Insight.Track("IsValidPhone try-catch");
                Insight.Track(ex.ToString());
                return null;
            }
        }
        public static string IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            email = email.ToLower().Trim();
            if(Regex.IsMatch(email, "^.+@[\\w\\.]+\\.[\\w]{2,6}$"))
                return email;
            return null;
        }
        public static async Task<string> GetEmailHash(string email)
        {
            return Convert.ToBase64String(await GetKey(email, System.Text.Encoding.UTF8.GetBytes("Chadder Salt"), 1000));
        }
        public static async Task<string> GetPhoneHash(string phone)
        {
            PhoneNumber number = PhoneNumberUtil.GetInstance().Parse(phone, "US");
            phone = PhoneNumberUtil.GetInstance().Format(number, PhoneNumberFormat.E164);
            return Convert.ToBase64String(await GetKey(phone, System.Text.Encoding.UTF8.GetBytes("Chadder Phone Salt"), 2000));
        }

        static public string RandomPassword(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        static public string GetUserScannableId(string userId, Chadder.Data.Keys.ECDSAPublicKey master)
        {
            var compressed = Convert.ToBase64String(master.Compressed);
            return string.Format("1,{0},{1}", userId, compressed);
        }
    }
}
