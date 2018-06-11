using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chadder.Data.Util
{
    public class DataDeserializer
    {
        public DataDeserializer(byte[] data, int offset = 0)
        {
            this.data = data;
            this.offset = offset;
            this.totalSize = 0;
        }

        protected byte[] data { get; private set; }
        public int offset { get; protected set; }
        public int totalSize { get; protected set; }

        public byte nextByte() { return data[offset + totalSize]; }

        public int PeekInt()
        {
            return BitConverter.ToInt32(data, offset + totalSize);
        }
        public int ReadInt()
        {
            var i = BitConverter.ToInt32(data, offset + totalSize);
            totalSize += 4;
            return i;
        }
        public Guid ReadGuid()
        {
            var guidData = new byte[16];
            Array.Copy(data, offset + totalSize, guidData, 0, 16);
            totalSize += 16;
            return new Guid(guidData);
        }
        public DateTime ReadDate()
        {
            var date = new DateTime(BitConverter.ToInt64(data, offset + totalSize));
            totalSize += sizeof(long);
            return date;
        }
        public string ReadString()
        {
            var size = BitConverter.ToInt32(data, offset + totalSize);
            totalSize += 4;
            if (size == -1)
                return null;
            var str = Encoding.UTF8.GetString(data, offset + totalSize, size);
            totalSize += size;
            return str;
        }

        public byte ReadByte()
        {
            return data[offset + totalSize++];
        }
        public void ReadBin(byte[] dest, int length)
        {
            Array.Copy(data, offset + totalSize, dest, 0, length);
            totalSize += length;
        }
        public byte[] ReadBin(int length)
        {
            var dest = new byte[length];
            Array.Copy(data, offset + totalSize, dest, 0, length);
            totalSize += length;
            return dest;
        }
        public byte[] ReadBin()
        {
            var size = ReadInt();
            if (size == 0)
                return null;
            var data = new byte[size];
            ReadBin(data, size);
            return data;
        }
        public bool ReadBool()
        {
            return BitConverter.ToBoolean(data, offset + totalSize++);
        }
        public long ReadLong()
        {
            var i = BitConverter.ToInt64(data, offset + totalSize);
            totalSize += 8;
            return i;
        }
        public string ReadShortString()
        {
            var size = data[offset + totalSize++];
            if (size == 0xFF)
                return null;
            var str = Encoding.UTF8.GetString(data, offset + totalSize, size);
            totalSize += size;
            return str;
        }

        public TimeSpan ReadTimeSpan()
        {
            var date = new TimeSpan(BitConverter.ToInt64(data, offset + totalSize));
            totalSize += sizeof(long);
            return date;
        }
        public Org.BouncyCastle.Math.BigInteger readBCBigInteger()
        {
            var size = ReadInt();

            var i = new Org.BouncyCastle.Math.BigInteger(data, offset + totalSize, size);
            totalSize += size;
            return i;
        }

        public Chadder.Data.Base.Content ReadContent()
        {
            return Chadder.Data.Base.Content.Deserialize(this);
        }
        public T ReadContent<T>() where T : Chadder.Data.Base.Content
        {
            return Chadder.Data.Base.Content.Deserialize(this) as T;
        }

        public short ReadShort()
        {
            var i = BitConverter.ToInt16(data, offset + totalSize);
            totalSize += 2;
            return i;
        }
    }
}
