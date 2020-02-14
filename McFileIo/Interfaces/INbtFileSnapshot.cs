using fNbt;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    /// <summary>
    /// By implementing this, your object can store a <see cref="NbtFile"/> structure as snapshot, allowing low-level operations.
    /// This interface is for objects utilizing top-level Nbt storage, which provides more information.
    /// </summary>
    public interface INbtFileSnapshot : INbtSnapshot
    {
        /// <summary>
        /// Snapshot of the Nbt file storage representing this object
        /// </summary>
        NbtFile NbtFileSnapshot { get; }
    }
}
