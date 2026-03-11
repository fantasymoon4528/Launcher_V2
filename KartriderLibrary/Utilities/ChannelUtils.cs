using KartRider.IO.Packet;
using System;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace KartRider.Common.Utilities
{
    internal class ChannelUtils
    {
        public static uint GenerateAdler32(byte[] str, uint a1 = 0U)
        {
            int num = 0;
            uint num2 = (uint)str.Length;
            uint num3 = a1 >> 16;
            uint num4 = (uint)((ushort)a1);
            bool flag = str.Length == 1;
            uint result;
            if (flag)
            {
                int num5 = (int)((uint)str[0] + num4);
                bool flag2 = num5 >= 65521;
                if (flag2)
                {
                    num5 -= 65521;
                }
                int num6 = (int)((long)num5 + (long)((ulong)num3));
                bool flag3 = num6 >= 65521;
                if (flag3)
                {
                    num6 -= 65521;
                }
                result = (uint)(num5 | num6 << 16);
            }
            else
            {
                bool flag5 = str.Length >= 16;
                if (flag5)
                {
                    bool flag6 = str.Length >= 5552;
                    if (flag6)
                    {
                        uint num7 = (uint)(str.Length / 5552);
                        do
                        {
                            num2 -= 5552U;
                            int num8 = 347;
                            do
                            {
                                int num9 = (int)((uint)str[num] + num4);
                                int num10 = num9 + (int)num3;
                                int num11 = (int)str[num + 1] + num9;
                                int num12 = num11 + num10;
                                int num13 = (int)str[num + 2] + num11;
                                int num14 = num13 + num12;
                                int num15 = (int)str[num + 3] + num13;
                                int num16 = num15 + num14;
                                int num17 = (int)str[num + 4] + num15;
                                int num18 = num17 + num16;
                                int num19 = (int)str[num + 5] + num17;
                                int num20 = num19 + num18;
                                int num21 = (int)str[num + 6] + num19;
                                int num22 = num21 + num20;
                                int num23 = (int)str[num + 7] + num21;
                                int num24 = num23 + num22;
                                int num25 = (int)str[num + 8] + num23;
                                int num26 = num25 + num24;
                                int num27 = (int)str[num + 9] + num25;
                                int num28 = num27 + num26;
                                int num29 = (int)str[num + 10] + num27;
                                int num30 = num29 + num28;
                                int num31 = (int)str[num + 11] + num29;
                                int num32 = num31 + num30;
                                int num33 = (int)str[num + 12] + num31;
                                int num34 = num33 + num32;
                                int num35 = (int)str[num + 13] + num33;
                                int num36 = num35 + num34;
                                int num37 = (int)str[num + 14] + num35;
                                int num38 = num37 + num36;
                                num4 = (uint)((int)str[num + 15] + num37);
                                num3 = num4 + (uint)num38;
                                num += 16;
                                num8--;
                            }
                            while (num8 != 0);
                            num4 %= 65521U;
                            num7 -= 1U;
                            num3 %= 65521U;
                        }
                        while (num7 > 0U);
                    }
                    bool flag7 = num2 > 0U;
                    if (flag7)
                    {
                        bool flag8 = num2 >= 16U;
                        if (flag8)
                        {
                            uint num39 = num2 >> 4;
                            do
                            {
                                int num40 = (int)((uint)str[num] + num4);
                                int num41 = num40 + (int)num3;
                                int num42 = (int)str[num + 1] + num40;
                                int num43 = num42 + num41;
                                int num44 = (int)str[num + 2] + num42;
                                int num45 = num44 + num43;
                                int num46 = (int)str[num + 3] + num44;
                                int num47 = num46 + num45;
                                int num48 = (int)str[num + 4] + num46;
                                int num49 = num48 + num47;
                                int num50 = (int)str[num + 5] + num48;
                                int num51 = num50 + num49;
                                int num52 = (int)str[num + 6] + num50;
                                int num53 = num52 + num51;
                                int num54 = (int)str[num + 7] + num52;
                                int num55 = num54 + num53;
                                int num56 = (int)str[num + 8] + num54;
                                int num57 = num56 + num55;
                                int num58 = (int)str[num + 9] + num56;
                                int num59 = num58 + num57;
                                int num60 = (int)str[num + 10] + num58;
                                int num61 = num60 + num59;
                                int num62 = (int)str[num + 11] + num60;
                                int num63 = num62 + num61;
                                int num64 = (int)str[num + 12] + num62;
                                int num65 = num64 + num63;
                                int num66 = (int)str[num + 13] + num64;
                                int num67 = num66 + num65;
                                int num68 = (int)str[num + 14] + num66;
                                int num69 = num68 + num67;
                                num4 = (uint)((int)str[num + 15] + num68);
                                num2 -= 16U;
                                num3 = num4 + (uint)num69;
                                num += 16;
                                num39 -= 1U;
                            }
                            while (num39 > 0U);
                        }
                        while (num2 > 0U)
                        {
                            num4 += (uint)str[num++];
                            num3 += num4;
                            num2 -= 1U;
                        }
                        num4 %= 65521U;
                        num3 %= 65521U;
                    }
                    result = (num4 | num3 << 16);
                }
                else
                {
                    bool flag9 = str.Length != 0;
                    if (flag9)
                    {
                        do
                        {
                            num4 += (uint)str[num++];
                            num3 += num4;
                            num2 -= 1U;
                        }
                        while (num2 > 0U);
                    }
                    bool flag10 = num4 >= 65521U;
                    if (flag10)
                    {
                        num4 -= 65521U;
                    }
                    result = (num4 | num3 % 65521U << 16);
                }

            }
            return result;
        }

        public static int GenerateSimpleAdler32(byte[] bytes)
        {
            uint num = 1U;
            uint num2 = 0U;
            foreach (byte b in bytes)
            {
                num = (num + (uint)b) % 65521U;
                num2 = (num2 + num) % 65521U;
            }
            return (int)((num2 << 16) + num);
        }

        public enum EncodeFlag : byte
        {
            ZLib = 1,
            KartCrypto
        }

        public static byte[] Encode(byte[] inputBytes, EncodeFlag flag)
        {
            byte[] result;
            OutPacket outPacket = new OutPacket();
            {
                outPacket.WriteByte(83);
                outPacket.WriteByte((byte)flag);
                outPacket.WriteUInt(GenerateAdler32(inputBytes));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                    {
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            using (DeflateStream deflateStream = new DeflateStream(memoryStream2, CompressionMode.Compress))
                            {
                                using (MemoryStream memoryStream3 = new MemoryStream(inputBytes))
                                {
                                    memoryStream3.CopyTo(deflateStream);
                                }
                                deflateStream.Close();
                                outPacket.WriteInt(inputBytes.Length);
                                byte[] buffer = memoryStream2.ToArray();
                                binaryWriter.Write(new byte[]
                                {
                                    120,
                                    218
                                });
                                binaryWriter.Write(buffer);
                                binaryWriter.Write(IPAddress.HostToNetworkOrder(GenerateSimpleAdler32(inputBytes)));
                                inputBytes = memoryStream.ToArray();
                            }
                        }
                    }
                }
                outPacket.WriteBytes(inputBytes);
                result = outPacket.ToArray();
            }
            return result;
        }
    }
}