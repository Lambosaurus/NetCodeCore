using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Compression;

namespace NetCode.Connection
{
    public static class Compress
    {
        static MemoryStream Buffer;

        static Compress()
        {
            Buffer = new MemoryStream(1024);
        }

        public static byte[] Deflate(byte[] data)
        {
            Buffer.Position = 0;
            DeflateStream deflator = new DeflateStream(Buffer, CompressionMode.Compress, true);
            
            deflator.Write(data, 0, data.Length);
            deflator.Close();

            int count = (int)Buffer.Position;
            byte[] result = new byte[count];
            Array.Copy(Buffer.GetBuffer(), result, count);
            return result;
        }

        public static byte[] Enflate(byte[] data)
        {
            Buffer.Position = 0;
            using (DeflateStream enflator = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress, true))
            {
                enflator.CopyTo(Buffer);

                int count = (int)Buffer.Position;
                byte[] result = new byte[count];
                Array.Copy(Buffer.GetBuffer(), result, count);
                return result;
            }
        }
    }
}
