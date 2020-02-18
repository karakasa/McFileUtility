using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Utility
{
    /// <summary>
    /// A 6-7x faster implementation of DynBitArray when cellSize is 4 (half-byte)
    /// </summary>
    internal class DynBitArray4 : IDynBitArray
    {
        private long[] _longs;

        public void Clear()
        {
            _longs = null;
        }

        public DynBitArray4(int length)
        {
            if (length % 16 != 0) throw new NotSupportedException();
            _longs = new long[length >> 4];
        }

        public DynBitArray4(long[] dataSource)
        {
            _longs = dataSource;
        }

        public int Length => _longs.Length << 4;

        public int CellSize => 4;

        public int this[int index]
        {
            get
            {
                unchecked
                {
                    return (int)((_longs[index >> 4]) >> ((index & 15) << 2) & 0xf);
                }
            }
            set
            {
                unchecked
                {
                    _longs[index >> 4] = (_longs[index >> 4] & (-1 ^ (0xfL << ((index & 15) << 2))))
                    | (long)value << ((index & 15) << 2);
                }
            }
        }
    }
}
