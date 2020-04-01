using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockFactoryMultiple
    {
        IEnumerable<int> ClassicIndex { get; }
        IEnumerable<string> NamespacedName { get; }
        Block CreateFromClassic(int index, ClassicBlock block, BlockEntity entity);
        Block CreateFromNamespaced(int index, NamespacedBlock block, BlockEntity entity);
    }
}
