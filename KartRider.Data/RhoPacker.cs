using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Ionic.Zlib;
using KartRider.IO.Packet;

namespace KartRider;

internal static class RhoParser
{
    public static uint Pack(string dirName, string rhoName, string rhoFileName, uint key)
    {
        OutPacket data;
        IEnumerable<RHOBlock> enumerable = BuildData(dirName, rhoName, key, out data);
        GC.Collect();
        uint num = RTTIHelper.GenerateRTTIHash(data.m_stream.ToArray());
        using (OutPacket outPacket = new OutPacket(64))
        {
            outPacket.WriteString("Rh layer spec 1.1", 32);
            outPacket.WriteString("", 32);
            using (OutPacket outPacket3 = new OutPacket(128))
            {
                uint num2 = 3131961357u;
                uint num3 = num2 ^ key;
                using (OutPacket outPacket2 = new OutPacket(124))
                {
                    outPacket2.WriteInt(65537);
                    outPacket2.WriteInt(enumerable.Count());
                    outPacket2.WriteUInt(num2);
                    outPacket2.WriteInt();
                    outPacket2.WriteInt();
                    outPacket2.WriteUInt(num);
                    outPacket2.WriteUInt(4229928824u);
                    outPacket2.WriteInt();
                    outPacket2.WriteBytes(new byte[124 - outPacket2.Length]);
                    outPacket3.WriteUInt(RTTIHelper.GenerateRTTIHash(outPacket2.ToArray()));
                    outPacket3.WriteBytes(outPacket2.ToArray());
                }
                byte[] array = new byte[outPacket3.Length];
                Buffer.BlockCopy(outPacket3.ToArray(), 0, array, 0, array.Length);
                RHOCrypto(array, key, encrypt: true);
                outPacket.WriteBytes(array);
                uint num4 = (uint)((outPacket.Length + enumerable.Count() * 32) / 256 + 1);
                foreach (RHOBlock item in enumerable)
                {
                    using (OutPacket outPacket4 = new OutPacket(32))
                    {
                        Console.WriteLine("Writing RHOBlock {0}: {1}", item.Index, item.DebugPath);
                        outPacket4.WriteUInt(item.Index);
                        outPacket4.WriteUInt(item.Offset + num4);
                        outPacket4.WriteUInt(item.CompressedSize);
                        outPacket4.WriteUInt(item.Size);
                        outPacket4.WriteUInt(item.Flag);
                        outPacket4.WriteUInt(item.CheckSum);
                        outPacket4.WriteLong(0);
                        byte[] array2 = outPacket4.ToArray();
                        RHOCrypto(array2, num3++, encrypt: true);
                        outPacket.WriteBytes(array2);
                    }
                }
                outPacket.WriteBytes(new byte[num4 * 256 - outPacket.Length]);
                outPacket.CopyFrom(data);
                if (outPacket.Length % 256 != 0)
                {
                    outPacket.WriteBytes(new byte[256 - outPacket.Length % 256]);
                }
            }
            GC.Collect();
            File.WriteAllBytes(dirName + ".rho", outPacket.m_stream.ToArray());
        }
        data.Dispose();
        GC.Collect();
        return num;
    }

