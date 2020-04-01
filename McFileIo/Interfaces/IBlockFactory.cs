using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockFactory
    {
        int ClassicIndex { get; }
        string NamespacedName { get; }
        Block CreateFromClassic(ClassicBlock block, BlockEntity entity);
        Block CreateFromNamespaced(NamespacedBlock block, BlockEntity entity);
    }
}
