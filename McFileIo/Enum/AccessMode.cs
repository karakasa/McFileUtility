using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    /// <summary>
    /// Determine the access of certain objects. It will affect how data is stored and cached.
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        /// The object cannot be written into.
        /// The object doesn't initialize writing queue to save memory.
        /// Usually reading read-only objects is thread-safe.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// The object creates necessary data structures to support writing,
        /// therefore uses more memory. In a multithreaded environment, you need to
        /// implement your own lock, or the object uses its internal lock, depending on
        /// internal implementation (see the documentation from the object).
        /// </summary>
        Write,

        /// <summary>
        /// The object would adjust its internal structure and/or
        /// algorithm to support parallel writing operations. Not all objects support this
        /// access mode. You don't need to use locks around the object, if the access mode
        /// is supported.
        /// </summary>
        ParallelWrite
    }
}
