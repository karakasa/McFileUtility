using McFileIo.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    // BitArray always uses little-endian order.
    public class DynBitArray : IDynBitArray
    {
        private readonly BitArray _bits;

        public int Length { get; private set; }
        public int CellSize { get; }

        /// <summary>
        /// Create from an existing DynBitArray with a different cell size
        /// </summary>
        /// <param name="bitArray"></param>
        /// <param name="newCellSize"></param>
        public DynBitArray(IDynBitArray bitArray, int newCellSize) : this(newCellSize, bitArray.Length)
        {
            for (var i = 0; i < Length; i++)
                SetAt(i, bitArray[i]);
        }

        /// <summary>
        /// Create an empty DynBitArray.
        /// </summary>
        /// <param name="cellSize"></param>
        /// <param name="length"></param>
        public DynBitArray(int cellSize, int length)
        {
            _bits = new BitArray(cellSize * length);
            CellSize = cellSize;
            Length = length;
        }

        /// <summary>
        /// Create a DynBitArray based on data
        /// Recommended to use <see cref="CreateFromLongArray(long[])"/> as it creates more specialized versions.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="cellSize"></param>
        public DynBitArray(byte[] table, int cellSize)
        {
            if ((table.Length << 3) % cellSize != 0) throw new ArgumentException(nameof(cellSize));

            _bits = new BitArray(table);
            CellSize = cellSize;
            Length = _bits.Length / CellSize;
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
                var value = 0;
                var offset = index * CellSize;
                for (var i = 0; i < CellSize; i++)
                    value |= (_bits[offset++] ? 1 : 0) << i;
                return value;
            }
        }

        private void SetAt(int index, int value)
        {
            if (index < 0 || index >= Length) throw new ArgumentOutOfRangeException(nameof(index));

            unchecked
            {
                var offset = index * CellSize;
                for (var i = 0; i < CellSize; i++)
                {
                    var curBit = value & 1;
                    value >>= 1;
                    _bits[offset++] = curBit == 1;
                }
            }    
        }

        /// <summary>
        /// Create from a long array (used by latest Chunk section)
        /// The method will create a performance-specialized version if applicable
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static IDynBitArray CreateFromLongArray(long[] arr, int cellSize = 0)
        {
            if (cellSize == 0)
                cellSize = arr.Length >> 6;

            if (cellSize == 4)
            {
                return new DynBitArray4(arr);
            }
            else
            {
                var bytes = EndianHelper.LongArrayToBytes(arr);
                var bits = CreateFromByteArray(bytes, cellSize);
                return bits;
            }
        }

        public static IDynBitArray CreateFromByteArray(byte[] bytes, int cellSize = 0)
        {
            if (cellSize == 0)
                cellSize = bytes.Length >> 9;

            if (cellSize == 5)
            {
                var bits = new DynBitArray5(bytes);
                return bits;
            }
            else
            {
                var bits = new DynBitArray(bytes, cellSize);
                return bits;
            }
        }

        public static IDynBitArray CreateEmpty(int cellSize, int length)
        {
            if (cellSize == 4 && length % 16 == 0)
            {
                return new DynBitArray4(length);
            }
            else if (cellSize == 5)
            {
                return new DynBitArray5(length);
            }
            else
            {
                return new DynBitArray(cellSize, length);
            }
        }

        public static IDynBitArray CreateCloneFrom(int newCellSize, IDynBitArray dynArray)
        {
            if (dynArray.CellSize > newCellSize)
                throw new NotSupportedException($"{nameof(newCellSize)} is smaller than the current");

            var arr = CreateEmpty(newCellSize, dynArray.Length);
            for (var i = 0; i < dynArray.Length; i++)
                arr[i] = dynArray[i];
            return arr;
        }
    }
}
