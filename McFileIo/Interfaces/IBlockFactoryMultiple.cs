using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockFactoryMultiple : IContextAware
    {
        IEnumerable<int> ClassicIndexes { get; }
        IEnumerable<string> NamespacedNames { get; }
        IEnumerable<Guid> UniqueIds { get; }
        Block FromClassic(int index, ClassicBlock block, BlockEntity entity);
        Block FromNamespaced(int index, NamespacedBlock block, BlockEntity entity);
        ClassicBlock ToClassic(int index, Block block, out BlockEntity entity);
        NamespacedBlock ToNamespaced(int index, Block block, out BlockEntity entity);
        void RegisterCachedBlocks(IBlockDispatcherCache cache);
    }
}
