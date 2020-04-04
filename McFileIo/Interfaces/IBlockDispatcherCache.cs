using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IBlockDispatcherCache
    {
        int RegisterDispatcherCache(bool hasClassicBlock = false, ClassicBlock classic = default, NamespacedBlock ns = null, BlockEntity entity = null);
    }
}
