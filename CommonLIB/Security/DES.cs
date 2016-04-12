using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace CommonLIB.Security
{
    public static class DES
    {
        static byte[] rgbKey = Encoding.UTF8.GetBytes("0dl8E5Lt");
        static byte[] rgbIV = Encoding.UTF8.GetBytes("RLD8iFGO");
        public static byte[] Encrypt(byte[] data)
        {
            try
            {
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                var enc=dCSP.CreateEncryptor(rgbKey, rgbIV);
                return enc.TransformFinalBlock(data, 0, data.Length);
            }
            catch
            {
                return null ;
            }

        }
        public static byte[] Decrypt(byte[] data)
        {
            try
            {
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                var enc = dCSP.CreateDecryptor(rgbKey, rgbIV);
                return enc.TransformFinalBlock(data, 0, data.Length);
            }
            catch
            {
                return null;
            }
        }
    }
}
