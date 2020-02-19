using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    public enum RegionLoadApproach
    {
        /// <summary>
        /// Chunks will be loaded from Stream on demand.
        /// You must ensure the stream is not closed.
        /// </summary>
        InStream,

        /// <summary>
        /// All chunks are loaded into memory when the region file is loaded
        /// </summary>
        InMemory,

        /// <summary>
        /// Do not attempt to load chunks. For region-specific information only.
        /// </summary>
        ForProbing
    }
}
