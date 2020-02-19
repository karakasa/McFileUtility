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
        internal long[] InternalData { get; private set; }

        public void Clear()
        {
            InternalData = null;
        }

        public DynBitArray4(int length)
        {
            if (length % 16 != 0) throw new NotSupportedException();
            InternalData = new long[length >> 4];
        }

        public DynBitArray4(long[] dataSource)
        {
            InternalData = dataSource;
        }

        public int Length => InternalData.Length << 4;

        public int CellSize => 4;

        public int this[int index]
        {
            get
            {
                unchecked
                {
                    return (int)((InternalData[index >> 4]) >> ((index & 15) << 2) & 0xf);
                }
            }
            set
            {
                unchecked
                {
                    InternalData[index >> 4] = (InternalData[index >> 4] & (-1 ^ (0xfL << ((index & 15) << 2))))
                    | (long)value << ((index & 15) << 2);
                }
            }
        }
    }
}
