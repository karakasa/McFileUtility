using McFileIo.Misc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace McFileIo.Utility
{
    internal static class StringUtility
    {
        public static string RemoveVanillaNamespace(string id)
        {
            if (id.StartsWith("minecraft:"))
                return id.Substring(10);
            return id;
        }

        public static Uuid JavaUuidFromBytes(byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(bytes);
                hash[6] &= 0x0f;
                hash[6] |= 0x30;
                hash[8] &= 0x3f;
                hash[8] |= 0x80;

                return new Uuid()
                {
                    Least = BitConverter.ToInt64(hash, 0),
                    Most = BitConverter.ToInt64(hash, 8)
                };
            }
        }
    }
}
