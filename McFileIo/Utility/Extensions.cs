using fNbt;
using McFileIo.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.Utility
{
    public static class Extensions
    {
        public static NbtCompression ToNbtCompression(this ChunkCompressionType type)
        {
            if (type == ChunkCompressionType.GZip) return NbtCompression.GZip;
            if (type == ChunkCompressionType.ZLib) return NbtCompression.ZLib;
            return NbtCompression.AutoDetect;
        }

        /// <summary>
        /// Deep-copy a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(this List<T> list) where T : ICloneable
        {
            return new List<T>(list.Select(e => (T)e.Clone()));
        }

        public static byte[] ToBytes(this BitArray bits)
        {
            if (bits.Length == 0) return new byte[0];

            var ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
    }
}
