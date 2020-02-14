using fNbt;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Interfaces
{
    /// <summary>
    /// By implementing this, your object can store a <see cref="NbtCompound"/> structure as snapshot, allowing low-level operations.
    /// </summary>
    public interface INbtSnapshot
    {
        /// <summary>
        /// Snapshot of the Nbt storage representing this object
        /// </summary>
        NbtCompound NbtSnapshot { get; }
    }
}
