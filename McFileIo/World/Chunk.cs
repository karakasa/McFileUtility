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
        protected const string FieldLevel = "Level";
        protected const string FieldSections = "Sections";
        protected const string FieldTileEntities = "TileEntities";
        protected const string FieldLightPopulated = "LightPopulated";
        protected const string FieldxPos = "xPos";
        protected const string FieldzPos = "zPos";

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
        public NbtCompound NbtSnapshot { get; private set; }

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

                var posAvailable = level.TryGet(FieldxPos, out NbtInt xpos);
                posAvailable = level.TryGet(FieldzPos, out NbtInt zpos) && posAvailable;

                if (posAvailable)
                {
                    if (InChunkPositionAvailable)
                    {
                        if(xpos.Value != _xpos || zpos.Value != _zpos)
                        {
                            // ExceptionHelper.ThrowParseError($"Chunk {_xpos},{_zpos} position mismatch", ParseErrorLevel.Warning);
                        }
                    }
                    else
                    {
                        _xpos = xpos.Value;
                        _zpos = zpos.Value;
                        InChunkPositionAvailable = true;
                    }
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
                if (NbtSnapshot == null) throw new NotSupportedException();
                if (!NbtSnapshot.TryGet(FieldLevel, out NbtCompound level)) return;
                ReadBlockEntities(level);
            }
        }

        protected void EnsureChunkSections()
        {
            if (!IsSectionsLoaded)
            {
                if (NbtSnapshot == null) throw new NotSupportedException();
                if (!NbtSnapshot.TryGet(FieldLevel, out NbtCompound level)) return;
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

        private void ReadBlockEntities(NbtCompound level)
        {
            _entities = new Dictionary<(int x, int y, int z), BlockEntity>();
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

        private void ReadFromNbt(NbtCompound nbt)
        {
            InitializeComponents(nbt);

            if (SnapshotStrategy == NbtSnapshotStrategy.Enable)
            {
                NbtSnapshot = nbt;
            }
            else if (SnapshotStrategy == NbtSnapshotStrategy.Disable)
            {
                if (!nbt.TryGet(FieldLevel, out NbtCompound level)) return;

                ReadBlockEntities(level);
                ReadHeightMap(level);
            }

            PostInitialization(nbt);
        }

        public NbtSnapshotStrategy SnapshotStrategy;

        public int Version;

        public uint Timestamp;

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
            int? ChunkX = null, int? ChunkZ = null,
            ChunkCompressionType compressionType = ChunkCompressionType.ZLib,
            NbtSnapshotStrategy snapshot = NbtSnapshotStrategy.Enable)
        {
            if (length == 0) length = buffer.Length - offset;
            Chunk chunk = null;

            var nbt = new NbtFile();
            nbt.LoadFromBuffer(buffer, offset, length, compressionType.ToNbtCompression());

            var hasVersion = nbt.RootTag.TryGet(FieldDataVersion, out NbtInt version);

            if (!hasVersion || version.Value <= DataVersion.v1_12_2)
            {
                chunk = new ClassicChunk();
            }
            else
            {
                chunk = new NamespacedChunk();
            }

            if (ChunkX != null && ChunkZ != null)
            {
                chunk.InChunkPositionAvailable = true;
                chunk._xpos = ChunkX.Value;
                chunk._zpos = ChunkZ.Value;
            }

            chunk.Version = hasVersion ? version.Value : DataVersion.PreSnapshot15w32a;
            chunk.SnapshotStrategy = snapshot;
            chunk.NbtFileSnapshot = nbt;
            chunk.ReadFromNbt(nbt.RootTag);

            return chunk;
        }

        /// <summary>
        /// Commit changes to Nbt storage.
        /// </summary>
        public virtual void CommitChanges()
        {
            if (NbtSnapshot == null) throw new NotSupportedException();
            WriteToNbt();
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

        // Use HeightMaps instead of HeightMap after 1.13

        private HeightMap _heightMap = null;

        private void ReadHeightMap(NbtCompound level)
        {
            _heightMap = NbtClassIo.CreateAndReadFromNbt<HeightMap>(level);
            if (_heightMap.State == HeightMap.StorageType.NotCalculated)
                _heightMap.Calculate(this);
        }

        private void EnsureHeightMap()
        {
            if (_heightMap == null)
            {
                if (NbtSnapshot == null) throw new NotSupportedException();
                if (!NbtSnapshot.TryGet(FieldLevel, out NbtCompound level)) return;
                ReadHeightMap(level);
            }
        }

        public HeightMap HeightMap
        {
            get
            {
                EnsureHeightMap();
                return _heightMap;
            }
            protected set
            {
                _heightMap = value;
            }
        }

        /// <summary>
        /// Remove height map from the current chunk so that it will be recalculated by Minecraft when loaded next time.
        /// </summary>
        public void PurgeHeightMap()
        {
            _heightMap = null;
        }

        internal abstract bool IsAirBlock(int x, int y, int z);

        /// <summary>
        /// Call this if you are creating a new, empty derived class.
        /// It will initialize necessary structure for an empty chunk.
        /// </summary>
        protected void CreateAnew(int dataversion)
        {
            HeightMap = new HeightMap();
            NbtSnapshot = new NbtCompound("");

            var level = new NbtCompound(FieldLevel);
            level.Add(new NbtList(FieldSections, NbtTagType.Compound));
            level.Add(new NbtList(FieldTileEntities, NbtTagType.Compound));

            NbtSnapshot.Add(level);
            NbtSnapshot.Add(new NbtInt(FieldDataVersion, dataversion));
        }

        /// <summary>
        /// Determine how block/skylight is recalculated if the chunk is altered
        /// </summary>
        public enum LightCalculationStrategy
        {
            /// <summary>
            /// Default mode. Light information will be removed and Minecraft is reponsibile for re-calculation.
            /// It has the best lighting outcome but may not be compatible with older MC versions.
            /// Tested and works with Minecraft Client 1.12.2.
            /// Bukkit (Spigot, Catserver, etc) server may regenerate chunks incorrectly with this mode.
            /// Refer to bugtrack: <a href="https://bugs.mojang.com/browse/MC-133855">https://bugs.mojang.com/browse/MC-133855</a>
            /// </summary>
            RemoveExisting,

            /// <summary>
            /// Copy the lighting data from the original chunk.
            /// Best for block-replacing without emissive/transparent blocks.
            /// You need to raise a light update in that chunk manually, in other conditions.
            /// </summary>
            CopyFromOldData,

            /// <summary>
            /// NOT IMPLEMENTED YET. DO NOT USE.
            /// Recalculate the light by McFileIo. Probably not as accurate as Minecraft.
            /// </summary>
            Recalculate
        }

        public LightCalculationStrategy LightCalculationMode = LightCalculationStrategy.RemoveExisting;

        protected virtual void WriteToNbt()
        {
            if (LightCalculationMode == LightCalculationStrategy.Recalculate)
                throw new NotImplementedException();

            WriteSections();

            if (LightCalculationMode == LightCalculationStrategy.RemoveExisting)
            {
                if (NbtSnapshot.TryGet<NbtByte>(FieldLightPopulated, out var lightPopulated))
                    lightPopulated.Value = 0;
            }
        }

        protected abstract void WriteSections();
    }
}
