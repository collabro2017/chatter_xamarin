using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Util
{
    public class CryptoHelper
    {
        static public Task<byte[]> CreateRandomData(uint sizeInBytes)
        {
            return Task.Run(() =>
            {
                SecureRandom random = new SecureRandom();
                byte[] key = new byte[sizeInBytes];
                random.NextBytes(key);
                return key;
            });
        }
        static public byte[] Sha256(byte[] data)
        {
            return Digest(data, new Sha256Digest());
        }

        static public byte[] Sha1(byte[] data)
        {
            return Digest(data, new Sha1Digest());
        }

        static public byte[] Digest(byte[] data, GeneralDigest digest)
        {
            var hashed = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(hashed, 0);
            return hashed;
        }
    }
}
