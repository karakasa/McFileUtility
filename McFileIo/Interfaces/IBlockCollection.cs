using McFileIo.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    /// <summary>
    /// Collection of blocks
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBlockCollection<T>
    {
        void SetBlock(IEnumerable<ChangeBlockRequest> requests, IList<T> customPalette);
        void SetBlock(int X, int Y, int Z, T block);
        IEnumerable<(int X, int Y, int Z, T Block)> AllBlocks();
        T GetBlock(int X, int Y, int Z);
    }
}
