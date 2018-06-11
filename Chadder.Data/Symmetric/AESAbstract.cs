using Chadder.Data.Base;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Symmetric
{
    public abstract class AESAbstract : Content
    {
        protected abstract uint IV_size { get; }
        protected byte[] IV { get; set; }
        protected byte[] CipherText { get; set; }

        protected AESAbstract()
        {
        }

        protected async Task Create(byte[] keyData, byte[] content)
        {
            IV = await Util.CryptoHelper.CreateRandomData(IV_size);
            CipherText = await Encrypt(content, keyData, IV);
        }
        public async Task<byte[]> Decrypt(IContentContext context)
        {
            return await Decrypt(CipherText, await GetKey(context), IV);
        }
        public async Task<Content> GetContent(IContentContext context)
        {
            var data = await Decrypt(context);
            if (data == null)
                return null;
            return Deserialize(data);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            IV = reader.ReadBin((int)IV_size);
            CipherText = reader.ReadBin();
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);

            writer.WriteBin(IV, (int)IV_size);
            writer.WriteBin(CipherText);
        }

        public abstract Task<byte[]> GetKey(IContentContext context);

        static public Task<byte[]> Encrypt(byte[] Data, byte[] keyData, byte[] iv)
        {
            return Task.Run(() =>
            {
                var cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");
                cipher.Init(true, new ParametersWithIV(new KeyParameter(keyData), iv));

                return cipher.DoFinal(Data);
            });
        }

        static public Task<byte[]> Decrypt(byte[] CipherText, byte[] keyData, byte[] iv)
        {
            return Task.Run(() =>
            {
                try
                {
                    var cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");
                    cipher.Init(false, new ParametersWithIV(new KeyParameter(keyData), iv));
                    return cipher.DoFinal(CipherText);
                }
                catch
                {
                    return null;
                }
            });
        }

        public override async Task<T> Find<T>(IContentContext context)
        {
            var a = await base.Find<T>(context);
            if (a == null)
            {
                var content = await GetContent(context);
                if(content != null)
                    return await content.Find<T>(context);
            }
            return a;
        }
    }
}
