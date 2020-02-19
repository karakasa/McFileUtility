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

        /// <summary>
        /// Read a big-endian UInt32 from Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static uint ToUInt32(Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();

            unchecked
            {
                return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
            }
        }

        public static byte[] ReadToArray(this Stream stream, int length = 4)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        public static int GetHalfInt(byte[] buffer, int index)
        {
            return ((index & 1) == 0) ? buffer[index >> 1] & 0xf : (buffer[index >> 1] >> 4) & 0xf; 
        }

        public static void SetHalfInt(byte[] buffer, int index, int value)
        {
            unchecked
            {
                if ((index & 1) == 0)
                {
                    buffer[index >> 1] = (byte)(((byte)(buffer[index >> 1] & 0xf0)) | ((byte)(value & 0xf)));
                }
                else
                {
                    buffer[index >> 1] = (byte)(((byte)(buffer[index >> 1] & 0xf)) | ((byte)((value & 0xf) << 4)));
                }
            }
            
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
            var bytes = new byte[array.Length << 3];
            for (var i = 0; i < array.Length; i++)
            {
                var data = BitConverter.GetBytes(array[i]);
                Array.Copy(data, 0, bytes, i << 3, 8);
            }
            return bytes;
        }

        public static long[] BytesToLongArray(byte[] array)
        {
            var longs = new long[array.Length >> 3];

            for (var i = 0; i < longs.Length; i++)
            {
                longs[i] = BitConverter.ToInt64(array, i << 3);
            }
            return longs;
        }
    }
}
