using McFileIo.Blocks;
using McFileIo.Blocks.LowLevel;
using McFileIo.Blocks.LowLevel.BlockEntities;
using McFileIo.Enum;
using McFileIo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.World
{
    /// <summary>
    /// Represents an abstract version of chunks.
    /// Its contents are majorly independent of underlying storage type.
    /// </summary>
    public class Chunk : IBlockCollection<Block>
    {
        private readonly bool _namespaced = false;
        /// <summary>
        /// Get whether the chunk is read-only
        /// </summary>
        public bool ReadOnly { get; private set; }

        private readonly NamespacedChunk _namespacedChunk = null;
        private readonly NSCBlockTransaction _namespacedTransaction = null;
        private readonly ClassicChunk _classicChunk = null;

        private readonly AccessMode _access = AccessMode.Write;

        /// <summary>
        /// Create from a low-level NamespacedChunk.
        /// Do not use this unless you're sure of what you're doing.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="access"></param>
        /// <param name="dispatcher"></param>
        public Chunk(NamespacedChunk chunk, AccessMode access = AccessMode.Write, BlockFactoryDispatcher dispatcher = null)
        {
            _namespaced = true;
            _namespacedChunk = chunk;
            _access = access;
            ReadOnly = access == AccessMode.ReadOnly;

            if (!ReadOnly)
                _namespacedTransaction = _namespacedChunk.CreateChangeBlockTransaction();

            Factory = dispatcher;
        }

        /// <summary>
        /// Create from a low-level ClassicChunk.
        /// Do not use this unless you're sure of what you're doing.
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="access"></param>
        /// <param name="dispatcher"></param>
        public Chunk(ClassicChunk chunk, AccessMode access = AccessMode.Write, BlockFactoryDispatcher dispatcher = null)
        {
            _namespaced = false;
            _classicChunk = chunk;
            _access = access;
            ReadOnly = access == AccessMode.ReadOnly;

            Factory = dispatcher;
        }

        internal BlockFactoryDispatcher Factory { set; private get; }

        /// <summary>
        /// Get the internal chunk. Avoid operating on the low-level chunk directly,
        /// as it may break the state of this object.
        /// </summary>
        public LowLevelChunk InternalChunk => _namespaced ? (LowLevelChunk)_namespacedChunk : _classicChunk;

        private IBlockCollection<NamespacedBlock> ActiveNSBCollection => 
            (IBlockCollection<NamespacedBlock>)_namespacedTransaction ?? _namespacedChunk;

        /// <summary>
        /// Get all blocks. Use <see cref="AllBlocks(PredicateIncludeBlock)"/> if only a certain type of blocks are needed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(int X, int Y, int Z, Block Block)> AllBlocks()
        {
            if (_namespaced)
            {
                foreach (var (x, y, z, block) in ActiveNSBCollection.AllBlocks())
                {
                    _namespacedChunk.TryGetBlockEntityByWorldCoord(x, y, z, out BlockEntity entity);
                    yield return (x, y, z, Factory.CreateBlock(block, entity));
                }
            }
            else
            {
                foreach (var (x, y, z, block) in _classicChunk.AllBlocks())
                {
                    _classicChunk.TryGetBlockEntityByWorldCoord(x, y, z, out BlockEntity entity);
                    yield return (x, y, z, Factory.CreateBlock(block, entity));
                }
            }
        }

        /// <summary>
        /// Delegate to determine if a block should be created
        /// </summary>
        /// <param name="ClassicIndex"></param>
        /// <param name="NamespacedId"></param>
        /// <returns></returns>
        public delegate bool PredicateIncludeBlock(int ClassicIndex, string NamespacedId);

        /// <summary>
        /// Get all blocks of some types. It's faster than <see cref="AllBlocks"/> because it doesn't create excessive objects.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<(int X, int Y, int Z, Block Block)> AllBlocks(PredicateIncludeBlock predicate)
        {
            if (_namespaced)
            {
                foreach (var (x, y, z, block) in ActiveNSBCollection.AllBlocks())
                {
                    if (predicate(-1, block.Name))
                    {
                        _namespacedChunk.TryGetBlockEntityByWorldCoord(x, y, z, out BlockEntity entity);
                        yield return (x, y, z, Factory.CreateBlock(block, entity));
                    }
                }
            }
            else
            {
                foreach (var (x, y, z, block) in _classicChunk.AllBlocks())
                {
                    if (predicate(block.Id, null))
                    {
                        _classicChunk.TryGetBlockEntityByWorldCoord(x, y, z, out BlockEntity entity);
                        yield return (x, y, z, Factory.CreateBlock(block, entity));
                    }
                }
            }
        }

        public Block GetBlock(int X, int Y, int Z)
        {
            if (_namespaced)
            {
                var block = ActiveNSBCollection.GetBlock(X, Y, Z);
                _namespacedChunk.TryGetBlockEntityByWorldCoord(X, Y, Z, out BlockEntity entity);
                return Factory.CreateBlock(block, entity);
            }
            else
            {
                var block = _classicChunk.GetBlock(X, Y, Z);
                _classicChunk.TryGetBlockEntityByWorldCoord(X, Y, Z, out BlockEntity entity);
                return Factory.CreateBlock(block, entity);
            }
        }

        public void SaveToMemoryStorage()
        {
            if (ReadOnly)
                throw new AccessViolationException();

            if (_namespaced)
                _namespacedTransaction.CommitChanges();
        }

        public void SetBlock(ICollection<ChangeBlockRequest> requests, IList<Block> customPalette)
        {
            if (ReadOnly)
                throw new AccessViolationException();

            if (_namespaced)
            {
                var palette = customPalette.Select(block =>
                {
                    if (!Factory.TryGetNamespacedBlock(block, out var nsb, out var entity))
                        throw new ArgumentException("Some blocks don't have an underlying NamespacedBlock form.");

                    return (nsb, entity);

                }).ToArray();
                
                _namespacedTransaction.SetBlock(requests, palette.Select(p => p.nsb).ToArray());

                foreach(var it in requests)
                {
                    var entity = ApplyBlockEntity(palette[it.InListIndex].entity, it.X, it.Y, it.Z);
                    if (entity != null)
                        _namespacedChunk.SetBlockEntity(it.X, it.Y, it.Z, entity);
                }
            }
            else
            {
                var palette = customPalette.Select(block =>
                {
                    if (!Factory.TryGetClassicBlock(block, out var nsb, out var entity))
                        throw new ArgumentException("Some blocks don't have an underlying ClassicBlock form.");

                    return (nsb, entity);

                }).ToArray();

                _classicChunk.SetBlock(requests, palette.Select(p => p.nsb).ToArray());

                foreach (var it in requests)
                {
                    var entity = ApplyBlockEntity(palette[it.InListIndex].entity, it.X, it.Y, it.Z);
                    if (entity != null)
                        _classicChunk.SetBlockEntity(it.X, it.Y, it.Z, entity);
                }
            }
        }

        public void SetBlock(int X, int Y, int Z, Block block)
        {
            if (ReadOnly)
                throw new AccessViolationException();

            if (_namespaced)
            {
                if (Factory.TryGetNamespacedBlock(block, out var nsb, out var entity))
                {
                    var entity2 = ApplyBlockEntity(entity, X, Y, Z);
                    if (entity2 != null)
                        _namespacedChunk.SetBlockEntity(X, Y, Z, entity2);

                    _namespacedTransaction.SetBlock(X, Y, Z, nsb);
                }
            }
            else
            {
                if (Factory.TryGetClassicBlock(block, out var classic, out var entity))
                {
                    var entity2 = ApplyBlockEntity(entity, X, Y, Z);
                    if (entity2 != null)
                        _classicChunk.SetBlockEntity(X, Y, Z, entity2);

                    _classicChunk.SetBlock(X, Y, Z, classic);
                }
            }
        }

        private BlockEntity ApplyBlockEntity(BlockEntity entity, int X, int Y, int Z)
        {
            if (entity == null)
                return null;

            if (_access == AccessMode.ParallelWrite)
            {
                var entity2 = entity.InternalClone();
                entity2.AssignCoord(X, Y, Z);
                return entity2;
            } else if (_access == AccessMode.Write)
            {
                entity.AssignCoord(X, Y, Z);
                return entity;
            }
            else
            {
                throw new AccessViolationException();
            }
        }

        public void SaveToLowLevelStorage()
        {
            if (ReadOnly)
                throw new AccessViolationException();

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