    private static IEnumerable<RHOBlock> BuildData(string path, string rhoName, uint rhoMasterKey, out OutPacket data)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        List<FileInfo> list = new List<FileInfo>(directoryInfo.EnumerateFiles("*", (!(rhoName == "")) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        List<DirectoryInfo> list2 = new List<DirectoryInfo>();
        if (rhoName != "")
        {
            list2.AddRange(directoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories));
        }
        list.Sort(new CustomStringComparerFile());
        list2.Insert(0, directoryInfo);
        list2.Sort(new CustomStringComparerDir());
        List<uint> list3 = new List<uint>();
        List<RHOBlock> list4 = new List<RHOBlock>();
        data = new OutPacket(64);
        RHOBlock rHOBlock = new RHOBlock();
        rHOBlock.DebugPath = "dummy_padding";
        rHOBlock.Index = 4294967294u;
        rHOBlock.Flag = 0u;
        rHOBlock.CheckSum = 0u;
        rHOBlock.Offset = (uint)(data.Length / 256);
        rHOBlock.CompressedSize = 8u;
        rHOBlock.Size = 8u;
        data.WriteBytes(new byte[256]);
        list4.Add(rHOBlock);
        foreach (DirectoryInfo item in list2)
        {
            RHOBlock rHOBlock2 = new RHOBlock();
            string fullName = item.FullName;
            uint num = (!(fullName == directoryInfo.FullName)) ? CRC32.Compute(fullName) : uint.MaxValue;
            if (list3.Contains(num))
            {
                Debugger.Break();
            }
            list3.Add(num);
            uint key = rhoMasterKey + 630434289;
            rHOBlock2.DebugPath = fullName;
            rHOBlock2.Index = num;
            rHOBlock2.Flag = 5u;
            rHOBlock2.CheckSum = 0u;
            rHOBlock2.Offset = (uint)(data.Length / 256);
            OutPacket outPacket = new OutPacket(64);
            if (rhoName != "")
            {
                List<DirectoryInfo> list5 = new List<DirectoryInfo>(item.GetDirectories());
                list5.Sort(new CustomStringComparerDir());
                outPacket.WriteInt(list5.Count);
                foreach (DirectoryInfo item2 in list5)
                {
                    outPacket.WriteNullTerminatedString(item2.Name);
                    outPacket.WriteUInt(CRC32.Compute(item2.FullName));
                }
            }
            else
            {
                outPacket.WriteInt();
            }
            List<FileInfo> list6 = new List<FileInfo>(item.GetFiles());
            list6.Sort(new CustomStringComparerFile());
            outPacket.WriteInt(list6.Count);
            foreach (FileInfo item3 in list6)
            {
                outPacket.WriteNullTerminatedString(Path.GetFileNameWithoutExtension(item3.Name));
                string text = Path.GetExtension(item3.Name).Substring(1);
                if (text.Length > 4)
                {
                    throw new Exception("Cannot encode extension '" + text + "'; must be 4 chars or less");
                }
                for (int i = 0; i < 4; i++)
                {
                    if (text.Length > i)
                    {
                        outPacket.WriteByte((byte)text[i]);
                    }
                    else
                    {
                        outPacket.WriteByte(0);
                    }
                }
                outPacket.WriteInt(GetTypeByExtension(text, (int)item3.Length));
                outPacket.WriteUInt(CRC32.Compute(item3.FullName));
                outPacket.WriteUInt((uint)item3.Length);
            }
            int num3 = (int)(rHOBlock2.Size = (rHOBlock2.CompressedSize = (uint)outPacket.Length));
            if ((rHOBlock2.Flag & 1) != 0)
            {
                rHOBlock2.CheckSum = RTTIHelper.GenerateRTTIHash(outPacket.ToArray());
            }
            if ((rHOBlock2.Flag & 8) != 0)
            {
                byte[] array = outPacket.ToArray();
                DecryptKR2Crypto(array, key);
                outPacket.Reset(0);
                outPacket.WriteBytes(array);
            }
            if ((rHOBlock2.Flag & 4) != 0)
            {
                byte[] array2 = outPacket.ToArray();
                DecryptKRCrypto(array2, key);
                outPacket.Reset(0);
                outPacket.WriteBytes(array2);
            }
            if ((rHOBlock2.Flag & 2) != 0)
            {
                byte[] array3 = outPacket.ToArray();
                byte[] array4 = CompressZLib(array3);
                if (array4.Length < array3.Length)
                {
                    outPacket.Dispose();
                    outPacket = new OutPacket(64);
                    outPacket.WriteBytes(array4);
                    rHOBlock2.CompressedSize = (uint)outPacket.Length;
                }
                else
                {
                    rHOBlock2.Flag -= 2u;
                }
            }
            int length2 = outPacket.Length;
            if (length2 % 256 != 0)
            {
                outPacket.WriteBytes(new byte[256 - length2 % 256]);
            }
            data.WriteBytes(outPacket.m_stream.ToArray());
            outPacket.Dispose();
            list4.Add(rHOBlock2);
        }
        foreach (FileInfo item4 in list)
        {
            RHOBlock rHOBlock3 = new RHOBlock();
            rHOBlock3.DebugPath = item4.FullName;
            rHOBlock3.Index = CRC32.Compute(item4.FullName);
            rHOBlock3.Flag = 7u;
            rHOBlock3.CheckSum = 0u;
            rHOBlock3.Offset = (uint)(data.Length / 256);
            long length3 = item4.Length;
            uint key2 = 0u;
            if (list3.Contains(rHOBlock3.Index))
            {
                Debugger.Break();
            }
            list3.Add(rHOBlock3.Index);
            rHOBlock3.CompressedSize = (uint)length3;
            rHOBlock3.Size = (uint)length3;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item4.Name);
            string text2 = Path.GetExtension(item4.Name).Substring(1);
            int typeByExtension = GetTypeByExtension(text2, (int)length3);
            bool flag = typeByExtension == 5;
            if (typeByExtension == 3 || typeByExtension == 4 || typeByExtension == 5 || typeByExtension == 6)
            {
                byte[] array5 = Encoding.ASCII.GetBytes(text2);
                Array.Resize(ref array5, 4);
                key2 = RTTIHelper.GenerateRTTIHash(fileNameWithoutExtension) + BitConverter.ToUInt32(array5, 0) + rhoMasterKey - 1970136660;
            }
            using (FileStream fileStream = item4.OpenRead())
            {
                OutPacket outPacket2 = new OutPacket(64);
                if (flag)
                {
                    byte[] array6 = new byte[256];
                    fileStream.Read(array6, 0, 256);
                    outPacket2.WriteBytes(array6);
                    rHOBlock3.Size = 256u;
                    rHOBlock3.CompressedSize = 256u;
                    rHOBlock3.Flag = 5u;
                }
                else
                {
                    outPacket2.CopyFrom(fileStream);
                }
                if ((rHOBlock3.Flag & 1) != 0)
                {
                    rHOBlock3.CheckSum = RTTIHelper.GenerateRTTIHash(outPacket2.ToArray());
                }
                if ((rHOBlock3.Flag & 8) != 0)
                {
                    byte[] array7 = outPacket2.ToArray();
                    DecryptKR2Crypto(array7, key2);
                    outPacket2.Reset(0);
                    outPacket2.WriteBytes(array7);
                }
                if ((rHOBlock3.Flag & 4) != 0)
                {
                    byte[] array8 = outPacket2.ToArray();
                    DecryptKRCrypto(array8, key2);
                    outPacket2.Reset(0);
                    outPacket2.WriteBytes(array8);
                }
                if ((rHOBlock3.Flag & 2) != 0)
                {
                    byte[] array9 = outPacket2.ToArray();
                    byte[] array10 = CompressZLib(array9);
                    if (array10.Length < array9.Length)
                    {
                        outPacket2.Dispose();
                        outPacket2 = new OutPacket(64);
                        outPacket2.WriteBytes(array10);
                        rHOBlock3.CompressedSize = (uint)outPacket2.Length;
                    }
                    else
                    {
                        rHOBlock3.Flag -= 2u;
                    }
                }
                data.WriteBytes(outPacket2.m_stream.ToArray());
                outPacket2.Dispose();
            }
            if (rHOBlock3.CompressedSize % 256u != 0)
            {
                data.WriteBytes(new byte[256 - rHOBlock3.CompressedSize % 256u]);
            }
            list4.Add(rHOBlock3);
            if (flag)
            {
                RHOBlock rHOBlock4 = new RHOBlock();
                rHOBlock4.DebugPath = item4.FullName;
                rHOBlock4.Index = rHOBlock3.Index + 1;
                rHOBlock4.Flag = 3u;
                rHOBlock4.CheckSum = 0u;
                rHOBlock4.Offset = (uint)(data.Length / 256);
                length3 = item4.Length - 256;
                if (list3.Contains(rHOBlock4.Index))
                {
                    Debugger.Break();
                }
                list3.Add(rHOBlock4.Index);
                rHOBlock4.CompressedSize = (uint)length3;
                rHOBlock4.Size = (uint)length3;
                using (FileStream fileStream2 = item4.OpenRead())
                {
                    OutPacket outPacket3 = new OutPacket(64);
                    byte[] array11 = new byte[length3];
                    fileStream2.Seek(256L, SeekOrigin.Begin);
                    fileStream2.Read(array11, 0, (int)length3);
                    outPacket3.WriteBytes(array11);
                    if ((rHOBlock4.Flag & 1) != 0)
                    {
                        rHOBlock4.CheckSum = RTTIHelper.GenerateRTTIHash(outPacket3.ToArray());
                    }
                    if ((rHOBlock4.Flag & 2) != 0)
                    {
                        byte[] array12 = outPacket3.ToArray();
                        byte[] array13 = CompressZLib(array12);
                        if (array13.Length < array12.Length)
                        {
                            outPacket3.Dispose();
                            outPacket3 = new OutPacket(64);
                            outPacket3.WriteBytes(array13);
                            rHOBlock4.CompressedSize = (uint)outPacket3.Length;
                        }
                        else
                        {
                            rHOBlock4.Flag -= 2u;
                        }
                    }
                    data.WriteBytes(outPacket3.m_stream.ToArray());
                    outPacket3.Dispose();
                }
                if (rHOBlock4.CompressedSize % 256u != 0)
                {
                    data.WriteBytes(new byte[256 - rHOBlock4.CompressedSize % 256u]);
                }
                list4.Add(rHOBlock4);
            }
        }
        if (data.Length % 256 != 0)
        {
            data.WriteBytes(new byte[256 - data.Length % 256]);
        }
        return list4;
    }

    public static byte[] DecryptKRCrypto(byte[] data, uint key)
    {
        int num = 0;
        uint[] array = new uint[17];
        byte[] array2 = new byte[68];
        array[0] = (uint)((int)key ^ -2072773695);
        for (num = 1; num < 16; num++)
        {
            array[num] = array[num - 1] - 2072773695;
        }
        for (num = 0; num <= 16; num++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(array[num]), 0, array2, num * 4, 4);
        }
        for (num = 0; num + 64 <= data.Length; num += 64)
        {
            for (int i = 0; i < 16; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(array[i] ^ BitConverter.ToUInt32(data, num + 4 * i)), 0, data, num + 4 * i, 4);
            }
        }
        for (int j = num; j < data.Length; j++)
        {
            data[j] = (byte)(data[j] ^ array2[j - num]);
        }
        return data;
    }

    public static byte[] DecryptKR2Crypto(byte[] data, uint key)
    {
        int num = 0;
        uint[] array = new uint[33];
        byte[] array2 = new byte[132];
        array[0] = (key ^ 0x3746CA8F);
        for (num = 1; num < 16; num++)
        {
            array[num] = array[num - 1] + 666576812;
        }
        for (; num < 32; num++)
        {
            array[num] = 701442637 * array[num - 1];
        }
        for (num = 0; num < 33; num++)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(array[num]), 0, array2, num * 4, 4);
        }
        for (num = 0; num + 128 <= data.Length; num += 128)
        {
            for (int i = 0; i < 32; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(array[i] ^ BitConverter.ToUInt32(data, num + 4 * i)), 0, data, num + 4 * i, 4);
            }
        }
        for (int j = num; j < data.Length; j++)
        {
            data[j] = (byte)(data[j] ^ array2[j - num]);
        }
        return data;
    }

    public static uint GetKey(uint counter)
    {
        byte[] bytes = BitConverter.GetBytes(counter);
        return Constants.ThirdByte_Keys[bytes[3]] ^ Constants.SecondByte_Keys[bytes[2]] ^ Constants.FirstByte_Keys[bytes[1]] ^ Constants.ZeroByte_Keys[bytes[0]];
    }

    public static uint RHOCrypto(byte[] data, uint seed, bool encrypt)
    {
        uint num = 0u;
        int i = 0;
        for (int num2 = data.Length / 4; i < 4 * num2; i += 4)
        {
            uint num3 = BitConverter.ToUInt32(data, i);
            uint num4 = num ^ num3;
            uint key = GetKey(seed);
            seed++;
            uint num5 = key ^ num4;
            Buffer.BlockCopy(BitConverter.GetBytes(num5), 0, data, i, 4);
            num = ((!encrypt) ? (num + num5) : (num + num3));
        }
        if (i < data.Length)
        {
            uint key2 = GetKey(seed);
            byte[] bytes = BitConverter.GetBytes(key2);
            byte[] bytes2 = BitConverter.GetBytes(num);
            uint num6 = 0u;
            uint result = 0u;
            while (i < data.Length)
            {
                data[i] = (byte)(bytes[num6] ^ bytes2[num6] ^ data[i]);
                i++;
                result = num6++ + 1;
            }
            return result;
        }
        return 0u;
    }

    private static List<string> GetAllFiles(string path)
    {
        List<string> list = new List<string>();
        list.AddRange(Directory.GetFiles(path));
        string[] directories = Directory.GetDirectories(path);
        foreach (string path2 in directories)
        {
            list.AddRange(GetAllFiles(path2));
        }
        return list;
    }

    public static int GetTypeByExtension(string ext, int fileSize)
    {
        switch (ext)
        {
            case "1s":
            case "dds":
            case "tga":
            case "bmh":
            case "bmx":
            case "f30":
            case "hdr":
            case "fft":
            case "wav":
                return 1;
            case "uset":
            case "xml":
                return 3;
            case "png":
                return (fileSize <= 256) ? 4 : 5;
            case "kap":
            case "ogg":
            case "jpg":
            case "flac":
            case "ksv":
                return 5;
            case "bml":
                return 6;
            default:
                Console.WriteLine("Warning; unknown extension : " + ext);
                return 1;
        }
    }

    public static int wcscmp(string input, string candidate)
    {
        if (input == candidate)
        {
            return 0;
        }
        Encoding unicode = Encoding.Unicode;
        byte[] array = unicode.GetBytes(input);
        Array.Resize(ref array, array.Length + 2);
        byte[] array2 = unicode.GetBytes(candidate);
        Array.Resize(ref array2, array2.Length + 2);
        int num = (((array.Length <= array2.Length) ? array.Length : array2.Length) - 2) / 2;
        int i;
        for (i = 0; i < num && BitConverter.ToUInt16(array, i * 2) == BitConverter.ToUInt16(array2, i * 2); i++)
        {
        }
        ushort num2 = BitConverter.ToUInt16(array, i * 2);
        ushort num3 = BitConverter.ToUInt16(array2, i * 2);
        return num2 - num3;
    }

    public static byte[] CompressZLib(byte[] data)
    {
        using (MemoryStream memoryStream2 = new MemoryStream(data))
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZlibStream zlibStream = new ZlibStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestSpeed))
                {
                    memoryStream2.CopyTo(zlibStream);
                    zlibStream.Flush();
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
