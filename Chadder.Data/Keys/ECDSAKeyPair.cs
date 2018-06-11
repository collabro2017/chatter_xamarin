using Chadder.Data.Base;
using Chadder.Data.Util;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Keys
{
    public interface IECDSAKeyFactory
    {
        Task<ECDSAKeyPair> Generate();
        Task<PrivateKeyBook> GenerateBook();
        Task GenerateSendReceiveKeys(PrivateKeyBook result);
		Task<PrivateKeyBook> RefreshBook(PrivateKeyBook book);
        Task<PrivateKeyBook> RefreshEphemeralBook(PrivateKeyBook book);
    }
	public class ECDSASimpleFactory : IECDSAKeyFactory
    {
		public async Task<PrivateKeyBook> RefreshEphemeralBook (PrivateKeyBook book)
		{
			var result = new PrivateKeyBook();
			result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
				{
					Purpose = KeyPurpose.MASTER_KEY,
					PrivateKey = book.GetMaster()
				});
			result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
				{
					Purpose = KeyPurpose.DH_RECEIVE,
					PrivateKey = await Generate()
				});
			return result;
		}

        public Task<ECDSAKeyPair> Generate()
        {
            return ECDSAKeyPair.Generate();
        }
        public async Task GenerateSendReceiveKeys(PrivateKeyBook result)
        {
            result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
            {
                Purpose = KeyPurpose.DH_RECEIVE,
                PrivateKey = await Generate()
            });
            result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
            {
                Purpose = KeyPurpose.DH_SEND,
                PrivateKey = await Generate()
            });
        }
        public async Task<PrivateKeyBook> RefreshBook(PrivateKeyBook book)
        {
            var result = new PrivateKeyBook();
            result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
            {
                Purpose = KeyPurpose.MASTER_KEY,
                PrivateKey = book.GetMaster()
            });
            await GenerateSendReceiveKeys(result);
            return result;
        }
        public async Task<PrivateKeyBook> GenerateBook()
        {
            var result = new PrivateKeyBook();
            result.Book.Add(new PrivateKeyBook.PrivateKeyBookRecord()
            {
                Purpose = KeyPurpose.MASTER_KEY,
                PrivateKey = await Generate()
            });
            await GenerateSendReceiveKeys(result);
            return result;
        }
    }
    public class ECDSAKeyPair : ECDSAPublicKey
    {
        public override int Type { get { return (int)CONTENT_TYPE.KEY_ECDSA_PRIVATE; } }

        public Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();
        public byte[] PrivateKeyData { get; set; }
        public ECPrivateKeyParameters PrivateKey
        {
            get { return new ECPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(1, PrivateKeyData), getDomain()); }
        }

        public static Task<ECDSAKeyPair> Generate()
        {
            return Task.Run(() =>
            {
                ECKeyGenerationParameters param = new ECKeyGenerationParameters(getDomain(), new Org.BouncyCastle.Security.SecureRandom());

                var gen = new ECKeyPairGenerator("EC");
                gen.Init(param);
                var pair = gen.GenerateKeyPair();
                var prv = (pair.Private as ECPrivateKeyParameters).D.ToByteArrayUnsigned();
                if (prv.Length != 32)
                {
                    var temp = new byte[32];
                    Array.Clear(temp, 0, 32 - prv.Length);
                    prv.CopyTo(temp, 32 - prv.Length);
                    prv = temp;
                    Insight.Track("Generate ECDSAKeyPair: prv.Length != 32");
                }
                return new ECDSAKeyPair()
                {
                    PublicKeyData = (pair.Public as ECPublicKeyParameters).Q.GetEncoded(),
                    PrivateKeyData = prv
                };
            });
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            PrivateKeyData = reader.ReadBin(32);
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteBin(PrivateKeyData, 32);
        }
        public virtual Task<byte[]> SharedSecret(ECDSAPublicKey Pbk)
        {
            return Task.Run(() =>
            {
                lock (Cache)
                {
                    var clipped = Pbk.FingerprintEncodedClipped;
                    if(Cache.ContainsKey(clipped))
                        return Cache[clipped];
                }
                var agree = new ECDHBasicAgreement();
                agree.Init(PrivateKey);
                var result = agree.CalculateAgreement(Pbk.PublicKey).ToByteArrayUnsigned();
                result = CryptoHelper.Sha256(result);
                lock (Cache)
                    Cache[Pbk.FingerprintEncodedClipped] = result;
                return result;
            });
        }
        public virtual Task<byte[]> Sign(byte[] data)
        {
            return Task.Run(() =>
            {
                data = CryptoHelper.Sha256(data);
                var ecdsa = new ECDsaSigner();
                ecdsa.Init(true, PrivateKey);
                var signature = ecdsa.GenerateSignature(data);
                var r = signature[0].ToByteArray();
                var s = signature[1].ToByteArray();
                var buffer = new byte[r.Length + s.Length + 6];
                buffer[0] = 0x30;
                buffer[1] = (byte)(buffer.Length - 2);
                buffer[2] = 0x02;
                buffer[3] = (byte)r.Length;
                Array.Copy(r, 0, buffer, 4, r.Length);
                buffer[r.Length + 4] = 0x02;
                buffer[r.Length + 4 + 1] = (byte)s.Length;
                Array.Copy(s, 0, buffer, r.Length + 4 + 2, s.Length);
                return buffer;
            });
        }
        public ECDSAPublicKey Public
        {
            get
            {
                return new ECDSAPublicKey()
                {
                    PublicKeyData = PublicKeyData
                };
            }
        }
    }
}
