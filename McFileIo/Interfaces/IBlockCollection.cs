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
        /// <summary>
        /// Change a batch of blocks.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="customPalette"></param>
        void SetBlock(ICollection<ChangeBlockRequest> requests, IList<T> customPalette);
        /// <summary>
        /// Set a block at a designated position.
        /// Use <see cref="SetBlock(ICollection{ChangeBlockRequest}, IList{T})"/> if a lot of blocks would be changed.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="block"></param>
        void SetBlock(int X, int Y, int Z, T block);
        /// <summary>
        /// Get all blocks.
        /// </summary>
        /// <returns></returns>
        IEnumerable<(int X, int Y, int Z, T Block)> AllBlocks();
        /// <summary>
        /// Get the block at a designated position.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        T GetBlock(int X, int Y, int Z);

        /// <summary>
        /// Save all blocks to memory storage.
        /// </summary>
        void SaveToMemoryStorage();
        /// <summary>
        /// Save all blocks to the underlying storage (usually NBT).
        /// </summary>
        void SaveToLowLevelStorage();
    }
}
