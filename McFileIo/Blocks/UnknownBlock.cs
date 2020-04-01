using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Blocks
{
    /// <summary>
    /// Represents unknown block. Therefore chunks can be saved even if some blocks aren't supported.
    /// It can be also used as a raw way to insert low-level blocks into abstract chunks.
    /// UnknownBlock cannot be converted to the other way of storage.
    /// </summary>
    public sealed class UnknownBlock : Block
    {
        private readonly bool _isNamespaced;
        private readonly NamespacedBlock _nsblock = null;
        private readonly ClassicBlock _classicblock;
        private readonly BlockEntity _entity = null;
        /// <summary>
        /// Create an UnknownBlock from low-level namespaced block.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="entity"></param>
        public UnknownBlock(NamespacedBlock block, BlockEntity entity = null)
        {
            _isNamespaced = true;
            _entity = entity;
            _nsblock = block;
            _namespacedName = block.Name;
        }
        /// <summary>
        /// Create an UnknownBlock from low-level classic block.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="entity"></param>
        public UnknownBlock(ClassicBlock block, BlockEntity entity = null)
        {
            _isNamespaced = false;
            _entity = entity;
            _classicblock = block;
            _classicIndex = block.Id;
        }

        private int _classicIndex = -1;
        private string _namespacedName = null;

        public override int ClassicIndex => _classicIndex;
        public override string NamespacedName => _namespacedName;

        public override bool HasBlockEntity => _entity != null;

        public override BlockEntity CreateBlockEntity() => _entity;

        public override NamespacedBlock ToNamespaced()
        {
            if (_isNamespaced)
                return _nsblock;
            throw CannotConvert("Modern");
        }

        public override ClassicBlock ToClassic()
        {
            if (!_isNamespaced)
                return _classicblock;
            throw CannotConvert("Classic");
        }

        public override string ToString()
        {
            return _isNamespaced ? "Unknown Modern Block" : "Unknown Classic Block";
        }
        private Exception CannotConvert(string target)
        {
            return new InvalidOperationException($"{this} cannot converted to {target}.");
        }
    }
}
