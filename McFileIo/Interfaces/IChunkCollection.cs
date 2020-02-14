using McFileIo.Utility;
using McFileIo.World;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    public interface IChunkCollection
    {
        IEnumerable<Chunk> AllChunks(TraverseType type = TraverseType.AlreadyLoaded);
        void UnloadAllChunks();
    }
}
