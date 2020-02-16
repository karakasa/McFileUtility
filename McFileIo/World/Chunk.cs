using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fNbt;
using McFileIo.Blocks;
using McFileIo.Blocks.BlockEntities;
using McFileIo.Interfaces;
using McFileIo.Utility;

namespace McFileIo.World
{
    /// <summary>
    /// Stores basic chunk information
    /// </summary>
    public abstract class Chunk : INbtFileSnapshot
    {
        private const string FieldLevel = "Level";
        private const string FieldSections = "Sections";
        private const string FieldTileEntities = "TileEntities";
        private const string FieldxPos = "xPos";
        private const string FieldzPos = "zPos";

        private int _xpos, _zpos;
        private Dictionary<(int x, int y, int z), BlockEntity> _entities = null; 

        /// <summary>
        /// Whether the chunk defines xPos and zPos.
        /// If they are not defined, you may only infer the chunk coordinates from region file.
        /// </summary>
        public bool InChunkPositionAvailable { get; private set; } = false;

        public bool IsSectionsLoaded { get; private set; } = false;

        /// <summary>
        /// Whether the chunk is fully initialized
        /// </summary>
        public virtual bool IsComplete { get; private set; } = false;

        /// <inheritdoc/>
        public NbtFile NbtFileSnapshot { get; private set; } = null;

        /// <inheritdoc/>
        public NbtCompound NbtSnapshot => NbtFileSnapshot?.RootTag;

        /// <summary>
        /// Get all BlockEntities. See <see cref="BlockEntity"/> for more information.
        /// </summary>
        public ICollection<BlockEntity> BlockEntities
        {
            get
            {
                EnsureBlockEntities();
                return _entities.Values;
            }
        }

        /// <summary>
        /// Chunk X coordinate
        /// </summary>
        public int X => _xpos;

        /// <summary>
        /// Chunk Z coordinate
        /// </summary>
        public int Z => _zpos;

        protected Chunk()
        {
        }

