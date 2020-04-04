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
        public UnknownBlock(NamespacedBlock block, BlockEntity entity = null) : base(BuiltInUniqueId)
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
        public UnknownBlock(ClassicBlock block, BlockEntity entity = null) : base(BuiltInUniqueId)
        {
            _isNamespaced = false;
            _entity = entity;
            _classicblock = block;
            _classicIndex = block.Id;
        }

        private int _classicIndex = -1;
        private string _namespacedName = null;

        public static readonly Guid BuiltInUniqueId = new Guid("{36B1A8E1-BEB0-425C-B175-D4EAF2CC3CEA}");

        public BlockEntity AssociatedEntity => _entity;

        public NamespacedBlock ToNamespaced()
        {
            if (_isNamespaced)
                return _nsblock;
            throw CannotConvert("Modern");
        }

        public ClassicBlock ToClassic()
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
