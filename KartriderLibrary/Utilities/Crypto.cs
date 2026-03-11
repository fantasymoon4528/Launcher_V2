using System;

namespace KartRider.Common.Utilities
{
    internal class Crypto
    {
        public static byte[] HexToByte(string hex)
        {
            hex = hex.Replace(" ", "");
            byte[] convert = new byte[hex.Length / 2];
            int length = convert.Length;
            for (int i = 0; i < length; i++)
            {
                convert[i] = byte.Parse(hex.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return convert;
        }
        public static byte[] EncryptCryptoKey = HexToByte("BA7C70892116B0BDB541A257FF20F5EFAC7FD454293C82AADA4E9847CB5B51345C92F0ED563F77536A1E1805021F08FB94C161714BCDC48445375E27EAB74AF1C909B386B14D69D7CEC07E493AA435005932A86DE2AF6FA1EB9E1D10BCFAE5AD6C68D62D48D365ABC675E04C06DF2ACA6644DDC8138E76FD0D39EE5011A70E935862F6F7E8259DD2960B4381A030903DA35DD564D9B22B6742C50CE78DB8F388319C8FDE83B4BB196B9B3BD074040AE9607228BE2438037B8A85E3A6F9126EF8AE5299334F55DC9A017AC72EF21B790F265F78978B95911CFC73F42C15D840E6B91AEC9F36075A23CFCCDB8CC387E4173EA9C22F632280A57DE1FE14D1B646BF");
        public static byte[] encryptBytes(byte[] original)
        {
            byte[] array = new byte[original.Length];
            int offset = 0;
            while (offset < original.Length)
            {
                array[offset] = EncryptCryptoKey[(offset + original[offset]) % byte.MaxValue];
                offset++;
            }
            return array;
        }
        public static byte[] DecrypotCryptoKey = HexToByte("4FC82CB6AD2B6CE52E41AE899A787ECF5B7CBD74FBDC05EF2AA7E1CDD75A292D0D04F5E7B485D03BB2146E96DB63CBF38DA051C31F4EE439B5794CAA158FF025DE09988A7138FE1B644B3E346B4519C47B1EC12713C5240B8050E61D20913AD1B03281F493667097614628A86053BE560233B1D9AC697626D2CEC9B701F84A11F68B16A437B943ED9F03B8D4EB9C75A28ED6217F30D588D31AC2C7A9A18659E38C570A904DF7BB7D52F11767105FC05506449542A508FD3D9DE000A65C07B3FF4931F2EC369968CA73406F1CE93548E8ABFC876512926247DD9418EAC672A36D6AF954BAEE5EDF9B84AF3C58E2237A0F223FCC9EDA0E8283BFBC5D2FD877FA0C");
        public static byte[] DecryptBytes(byte[] original)
        {
            byte[] array = new byte[original.Length];
            int offset = 0;
            while (offset < original.Length)
            {
                array[offset] = (byte)(DecrypotCryptoKey[original[offset]] - offset);
                offset++;
            }
            return array;
        }
        public static uint HashDecrypt(byte[] pData, int nLength, uint nKey)
        {
            uint num = nKey ^ 347277256U;
            uint num2 = nKey ^ 2361332396U;
            uint num3 = nKey ^ 604215233U;
            uint num4 = nKey ^ 4089260480U;
            int length = 0;
            uint hash = 0U;
            int i = 0;
            while ((long)i < (long)((ulong)(nLength >> 4)))
            {
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, length) ^ num), 0, pData, length, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, length + 4) ^ num2), 0, pData, length + 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, length + 8) ^ num3), 0, pData, length + 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, length + 12) ^ num4), 0, pData, length + 12, 4);
                hash ^= (BitConverter.ToUInt32(pData, length + 12) ^ BitConverter.ToUInt32(pData, length + 8) ^ BitConverter.ToUInt32(pData, length + 4) ^ BitConverter.ToUInt32(pData, length));
                length += 16;
                i++;
            }
            i *= 16;
            length = 0;
            byte[] bytes = BitConverter.GetBytes(num);
            byte[] bytes2 = BitConverter.GetBytes(num2);
            byte[] bytes3 = BitConverter.GetBytes(num3);
            byte[] bytes4 = BitConverter.GetBytes(num4);
            byte[] array = new byte[16];
            Buffer.BlockCopy(bytes, 0, array, 0, 4);
            Buffer.BlockCopy(bytes2, 0, array, 4, 4);
            Buffer.BlockCopy(bytes3, 0, array, 8, 4);
            Buffer.BlockCopy(bytes4, 0, array, 12, 4);
            while ((long)i < (long)((ulong)nLength))
            {
                int num8 = i;
                pData[num8] ^= array[length];
                hash ^= (uint)((uint)pData[i] << length);
                i++;
                length++;
            }
            return hash;
        }

        public static uint HashEncrypt(byte[] pData, int nLength, uint nKey)
        {
            uint num = nKey ^ 347277256U;
            uint num2 = nKey ^ 2361332396U;
            uint num3 = nKey ^ 604215233U;
            uint num4 = nKey ^ 4089260480U;
            int num5 = 0;
            uint num6 = 0U;
            int num7 = 0;
            while (num7 < nLength >> 4)
            {
                num6 ^= (BitConverter.ToUInt32(pData, num5 + 12) ^ BitConverter.ToUInt32(pData, num5 + 8) ^ BitConverter.ToUInt32(pData, num5 + 4) ^ BitConverter.ToUInt32(pData, num5));
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, num5) ^ num), 0, pData, num5, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, num5 + 4) ^ num2), 0, pData, num5 + 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, num5 + 8) ^ num3), 0, pData, num5 + 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(BitConverter.ToUInt32(pData, num5 + 12) ^ num4), 0, pData, num5 + 12, 4);
                num5 += 16;
                num7++;
            }
            num7 *= 16;
            num5 = 0;
            byte[] bytes = BitConverter.GetBytes(num);
            byte[] bytes2 = BitConverter.GetBytes(num2);
            byte[] bytes3 = BitConverter.GetBytes(num3);
            byte[] bytes4 = BitConverter.GetBytes(num4);
            byte[] array = new byte[16];
            Buffer.BlockCopy(bytes, 0, array, 0, 4);
            Buffer.BlockCopy(bytes2, 0, array, 4, 4);
            Buffer.BlockCopy(bytes3, 0, array, 8, 4);
            Buffer.BlockCopy(bytes4, 0, array, 12, 4);
            while ((long)num7 < nLength)
            {
                num6 ^= (uint)((uint)pData[num7] << num5);
                int num8 = num7;
                pData[num8] ^= array[num5];
                num7++;
                num5++;
            }
            return num6;
        }
        public static byte[] ApplyCrypto(byte[] input, uint key)
        {
            byte[] array = new byte[input.Length];
            Buffer.BlockCopy(input, 0, array, 0, input.Length);
            uint[] array2 = new uint[17];
            byte[] array3 = new byte[68];
            array2[0] = key ^ 0x8473FBC1;
            int i;
            for (i = 1; i < 16; i++)
            {
                array2[i] = array2[i - 1] - 0x7B8C043F;
            }
            for (i = 0; i <= 16; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(array2[i]), 0, array3, i * 4, 4);
            }
            i = 0;
            while (i + 64 <= array.Length)
            {
                for (int j = 0; j < 16; j++)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(array2[j] ^ BitConverter.ToUInt32(array, i + 4 * j)), 0, array, i + 4 * j, 4);
                }
                i += 64;
            }
            for (int k = i; k < array.Length; k++)
            {
                array[k] ^= array3[k - i];
            }
            return array;
        }
    }
}