        /// <summary>
        /// Initialize properties from Nbt storage
        /// </summary>
        /// <param name="root">Nbt storage</param>
        protected virtual void InitializeComponents(NbtCompound root)
        {
            try
            {
                if (!root.TryGet(FieldLevel, out NbtCompound level)) return;

                InChunkPositionAvailable = level.TryGet(FieldxPos, out NbtInt xpos);
                InChunkPositionAvailable = level.TryGet(FieldzPos, out NbtInt zpos) && InChunkPositionAvailable;

                if (InChunkPositionAvailable)
                {
                    _xpos = xpos.Value;
                    _zpos = zpos.Value;
                }

                ReadChunkSections(level);

                IsComplete = true;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Load chunk section from Nbt storage
        /// </summary>
        /// <param name="section">Nbt storage</param>
        /// <returns>Successful or not</returns>
        protected abstract bool GetBlockData(NbtCompound section);

        /// <summary>
        /// Post-initialization after chunk sections are all loaded.
        /// </summary>
        /// <param name="rootTag">Nbt storage</param>
        protected virtual void PostInitialization(NbtCompound rootTag) { }

        private const string FieldDataVersion = "DataVersion";

        protected void EnsureBlockEntities()
        {
            if (_entities == null)
            {
                if (NbtFileSnapshot == null) throw new NotSupportedException();
                ReadBlockEntities(NbtFileSnapshot);
            }
        }

        protected void EnsureChunkSections()
        {
            if (!IsSectionsLoaded)
            {
                if (NbtFileSnapshot == null) throw new NotSupportedException();
                if (!NbtFileSnapshot.RootTag.TryGet(FieldLevel, out NbtCompound level)) return;
                ReadChunkSections(level);
            }
        }

        private void ReadChunkSections(NbtCompound level)
        {
            if (!level.TryGet(FieldSections, out NbtList sections)) return;

            foreach (var section in sections)
            {
                if (!(section is NbtCompound seccomp))
                    continue;

                if (!GetBlockData(seccomp))
                    return;
            }

            IsSectionsLoaded = true;
        }

        private void ReadBlockEntities(NbtFile nbt)
        {
            _entities = new Dictionary<(int x, int y, int z), BlockEntity>();
            if (!nbt.RootTag.TryGet(FieldLevel, out NbtCompound level)) return;
            if (!level.TryGet(FieldTileEntities, out NbtList list)) return;
            foreach (var it in list.OfType<NbtCompound>())
            {
                var entity = BlockEntity.CreateFromNbtCompound(it);
                _entities[(entity.X, entity.Y, entity.Z)] = entity;
            }
        }

        /// <summary>
        /// Get BlockEntities by an Id predicate.
        /// This method doesn't create BlockEntity if (1) it isn't previously cached, and (2) it is filtered out by the predicate. Therefore the method is faster.
        /// However, if you need to traverse chunk entities for multiple times, it is recommend to use <see cref="BlockEntities"/> to cache all upfront or do your own caching.
        /// </summary>
        /// <param name="predicate">Predicate</param>
        /// <returns>BlockEntities</returns>
        public IEnumerable<BlockEntity> GetBlockEntitiesById(Func<string, bool> predicate)
        {
            if (_entities == null && NbtSnapshot != null)
            {
                if (!NbtSnapshot.TryGet(FieldLevel, out NbtCompound level))
                {
                    ExceptionHelper.ThrowParseMissingError(nameof(level));
                    yield break;
                }

                if (!level.TryGet(FieldTileEntities, out NbtList list))
                {
                    ExceptionHelper.ThrowParseMissingError(nameof(list));
                    yield break;
                }

                foreach (var it in list.OfType<NbtCompound>())
                    if (predicate(BlockEntity.GetIdFromNbtCompound(it)))
                        yield return BlockEntity.CreateFromNbtCompound(it);
            }
            else
            {
                foreach (var it in BlockEntities.Where(entity => predicate(entity.Id)))
                    yield return it;
            }
        }

        private bool TryGetBlockEntityByWorldCoord(int x, int y, int z, out BlockEntity entity)
        {
            EnsureBlockEntities();
            return _entities.TryGetValue((x, y, z), out entity);
        }

        private void ReadFromNbt(NbtFile nbt)
        {
            InitializeComponents(nbt.RootTag);

            if (SnapshotStrategy == NbtSnapshotStrategy.Enable)
            {
                NbtFileSnapshot = nbt;
            }
            else if(SnapshotStrategy == NbtSnapshotStrategy.Disable)
            {
                ReadBlockEntities(nbt);
            }

            PostInitialization(nbt.RootTag);
        }

        public NbtSnapshotStrategy SnapshotStrategy;

        /// <summary>
        /// Create chunk object from compressed bytes.
        /// Different types, such as <see cref="ClassicChunk"/> and <see cref="NamespacedChunk"/>, will be created per version.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset of data</param>
        /// <param name="length">Length of data</param>
        /// <param name="compressionType">Compress type</param>
        /// <param name="snapshot">Snapshot strategy</param>
        /// <returns>The created chunk</returns>
        public static Chunk CreateFromBytes(byte[] buffer, int offset = 0, int length = 0,
            ChunkCompressionType compressionType = ChunkCompressionType.ZLib,
            NbtSnapshotStrategy snapshot = NbtSnapshotStrategy.Enable)
        {
            if (length == 0) length = buffer.Length - offset;
            Chunk chunk = null;

            var nbt = new NbtFile();
            nbt.LoadFromBuffer(buffer, offset, length, compressionType.ToNbtCompression());

            if (!nbt.RootTag.TryGet(FieldDataVersion, out NbtInt version) || version.Value <= 1343)
            {
                chunk = new ClassicChunk();
            }
            else
            {
                chunk = new NamespacedChunk();
            }

            chunk.SnapshotStrategy = snapshot;
            chunk.ReadFromNbt(nbt);

            return chunk;
        }

        /// <summary>
        /// Commit changes to Nbt storage.
        /// </summary>
        public virtual void CommitChanges()
        {
            if (NbtFileSnapshot == null) throw new NotSupportedException();
        }

        /// <summary>
        /// Get post-Anvil block index by its world coord.
        /// </summary>
        /// <param name="x">World X</param>
        /// <param name="y">World Y</param>
        /// <param name="z">World Z</param>
        /// <returns>Block index</returns>
        public static int GetBlockIndexByCoord(int x, int y, int z)
        {
            return ((y & 15) << 8) + ((z & 15) << 4) + (x & 15);
        }

        /// <summary>
        /// Get pre-Anvil block index by its world coord.
        /// </summary>
        /// <param name="x">World X</param>
        /// <param name="y">World Y</param>
        /// <param name="z">World Z</param>
        /// <returns>Block index</returns>
        public static int GetBlockIndexByCoordOld(int x, int y, int z)
        {
            return ((x & 15) << 8) + ((z & 15) << 4) + (y & 15);
        }

        /// <summary>
        /// Get chunk coordinates by world coord.
        /// </summary>
        /// <param name="x">World X</param>
        /// <param name="z">World Z</param>
        /// <returns></returns>
        public static (int cx, int cz) GetChunkCoordByWorld(int x, int z)
        {
            return (x >> 4, z >> 4);
        }

        /// <summary>
        /// Returns existing Y section indexes.
        /// </summary>
        /// <returns>Ys</returns>
        public abstract IEnumerable<int> GetExistingYs();
    }
}
