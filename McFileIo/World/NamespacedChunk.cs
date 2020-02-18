using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using fNbt;
using McFileIo.Blocks;
using McFileIo.Interfaces;
using McFileIo.Utility;

namespace McFileIo.World
{
    public sealed class NamespacedChunk : Chunk, IBlockCollection<NamespacedBlock>
    {
        public static bool IgnoreBlockProperty = false;

        private const string FieldY = "Y";
        private const string FieldPalette = "Palette";
        private const string FieldBlockStates = "BlockStates";

        private NbtList[] _paletteList = new NbtList[16];
        private long[][] _blockStateRaw = new long[16][];

        private List<NamespacedBlock>[] _palette = new List<NamespacedBlock>[16];
        private IDynBitArray[] _blockStates = new IDynBitArray[16];

        public static NamespacedChunk CreateEmpty()
        {
            var chunk = new NamespacedChunk();
            chunk.CreateAnew();
            return chunk;
        }

        internal NamespacedChunk()
        {
        }

        private static List<NamespacedBlock> ToNamespacedBlockList(NbtList list)
        {
            return new List<NamespacedBlock>(list.OfType<NbtCompound>()
                .Select(c => NamespacedBlock.CreateFromNbt(c, IgnoreBlockProperty)));
        }

        protected override bool GetBlockData(NbtCompound section)
        {
            if (!section.TryGet(FieldY, out NbtByte nbtY)) return false;
            int y = nbtY.Value;

            if (y == 255) return true;
            if (y >= 16) return false;

            if (!section.TryGet(FieldPalette, out NbtList list)) return false;
            if (!section.TryGet(FieldBlockStates, out NbtLongArray blocks)) return false;

            if (SnapshotStrategy == NbtSnapshotStrategy.Enable)
            {
                _paletteList[y] = list;
                _blockStateRaw[y] = blocks.Value;
            }
            else
            {
                _palette[y] = ToNamespacedBlockList(list);
                _blockStates[y] = DynBitArray.CreateFromLongArray(blocks.Value);
            }

            return true;
        }

        private bool EnsureY(int y)
        {
            if (_blockStates[y] == null)
            {
                var longs = _blockStateRaw[y];

                if (longs != null)
                {
                    _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                    _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList[y] = null;
                    _blockStateRaw[y] = null;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void EnsureAllYs()
        {
            for (var sy = 0; sy < 16; sy++)
            {
                var y = sy;
                var longs = _blockStateRaw[sy];

                if (longs == null)
                    continue;

                _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                _paletteList[y] = null;
                _blockStateRaw[y] = null;
            }
        }

        private bool EnsureY(int y, out IDynBitArray bitarray, out List<NamespacedBlock> palette)
        {
            bitarray = _blockStates[y];
            if (bitarray == null)
            {
                var longs = _blockStateRaw[y];
                if (longs != null)
                {
                    palette = _palette[y] = ToNamespacedBlockList(_paletteList[y]);
                    bitarray = _blockStates[y] = DynBitArray.CreateFromLongArray(longs);

                    _paletteList[y] = null;
                    _blockStateRaw[y] = null;
                    return true;
                }
                else
                {
                    bitarray = null;
                    palette = null;
                    return false;
                }
            }

            palette = _palette[y];
            return true;
        }

        public override IEnumerable<int> GetExistingYs()
        {
            for (var y = 0; y < 16; y++)
                if(_blockStates[y] != null)
                yield return y;

            for (var y = 0; y < 16; y++)
                if (_blockStateRaw[y] != null)
                    yield return y;
        }

        public NamespacedBlock GetBlock(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out var palette))
                return NamespacedBlock.AirBlock;

            var index = blocks[GetBlockIndexByCoord(x, y, z)];
            if (index < 0 || index >= palette.Count)
            {
                ExceptionHelper.ThrowParseError("Palette: Index out of range", ParseErrorLevel.Exception);
                return default;
            }

            return palette[index];
        }
        
        public int GetBlockIndex(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out _))
                return -1;

            return blocks[GetBlockIndexByCoord(x, y, z)];
        }

