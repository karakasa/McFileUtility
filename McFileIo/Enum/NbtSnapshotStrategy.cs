using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    /// <summary>
    /// Define whether an Nbt-based object should take a snapshot of the original Nbt structure.
    /// Taking snapshots will consume more memory.
    /// </summary>
    public enum NbtSnapshotStrategy
    {
        /// <summary>
        /// Snapshot will be taken. Supported properties will be loaded on demand.
        /// </summary>
        Enable,

        /// <summary>
        /// Snapshot won't be taken, but all supported properties are pre-loaded when object is created.
        /// If you save an object later, non-supported properties are ignored. Takes longer time when creating an object.
        /// </summary>
        Disable,

        /// <summary>
        /// Snapshot won't be taken and supported files are not pre-loaded.
        /// Fastest and least memory is consumed, but you may only access core properties.
        /// Accessing other properties will raise an NotSupportedException.
        /// Saving with this flag is disabled for most objects.
        /// </summary>
        DisableAndDontPreload
    }
}
