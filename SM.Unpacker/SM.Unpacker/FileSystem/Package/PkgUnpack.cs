using System;
using System.IO;

namespace SM.PKG.Unpacker
{
    class PkgUnpack
    {
        public static void iDoit(String m_File, String m_DstFolder)
        {
            FileStream TFileStream = new FileStream(m_File, FileMode.Open);

            UInt32 dwMagic = TFileStream.ReadUInt32();
            if (dwMagic != 0x01E2393F)
            {
                Utils.iSetError("[ERROR]: Invalid magic of PKG archive file");
                return;
            }

            UInt32 dwFiles = TFileStream.ReadUInt32();
            UInt32 dwTableOffset = TFileStream.ReadUInt32();
            UInt32 dwCRC32 = TFileStream.ReadUInt32();
            UInt32 dwTableSize = TFileStream.ReadUInt32();
            UInt32 dwUnknown0 = TFileStream.ReadUInt32();
            UInt32 dwUnknown1 = TFileStream.ReadUInt32();

            dwTableSize ^= 0x771;

            TFileStream.Seek(dwTableOffset, SeekOrigin.Begin);

            Byte[] lpTable = new Byte[dwTableSize];
            lpTable = TFileStream.ReadBytes((Int32)dwTableSize);
            lpTable = SM_Cipher.Decrypt(lpTable, (Int32)dwTableSize);

            using (MemoryStream TMemoryStream = new MemoryStream(lpTable))
            {
                for (Int32 i = 0; i < dwFiles; i++)
                {
                    String m_FileName = TMemoryStream.ReadString();
                    UInt32 dwUnknown2 = TMemoryStream.ReadUInt32();
                    UInt32 dwOffset = TMemoryStream.ReadUInt32();
                    UInt32 dwSize = TMemoryStream.ReadUInt32();
                    UInt32 dwCRC = TMemoryStream.ReadUInt32();
                    UInt64 dwFileTime = TMemoryStream.ReadUInt64();
                    UInt32 dwSmallCrypt = TMemoryStream.ReadUInt32();
                    UInt32 dwCompressed = TMemoryStream.ReadUInt32();
                    UInt32 dwZSize = TMemoryStream.ReadUInt32();

                    Console.WriteLine("[UNPACKING]: {0} SmallCrypt: {1} Compressed: {2} Size: {3} ZSize: {4} Offset: {5:X8}", m_FileName, Convert.ToBoolean(dwSmallCrypt) ? "Yes" : "No", Convert.ToBoolean(dwCompressed) ? "Yes" : "No", dwSize, dwZSize, dwOffset);

                    String m_FullPath = m_DstFolder + m_FileName.Replace("/", @"\");
                    Utils.iCreateDirectory(m_FullPath);

                    TFileStream.Seek(dwOffset, SeekOrigin.Begin);

                    if (dwCompressed == 1)
                    {
                        var lpBuffer = TFileStream.ReadBytes((Int32)dwZSize);
                        var lpOutBuffer = Zlib.iDecompress(lpBuffer, (Int32)dwZSize);
                        lpOutBuffer = SM_Cipher.iDecryptData(lpOutBuffer, (Int32)dwSize);
                        if (dwSmallCrypt == 1)
                        {
                            lpOutBuffer = SM_Cipher.iDecryptLittleBlock(lpOutBuffer);
                        }

                        File.WriteAllBytes(m_FullPath, lpOutBuffer);
                    }
                    else
                    {
                        var lpBuffer = TFileStream.ReadBytes((Int32)dwSize);
                        lpBuffer = SM_Cipher.iDecryptData(lpBuffer, (Int32)dwSize);
                        if (dwSmallCrypt == 1)
                        {
                            lpBuffer = SM_Cipher.iDecryptLittleBlock(lpBuffer);
                        }

                        File.WriteAllBytes(m_FullPath, lpBuffer);
                    }
                }
            }
        }
    }
}
