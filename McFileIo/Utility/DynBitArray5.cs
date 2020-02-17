using McFileIo.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    // BitArray always uses little-endian order.
    internal class DynBitArray5 : IDynBitArray
    {
        private readonly BitArray _bits;

        public int Length { get; private set; }
        public int CellSize => 5;

        /// <summary>
        /// Create an empty DynBitArray.
        /// </summary>
        /// <param name="length"></param>
        public DynBitArray5(int length)
        {
            _bits = new BitArray(5 * length);
            Length = length;
        }

        /// <summary>
        /// Create a DynBitArray based on data
        /// Recommended to use <see cref="CreateFromLongArray(long[])"/> as it creates more specialized versions.
        /// </summary>
        /// <param name="table"></param>
        public DynBitArray5(byte[] table)
        {
            if ((table.Length << 3) % 5 != 0) throw new ArgumentException(nameof(CellSize));

            _bits = new BitArray(table);
            Length = _bits.Length / 5;
        }

        public int this[int index]
        {
            get => GetAt(index);
            set => SetAt(index, value);
        }

        private int GetAt(int index)
        {
            if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));

            unchecked
            {
                var offset = index * 5;
                return (_bits[offset] ? 1 : 0) | (_bits[offset + 1] ? 2 : 0) | (_bits[offset + 2] ? 4 : 0)
                    | (_bits[offset + 3] ? 8 : 0) | (_bits[offset + 4] ? 16 : 0);
            }
        }

        private void SetAt(int index, int value)
        {
            if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));

            unchecked
            {
                var offset = index * 5;
                _bits[offset] = 0 != (value & 1);
                _bits[offset + 1] = 0 != (value & 2);
                _bits[offset + 2] = 0 != (value & 4);
                _bits[offset + 3] = 0 != (value & 8);
                _bits[offset + 4] = 0 != (value & 16);
            }
        }

        /// <summary>
        /// Create from a long array (used by latest Chunk section)
        /// The method will create a performance-specialized version if applicable
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static IDynBitArray CreateFromLongArray(long[] arr)
        {
            if (arr.Length == 256)
            {
                return new DynBitArray4(arr);
            }
            else if (arr.Length == 320)
            {
                var bytes = EndianHelper.LongArrayToBytes(arr);
                var bits = new DynBitArray5(bytes);
                return bits;
            }
            else
            {
                var bytes = EndianHelper.LongArrayToBytes(arr);
                var bits = new DynBitArray(bytes, arr.Length >> 6);
                return bits;
            }
        }
    }
}
