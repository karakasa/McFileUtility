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
    public abstract class Chunk : INbtSnapshot
    {
        private const string FieldLevel = "Level";
        private const string FieldSections = "Sections";
        private const string FieldTileEntities = "TileEntities";
        private const string FieldxPos = "xPos";
        private const string FieldzPos = "zPos";

        private int _xpos, _zpos;
        private Dictionary<(int x, int y, int z), BlockEntity> _entities = null; 
        public bool InChunkPositionAvailable { get; private set; } = false;

        public virtual bool IsValid { get; private set; } = false;
        public NbtFile NbtFileSnapshot { get; private set; } = null;
        public NbtCompound NbtSnapshot => NbtFileSnapshot?.RootTag;

        public ICollection<BlockEntity> BlockEntities
        {
            get
            {
                EnsureBlockEntities();
                return _entities.Values;
            }
        }

        public int X => _xpos;
        public int Z => _zpos;

        protected Chunk()
        {
        }

        protected virtual void InitializeComponents(NbtCompound root)
        {
            try
            {
                if (!root.TryGet(FieldLevel, out NbtCompound level)) return;
                if (!level.TryGet(FieldSections, out NbtList sections)) return;

                foreach(var section in sections)
                {
                    if (!(section is NbtCompound seccomp))
                        continue;

                    if (!GetBlockData(seccomp)) return;
                }

                InChunkPositionAvailable = level.TryGet(FieldxPos, out NbtInt xpos);
                InChunkPositionAvailable = level.TryGet(FieldzPos, out NbtInt zpos) && InChunkPositionAvailable;

                if (InChunkPositionAvailable)
                {
                    _xpos = xpos.Value;
                    _zpos = zpos.Value;
                }

                IsValid = true;
            }
            catch
            {
            }
        }

        protected abstract bool GetBlockData(NbtCompound seciton);
        protected virtual void PostInitialization(NbtCompound rootTag) { }

        private const string FieldDataVersion = "DataVersion";

        private void EnsureBlockEntities()
        {
            if (_entities == null)
            {
                if (NbtFileSnapshot == null) throw new NotSupportedException();
                ReadBlockEntities(NbtFileSnapshot);
            }
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

        private void ReadFromNbt(NbtFile nbt, NbtSnapshotStrategy snapshot)
        {
            InitializeComponents(nbt.RootTag);

            if (snapshot == NbtSnapshotStrategy.Enable)
            {
                NbtFileSnapshot = nbt;
            }
            else if(snapshot == NbtSnapshotStrategy.Disable)
            {
                ReadBlockEntities(nbt);
            }

            PostInitialization(nbt.RootTag);
        }

        public static Chunk CreateFromCompressedBytes(byte[] buffer, int offset = 0, int length = 0,
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

            chunk.ReadFromNbt(nbt, snapshot);

            return chunk;
        }

        public virtual void CommitChanges()
        {
            if (NbtFileSnapshot == null) throw new NotSupportedException();
        }

        public static int GetBlockIndexByCoord(int x, int y, int z)
        {
            return ((y & 15) << 8) + ((z & 15) << 4) + (x & 15);
        }

        public static int GetBlockIndexByCoordOld(int x, int y, int z)
        {
            return ((x & 15) << 8) + ((z & 15) << 4) + (y & 15);
        }

        public static (int cx, int cz) GetChunkCoordByWorld(int x, int z)
        {
            return (x >> 4, z >> 4);
        }
    }
}
