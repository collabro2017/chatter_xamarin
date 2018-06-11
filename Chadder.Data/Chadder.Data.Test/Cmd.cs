using Chadder.Data.Base;
using Chadder.Data.Cmd;
using Chadder.Data.Keys;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Test
{
    [TestClass]
    public class Cmd
    {
        [TestMethod]
        public async Task TestKeys()
        {
            var factory = new ECDSASimpleFactory();

            var pair1 = await factory.Generate();

            var data = pair1.Serialize();
            var pair1copy = Content.Deserialize<ECDSAKeyPair>(data);
            Assert.IsNotNull(pair1copy);
            Assert.IsTrue(pair1.PrivateKeyData.SequenceEqual(pair1copy.PrivateKeyData));

            var pair2 = await factory.Generate();
            var pair3 = await factory.Generate();

            var secret12 = await pair1.SharedSecret(pair2);
            var secret21 = await pair2.SharedSecret(pair1);
            var secret13 = await pair1.SharedSecret(pair3);

            Assert.IsTrue(secret12.SequenceEqual(secret21));
            Assert.IsFalse(secret12.SequenceEqual(secret13));

            var signed = await pair1.Sign(secret12);
            var signed2 = await pair2.Sign(secret12);

            Assert.IsTrue(await pair1.Verify(secret12, signed));
            Assert.IsFalse(await pair1.Verify(secret13, signed));
            Assert.IsFalse(await pair1.Verify(secret12, signed2));
            Assert.IsFalse(await pair2.Verify(secret12, signed));
        }

        [TestMethod]
        public async Task PairDeviceContent()
        {
            var keyFactory = new ECDSASimpleFactory();
            var pairDevice = new PairDeviceContent();
            pairDevice.Book = await keyFactory.GenerateBook();

            var result = pairDevice.Serialize();

            var content = Content.Deserialize(result);
            Assert.IsInstanceOfType(content, typeof(PairDeviceContent));
            var contentPair = content as PairDeviceContent;
            Assert.IsTrue(contentPair.Book.IsEqual(pairDevice.Book));
        }
    }
}
