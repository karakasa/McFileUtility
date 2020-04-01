using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.World
{
    public class Chunk : IBlockCollection<Block>
    {
        private readonly bool _namespaced = false;
        public bool ReadOnly { get; private set; }

        private readonly NamespacedChunk _namespacedChunk = null;
        private readonly NSCBlockTransaction _namespacedTransaction = null;
        private readonly ClassicChunk _classicChunk = null;
        public Chunk(NamespacedChunk chunk, bool readOnly = false)
        {
            _namespaced = true;
            _namespacedChunk = chunk;
            ReadOnly = readOnly;

            if (!readOnly)
                _namespacedTransaction = _namespacedChunk.CreateChangeBlockTransaction();
        }

        public Chunk(ClassicChunk chunk, bool readOnly = false)
        {
            _namespaced = false;
            _classicChunk = chunk;
            ReadOnly = readOnly;
        }

        public LowLevelChunk InternalChunk => _namespaced ? (LowLevelChunk)_namespacedChunk : _classicChunk;

        public IEnumerable<(int X, int Y, int Z, Block Block)> AllBlocks()
        {
            throw new NotImplementedException();
        }

        public Block GetBlock(int X, int Y, int Z)
        {
            throw new NotImplementedException();
        }

        public void SaveToMemoryStorage()
        {
            if (ReadOnly)
                throw new NotSupportedException();

            if (_namespaced)
                _namespacedTransaction.CommitChanges();
        }

        public void SetBlock(IEnumerable<ChangeBlockRequest> requests, IList<Block> customPalette)
        {
            if (ReadOnly)
                throw new NotSupportedException();

            var preserved = requests.ToArray();

            if (_namespaced)
            {
                var translated = customPalette.Select(b => b.ToNamespaced()).ToArray();
                _namespacedTransaction.SetBlock(preserved, translated);
            }
            else
            {
                var translated = customPalette.Select(b => b.ToClassic()).ToArray();
                _classicChunk.SetBlock(preserved, translated);
            }

            foreach(var it in preserved)
            {
                ApplyBlockEntity(it.X, it.Y, it.Z, customPalette[it.InListIndex]);
            }
        }

        public void SetBlock(int X, int Y, int Z, Block block)
        {
            if (ReadOnly)
                throw new NotSupportedException();

            if (_namespaced)
            {
                _namespacedTransaction.SetBlock(X, Y, Z, block.ToNamespaced());
            }
            else
            {
                _classicChunk.SetBlock(X, Y, Z, block.ToClassic());
            }

            ApplyBlockEntity(X, Y, Z, block);
        }

        private void ApplyBlockEntity(int X, int Y, int Z, Block block)
        {
            if (!block.HasBlockEntity)
                return;

            var entity = block.CreateBlockEntity();
            entity.AssignCoord(X, Y, Z);

            (_namespaced ? (LowLevelChunk)_namespacedChunk : _classicChunk).SetBlockEntity(X, Y, Z, entity);
        }

        public void SaveToLowLevelStorage()
        {
            if (ReadOnly)
                throw new NotSupportedException();

            if (_namespaced)
            {
                _namespacedTransaction.CommitChanges();
                _namespacedChunk.CommitChanges();
            }
            else
            {
                _classicChunk.CommitChanges();
            }
        }
    }
}
