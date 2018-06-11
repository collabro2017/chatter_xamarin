using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chadder.Data.Util
{
    public class DataSerializer
    {
        public DataSerializer(System.IO.Stream writer)
        {
            this.writer = writer;
        }
        protected System.IO.Stream writer { get; set; }

        public void WriteBool(bool b)
        {
            writer.Write(BitConverter.GetBytes(b), 0, 1);
        }
        public void WriteBin(byte[] data, int length)
        {
            if (data.Length != length)
                Insight.Track("DataSerializer: data.Length != length");
            writer.Write(data, 0, length);
        }
        public void WriteBin(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                WriteInt(0);
            }
            else
            {
                WriteInt(data.Length);
                writer.Write(data, 0, data.Length);
            }
        }
        public void WriteDate(DateTime date)
        {
            writer.Write(BitConverter.GetBytes(date.Ticks), 0, sizeof(long));
        }
        public void WriteTimeSpan(TimeSpan date)
        {
            writer.Write(BitConverter.GetBytes(date.Ticks), 0, sizeof(long));
        }
        public void WriteInt(int i)
        {
            writer.Write(BitConverter.GetBytes(i), 0, 4);
        }
        public void WriteLong(long i)
        {
            writer.Write(BitConverter.GetBytes(i), 0, sizeof(long));
        }
        public void WriteString(string str)
        {
            if (str == null)
                WriteInt(-1);
            else
            {
                var bin = Encoding.UTF8.GetBytes(str);
                writer.Write(BitConverter.GetBytes(bin.Length), 0, sizeof(int));
                writer.Write(bin, 0, bin.Length);
            }
        }
        public void WriteGuid(Guid guid)
        {
            writer.Write(guid.ToByteArray(), 0, 16);
        }
        public void WriteShortString(string str)
        {
            if (str == null)
                writer.WriteByte((byte)0xFF);
            else
            {
                var bin = Encoding.UTF8.GetBytes(str);
                writer.WriteByte((byte)bin.Length);
                writer.Write(bin, 0, bin.Length);
            }
        }
        public void WriteBigInteger(Org.BouncyCastle.Math.BigInteger big)
        {
            var data = big.ToByteArray();
            WriteInt(data.Length);
            writer.Write(data, 0, data.Length);
        }

        public void WriteContent(Chadder.Data.Base.Content content)
        {
            content.Serialize(this);
        }
        public void WriteShort(short i)
        {
            writer.Write(BitConverter.GetBytes(i), 0, 2);
        }
    }
}