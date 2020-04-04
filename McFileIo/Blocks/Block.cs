using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks
{
    /// <summary>
    /// Represents an abstract block, stores information regardless of how underlying data is stored.
    /// </summary>
    public abstract class Block
    {
        public const int NotSupportClassicIndex = -1;
        public const string NotSupportNamespacedId = null;

        private Guid _guid = Guid.Empty;
        /// <summary>
        /// Create an empty instance.
        /// Remember to override <see cref="UniqueId"/>.
        /// </summary>
        protected Block()
        {
        }
        /// <summary>
        /// Assign Guid to this Block.
        /// No need to override <see cref="UniqueId"/> if the constructor is used.
        /// </summary>
        /// <param name="guid"></param>
        protected Block(Guid guid)
        {
            _guid = guid;
        }

        /// <summary>
        /// Get the unique Id of this block. This is used to distinguish blocks in internal procedures.
        /// </summary>
        public virtual Guid UniqueId => _guid;
        public virtual int CachedId { get; set; } = -1;
        internal Block InternalClone() => MemberwiseClone() as Block;
    }
}
