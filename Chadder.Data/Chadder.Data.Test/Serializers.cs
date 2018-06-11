using Chadder.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chadder.Data.Test
{
    [TestClass]
    public class Serializers
    {
        [TestMethod]
        public void Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new DataSerializer(stream);
                var random = new Random();
                int int32 = random.Next();
                short int16 = (short)random.Next(short.MaxValue);
                bool b = random.Next(1) == 1;
                byte[] bin = new byte[random.Next(1000)];
                random.NextBytes(bin);
                byte[] FixedBin = new byte[random.Next(1000)];
                random.NextBytes(FixedBin);
                DateTime date = new DateTime(random.Next());
                TimeSpan span = new TimeSpan(random.Next());

                byte[] temp = new byte[random.Next(128)];
                random.NextBytes(temp);
                string sStr = Convert.ToBase64String(temp);
                string str = Convert.ToBase64String(bin);

                writer.WriteInt(int32);
                writer.WriteShort(int16);
                writer.WriteBool(b);
                writer.WriteBin(null);
                writer.WriteBin(bin);
                writer.WriteBin(FixedBin, FixedBin.Length);
                writer.WriteDate(date);
                writer.WriteTimeSpan(span);
                writer.WriteShortString(null);
                writer.WriteString(null);
                writer.WriteShortString(sStr);
                writer.WriteString(str);

                var result = stream.ToArray();
                Console.WriteLine("Result array size:{0}", result);

                var reader = new DataDeserializer(result);
                Assert.AreEqual(int32, reader.ReadInt());
                Assert.AreEqual(int16, reader.ReadShort());
                Assert.AreEqual(b, reader.ReadBool());
                Assert.AreEqual(null, reader.ReadBin());
                Assert.IsTrue(bin.SequenceEqual(reader.ReadBin()));
                Assert.IsTrue(FixedBin.SequenceEqual(reader.ReadBin(FixedBin.Length)));
                Assert.AreEqual(date, reader.ReadDate());
                Assert.AreEqual(span, reader.ReadTimeSpan());
                Assert.AreEqual(null, reader.ReadShortString());
                Assert.AreEqual(null, reader.ReadString());
                Assert.AreEqual(sStr, reader.ReadShortString());
                Assert.AreEqual(str, reader.ReadString());
                Assert.AreEqual(result.Length, reader.totalSize);
            }
        }
    }
}
