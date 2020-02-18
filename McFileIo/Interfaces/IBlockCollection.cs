using McFileIo.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockCollection<T>
    {
        void SetBlock(IEnumerable<ChangeBlockRequest> requests, T[] customPalette);
        void SetBlock(IEnumerable<ChangeBlockRequest> requests, IList<T> customPalette);
        void SetBlock(int X, int Y, int Z, T block);
        IEnumerable<(int X, int Y, int Z, T Block)> AllBlocks();
        T GetBlock(int X, int Y, int Z);
    }
}
