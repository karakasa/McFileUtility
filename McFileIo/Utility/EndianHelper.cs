using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace McFileIo.Utility
{
    public static class EndianHelper
    {
        public static int ToInt32(byte[] data)
        {
            return BitConverter.ToInt32(BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data, 0);
        }

        public static uint ToUInt32(byte[] data)
        {
            return BitConverter.ToUInt32(BitConverter.IsLittleEndian ? data.Reverse().ToArray() : data, 0);
        }

        public static byte[] ReadToArray(this Stream stream, int length = 4)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        public static int GetHalfInt(byte[] buffer, int index)
        {
            return ((index & 1) == 0) ? buffer[index >> 1] & 15 : (buffer[index >> 1] >> 4) & 15; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHalfIntOddIndex(byte[] buffer, int index)
        {
            return (buffer[index >> 1] >> 4) & 15;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHalfIntEvenIndex(byte[] buffer, int index)
        {
            return buffer[index >> 1] & 15;
        }

        public static byte[] LongArrayToBytes(long[] array)
        {
            var bytes = new byte[array.Length * 8];
            for (var i = 0; i < array.Length; i++)
            {
                var data = BitConverter.GetBytes(array[i]);
                Array.Copy(data, 0, bytes, i * 8, 8);
            }
            return bytes;
        }
    }
}
