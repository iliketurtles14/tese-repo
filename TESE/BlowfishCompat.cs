using TESE;
using System;
using System.Text;

namespace TESE
{
    public class BlowfishCompat
    {
        private readonly BlowFish blowfish;
        public BlowfishCompat(string key)
        {
            blowfish = new BlowFish(Encoding.ASCII.GetBytes(key));
        }
        private static byte[] ReverseByteOrder(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            int length = data.Length;
            if (length % 8 != 0)
            {
                length += 8 - length % 8;
                Array.Resize(ref data, length);
            }
            byte[] reversedBytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                reversedBytes[i + 3 - (i % 4 * 2)] = data[i];
            }
            return reversedBytes;
        }
        public byte[] Encrypt(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            byte[] encrypted = blowfish.Encrypt_ECB(ReverseByteOrder(data));
            return ReverseByteOrder(encrypted);
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            byte[] decrypted = blowfish.Decrypt_ECB(ReverseByteOrder(data));
            byte[] reverseDecrypted = ReverseByteOrder(decrypted);
            int i = reverseDecrypted.Length - 1;
            do
            {
                i--;
            } while (reverseDecrypted[i] == 0);
            byte[] temp = new byte[i + 1];
            Array.Copy(reverseDecrypted, temp, i + 1);
            return temp;
        }
    }
}
