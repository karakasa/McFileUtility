using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks
{
    /// <summary>
    /// Represents an abstract block, stores information regardless of the underlying type (ID-based or namespaced-based)
    /// </summary>
    public abstract class Block
    {
        private int _classicIndex = -1;
        private string _nsName = null;

        protected Block()
        {
        }
        protected Block(int id)
        {
            _classicIndex = id;
        }
        protected Block(string namespacedName)
        {
            _nsName = namespacedName;
        }
        protected Block(int id, string namespacedName)
        {
            _classicIndex = id;
            _nsName = namespacedName;
        }
        /// <summary>
        /// Determine if the block has an entity associated.
        /// </summary>
        public virtual bool HasBlockEntity { get => false; }
        /// <summary>
        /// Id in a classic format
        /// </summary>
        public virtual int ClassicIndex { get => _classicIndex; }
        /// <summary>
        /// Namespaced name in a modern format
        /// </summary>
        public virtual string NamespacedName { get => _nsName; }
        /// <summary>
        /// Get the associated BlockEntity.
        /// Location info (X, Y, Z) might not be reliable.
        /// </summary>
        /// <returns></returns>
        public virtual BlockEntity CreateBlockEntity() => null;
        /// <summary>
        /// Create a low-level classic block.
        /// </summary>
        /// <returns></returns>
        public virtual ClassicBlock ToClassic()
        {
            if (ClassicIndex != -1)
            {
                return new ClassicBlock(ClassicIndex);
            }
            return default;
        }

        /// <summary>
        /// Create a low-level namespaced block.
        /// </summary>
        /// <returns></returns>
        public virtual NamespacedBlock ToNamespaced()
        {
            if (NamespacedName != null)
            {
                return SimpleBlocks.QuerySimpleBlockCache(NamespacedName);
            }
            return default;
        }
    }
}
