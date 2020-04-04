using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockFactory : IContextAware
    {
        int ClassicIndex { get; }
        string NamespacedName { get; }
        Guid UniqueId { get; }
        Block FromClassic(ClassicBlock block, BlockEntity entity);
        Block FromNamespaced(NamespacedBlock block, BlockEntity entity);
        ClassicBlock ToClassic(Block block, out BlockEntity entity);
        NamespacedBlock ToNamespaced(Block block, out BlockEntity entity);
        void RegisterCachedBlocks(IBlockDispatcherCache cache);
    }
}
