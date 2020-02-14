using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockCollection<T>
    {
        IEnumerable<(int x, int y, int z, T block)> AllBlocks();
        T GetBlock(int x, int y, int z);
    }
}
