using Chadder.Data.Base;
using Chadder.Data.Util;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Keys
{
    public class ECDSAPublicKey : Content
    {
        public override int Type { get { return (int)CONTENT_TYPE.KEY_ECDSA_PUBLIC; } }

        public byte[] PublicKeyData { get; set; }

        public ECPublicKeyParameters PublicKey
        {
            get
            {
                var domain = getDomain();
                return new ECPublicKeyParameters(domain.Curve.DecodePoint(PublicKeyData), domain);
            }
        }
        public byte[] Compressed
        {
            get
            {
                return PublicKey.Q.Curve.CreatePoint(PublicKey.Q.X.ToBigInteger(), PublicKey.Q.Y.ToBigInteger(), true).GetEncoded();
            }
        }

        public override void Serialize(Util.DataSerializer writer)
        {
            base.Serialize(writer);
            writer.WriteBin(PublicKeyData, 65);
        }

        public override void Read(Util.DataDeserializer reader)
        {
            base.Read(reader);
            PublicKeyData = reader.ReadBin(65);
        }

        public Task<bool> Verify(byte[] data, byte[] signature)
        {
            return Task.Run(() =>
            {
                data = CryptoHelper.Sha256(data);
                var ecdsa = new ECDsaSigner();
                var domain = getDomain();
                ecdsa.Init(false, PublicKey);
                var r_length = signature[3];
                var r = new BigInteger(signature, 4, r_length);
                var s = new BigInteger(signature, 4 + r_length + 2, signature[5 + r_length]);
                return ecdsa.VerifySignature(data, r, s);
            });
        }
        public Task<bool> Verify(IContentSerializable data, byte[] signature)
        {
            return Verify(Serialize(data), signature);
        }
        static public Org.BouncyCastle.Crypto.Parameters.ECDomainParameters getDomain()
        {
            var ecps = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByOid(Org.BouncyCastle.Asn1.Sec.SecObjectIdentifiers.SecP256k1);
            return new Org.BouncyCastle.Crypto.Parameters.ECDomainParameters(
                    ecps.Curve, ecps.G, ecps.N, ecps.H, ecps.GetSeed());
        }
        public byte[] Fingerprint
        {
            get
            {
                return Util.CryptoHelper.Sha256(PublicKeyData);
            }
        }

        public string FingerprintEncoded
        {
            get
            {
                return BitConverter.ToString(Fingerprint).Replace("-", "");
            }
        }

        public string FingerprintEncodedClipped
        {
            get
            {
                return FingerprintEncoded.Substring(0, 32);
            }
        }


        static public byte[] FromLegacy(byte[] legacy)
        {

            if (legacy.Length == 65)
            {
                var result = new ECDSAPublicKey();
                result.PublicKeyData = legacy;
                return result.Serialize();
            }
            return legacy;
        }
    }
}
