using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    /// <summary>
    /// Choose what objects should be traversed
    /// </summary>
    public enum TraverseType
    {
        /// <summary>
        /// Only traverse the loaded objects
        /// </summary>
        AlreadyLoaded,

        /// <summary>
        /// Traverse all. Load them when necessary.
        /// </summary>
        All
    }
}
