using System;
using System.IO;

using Ionic.Zlib;

namespace SM.PKG.Unpacker
{
    class Zlib
    {
        public static byte[] iDecompress(byte[] pScrBuffer, int dwZSize)
        {
            byte[] result;
            Array.Resize(ref pScrBuffer, dwZSize);
            using (MemoryStream TMemoryStream = new MemoryStream())
            {
                using (MemoryStream TMemoryStream2 = new MemoryStream(pScrBuffer))
                {
                    using (ZlibStream TZlibStream = new ZlibStream(TMemoryStream2, CompressionMode.Decompress, CompressionLevel.Default))
                    {
                        TZlibStream.CopyTo(TMemoryStream);
                        result = TMemoryStream.ToArray();
                    }
                }
            }
            return result;
        }
    }
}