        public IEnumerable<(int X, int Y, int Z, NamespacedBlock Block)> AllBlocks()
        {
            EnsureAllYs();

            for (var sy = 0; sy < 16; sy++)
            {
                var blocks = _blockStates[sy];
                if (blocks == null)
                    continue;

                var baseY = sy << 4;
                var index = 0;
                var palette = _palette[sy];

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, palette[blocks[index]]);
                            ++index;
                        }
            }
        }

        public IEnumerable<(int X, int Y, int Z, int Index)> AllBlockIndexes()
        {
            EnsureAllYs();

            for (var sy = 0; sy < 16; sy++)
            {
                var blocks = _blockStates[sy];
                if (blocks == null)
                    continue;

                var baseY = sy << 4;
                var index = 0;

                for (var y = 0; y < 16; y++)
                    for (var z = 0; z < 16; z++)
                        for (var x = 0; x < 16; x++)
                        {
                            yield return (x, y + baseY, z, blocks[index]);
                            ++index;
                        }
            }
        }

        public List<NamespacedBlock> GetPalette(int y)
        {
            EnsureY(y);
            return _palette[y];
        }

        /// <summary>
        /// Create a transaction containing necessary methods to change blocks.
        /// It is much faster than calling <see cref="SetBlock(int, int, int, NamespacedBlock)"/>, etc.
        /// </summary>
        /// <returns></returns>
        public BlockChangeTransaction CreateChangeBlockTransaction()
        {
            return new BlockChangeTransaction(this);
        }

        internal override bool IsAirBlock(int x, int y, int z)
        {
            if (!EnsureY(y >> 4, out var blocks, out var palette))
                return true;

            var index = blocks[GetBlockIndexByCoord(x, y, z)];
            if (index < 0 || index >= palette.Count)
            {
                return false;
            }

            return palette[index].Name == NamespacedBlock.IdAirBlock;
        }

        /// <summary>
        /// DO NOT USE THIS, unless you are only changing one block.
        /// Use <see cref="CreateChangeBlockTransaction"/> for the best practice.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="block"></param>
        [Obsolete("This method is not recommended unless you are only changing one block.")]
        public void SetBlock(int x, int y, int z, NamespacedBlock block)
        {
            using (var t = CreateChangeBlockTransaction())
            {
                t.Set(x, y, z, block);
                t.CommitChanges();
            }
        }

        /// <summary>
        /// Change multiple blocks.
        /// Use <see cref="CreateChangeBlockTransaction"/> for the best practice.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="customPalette"></param>
        public void SetBlock(IEnumerable<ChangeBlockRequest> requests, NamespacedBlock[] customPalette)
        {
            using (var t = CreateChangeBlockTransaction())
            {
                t.Set(requests, customPalette);
                t.CommitChanges();
            }
        }

        /// <summary>
        /// Change multiple blocks.
        /// Use <see cref="CreateChangeBlockTransaction"/> for the best practice.
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="customPalette"></param>
        public void SetBlock(IEnumerable<ChangeBlockRequest> requests, IList<NamespacedBlock> customPalette)
        {
            using (var t = CreateChangeBlockTransaction())
            {
                t.Set(requests, customPalette);
                t.CommitChanges();
            }
        }

        private int _blockVersion = 0;

        /// <summary>
        /// Due to the speciality of palette-based chunks, a manipulator is required to change any block.
        /// </summary>
        public class BlockChangeTransaction : IDisposable
        {
            private readonly NamespacedChunk _chunk;
            private int _preversion;

            internal BlockChangeTransaction(NamespacedChunk chunk)
            {
                _chunk = chunk;
                InitializeData();
            }

            /// <summary>
            /// The default block to fill empty sections.
            /// By default AirBlock.
            /// </summary>
            public NamespacedBlock BlockToFill = NamespacedBlock.AirBlock;

            private ushort[][] _blocks = new ushort[16][];
            private bool[] _changed = new bool[16];
            private List<NamespacedBlock>[] _palette = new List<NamespacedBlock>[16];

            private void EnsureSection(int y, bool putDefault = true)
            {
                if (_blocks[y] == null)
                {
                    _blocks[y] = new ushort[4096];

                    if (putDefault)
                    {
                        _palette[y] = new List<NamespacedBlock> { (NamespacedBlock)BlockToFill.Clone() };
                    }
                    else
                    {
                        _palette[y] = new List<NamespacedBlock>();
                    }
                }
            }

            /// <summary>
            /// Return to the original data defined in chunk
            /// </summary>
            public void InitializeData()
            {
                for (var i = 0; i < 16; i++)
                {
                    _blocks[i] = null;
                    _palette[i]?.Clear();
                    _palette[i] = null;
                }

                _chunk.EnsureAllYs();

                unchecked
                {
                    foreach (var y in _chunk.GetExistingYs())
                    {
                        EnsureSection(y, false);
                        for (var i = 0; i < 4096; i++)
                            _blocks[y][i] = (ushort)_chunk._blockStates[y][i];
                        _palette[y].AddRange(_chunk._palette[y].Select(block => (NamespacedBlock)block.Clone()));
                    }
                }

                for (var i = 0; i < 16; i++) _changed[i] = false;
                IsModified = false;
                IsAbandoned = false;
                _preversion = _chunk._blockVersion;
            }

            private int FindOrCreateInternal(int section, NamespacedBlock block)
            {
                var blocks = _palette[section];

                for (var i = 0; i < blocks.Count; i++)
                    if (blocks[i] == block)
                        return i;

                blocks.Add((NamespacedBlock)block.Clone());

                ModifiedSection(section);
                Modified();

                return blocks.Count - 1;
            }

            public IEnumerable<int> GetExistingSections()
            {
                for (var i = 0; i < 16; i++)
                    if (_blocks[i] != null)
                        yield return i;
            }

            /// <summary>
            /// Find or create a palette entry.
            /// </summary>
            /// <param name="section"></param>
            /// <param name="block"></param>
            /// <param name="putDefault"></param>
            /// <returns></returns>
            public int FindOrCreate(int section, NamespacedBlock block, bool putDefault = true)
            {
                if (IsAbandoned)
                    throw new InvalidOperationException();

                if (section < 0 || section >= 16) throw new ArgumentOutOfRangeException(nameof(section));

                EnsureSection(section, putDefault);
                return FindOrCreateInternal(section, block);
            }

            /// <summary>
            /// Set a block.
            /// For an efficient way to set multiple blocks, see other overloads.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="block"></param>
            public void Set(int x, int y, int z, NamespacedBlock block)
            {
                if (IsAbandoned)
                    throw new InvalidOperationException();

                var sec = y >> 4;
                EnsureSection(sec);
                var index = FindOrCreateInternal(sec, block);

                _blocks[sec][GetBlockIndexByCoord(x, y, z)] = unchecked((ushort)index);
                ModifiedSection(sec);
                Modified();
            }

            public void Set(IEnumerable<ChangeBlockRequest> requests, NamespacedBlock[] customPalette)
            {
                Set(requests, new ArraySeqAccessor<NamespacedBlock>(customPalette));
            }

            public void Set(IEnumerable<ChangeBlockRequest> requests, IList<NamespacedBlock> customPalette)
            {
                Set(requests, new ListSeqAccessor<NamespacedBlock>(customPalette));
            }

            private void Set(IEnumerable<ChangeBlockRequest> requests, ISequenceAccessor<NamespacedBlock> customPalette)
            {
                if (IsAbandoned)
                    throw new InvalidOperationException();

                var paletteMapping = new int[customPalette.Length];

                foreach(var grp in requests.GroupBy(req => req.Y >> 4))
                {
                    EnsureSection(grp.Key);
                    var blocks = _blocks[grp.Key];

                    var blockChanges = grp.ToArray();

                    foreach (var rq in blockChanges)
                    {
                        paletteMapping[rq.InListIndex] = FindOrCreateInternal(grp.Key, customPalette[rq.InListIndex]);
                    }

                    foreach (var rq in blockChanges)
                    {
                        blocks[GetBlockIndexByCoord(rq.X, rq.Y, rq.Z)] = (ushort)paletteMapping[rq.InListIndex];
                    }

                    ModifiedSection(grp.Key);
                }

                Modified();
            }

            /// <summary>
            /// Set a block by its id in the palette.
            /// See <see cref="FindOrCreate"/> for the id.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="paletteId"></param>
            public void Set(int x, int y, int z, ushort paletteId)
            {
                if (IsAbandoned)
                    throw new InvalidOperationException();

                var sec = y >> 4;
                EnsureSection(sec);

                _blocks[sec][GetBlockIndexByCoord(x, y, z)] = paletteId;

                ModifiedSection(sec);
                Modified();
            }

            /// <summary>
            /// Get the id of the block in this transaction. Returns -1 if not found.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            public int GetId(int x, int y, int z)
            {
                var sec = y >> 4;
                if (_blocks[sec] == null) return -1;

                return _blocks[sec][GetBlockIndexByCoord(x, y, z)];
            }

            /// <summary>
            /// Get the id of the block in this transaction. Returns <see langword="null"/> if not found.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            public NamespacedBlock Get(int x, int y, int z)
            {
                var sec = y >> 4;
                if (_blocks[sec] == null) return null;

                return _palette[sec][_blocks[sec][GetBlockIndexByCoord(x, y, z)]];
            }

            /// <summary>
            /// Check if the object is modified since the last commit.
            /// </summary>
            public bool IsModified { get; private set; } = false;

            /// <summary>
            /// Check if the object has been rolled-back but not re-initialized.
            /// You cannot operate on an abandoned object.
            /// </summary>
            public bool IsAbandoned { get; private set; } = false;

            private void ModifiedSection(int id)
            {
                _changed[id] = true;
            }

            private void Modified()
            {
                IsModified = true;
            }

            /// <summary>
            /// Cancel all changes. The object will remain unusable until
            /// you called <see cref="InitializeData"/> later.
            /// </summary>
            /// <param name="reinit"></param>
            public void Rollback()
            {
                IsModified = false;
                IsAbandoned = true;
            }

            /// <summary>
            /// Whether the object is in a usable state
            /// </summary>
            public bool IsValid => !IsAbandoned && !IsUpdatedOutside;

            /// <summary>
            /// Whether the chunk has been updated of the transaction.
            /// </summary>
            public bool IsUpdatedOutside => _chunk._blockVersion != _preversion;

            /// <summary>
            /// Commit all changes to the original chunk.
            /// You may continue on this object.
            /// </summary>
            public void CommitChanges()
            {
                if (!IsModified)
                    return;

                if (IsUpdatedOutside)
                    throw new InvalidOperationException("The chunk has been updated outside.");

                _chunk._blockVersion++;
                _preversion++;

                for (var i = 0; i < 16; i++)
                    if (_changed[i])
                    {
                        CompactSection(i);
                        SaveSection(i);
                        _changed[i] = false;
                    }

                IsModified = false;
            }

            private void CompactSection(int section)
            {
                // TODO
            }

            private void SaveSection(int section)
            {
                _chunk._palette[section]?.Clear();

                if (_palette[section] == null || _palette[section].Count == 0)
                {
                    _chunk._palette[section] = null;
                    _chunk._blockStates[section] = null;
                    return;
                }

                _chunk._palette[section] = _palette[section].Clone();

                var cellSize = Math.Max(4, NumericUtility.GetRequiredBitLength(_palette[section].Count));
                var dyn = DynBitArray.CreateEmpty(cellSize, 4096);

                for (var i = 0; i < 4096; i++)
                {
                    if (_blocks[section][i] == 0)
                        continue;

                    dyn[i] = _blocks[section][i];
                }

                _chunk._blockStates[section]?.Clear();
                _chunk._blockStates[section] = dyn;
            }

            /// <summary>
            /// Dispose this object. Changes are not preserved if they are not committed.
            /// </summary>
            public void Dispose()
            {
                for (var i = 0; i < 16; i++)
                {
                    _blocks[i] = null;
                    _palette[i] = null;
                }
            }
        }
    }
}
