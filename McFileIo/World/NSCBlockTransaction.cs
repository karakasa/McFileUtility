using McFileIo.Blocks;
using McFileIo.Enum;
using McFileIo.Interfaces;
using McFileIo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McFileIo.World
{
    public class NSCBlockTransaction : IDisposable
    {
        public ConcurrencyStrategy ConcurrencyMode = ConcurrencyStrategy.OneTimeSnapshot;

        protected readonly NamespacedChunk _chunk;
        protected int[] _preversion = new int[16];

        internal NSCBlockTransaction(NamespacedChunk chunk)
        {
            _chunk = chunk;
            chunk.EnsureAllSections();
            InitializeData();
        }

        /// <summary>
        /// The default block to fill empty sections.
        /// By default AirBlock.
        /// </summary>
        public NamespacedBlock BlockToFill = NamespacedBlock.AirBlock;

        protected ushort[][] _blocks = new ushort[16][];
        protected List<NamespacedBlock>[] _palette = new List<NamespacedBlock>[16];
        protected List<int>[] _paletteCountUse = new List<int>[16];

        protected bool[] _changed = new bool[16];
        protected bool[] _paletteChanged = new bool[16];

        public IEnumerable<(NamespacedBlock Block, int Count)> GetPaletteInformation(int section)
        {
            if (_palette[section] == null) yield break;
            for (var i = 0; i < _palette[section].Count; i++)
            {
                yield return (_palette[section][i], _paletteCountUse[section][i]);
            }
        }

        protected void EnsureSection(int y, bool putDefault = true)
        {
            if (_blocks[y] == null)
            {
                _blocks[y] = new ushort[4096];

                if (putDefault)
                {
                    _palette[y] = new List<NamespacedBlock> { (NamespacedBlock)BlockToFill.Clone() };
                    _paletteCountUse[y] = new List<int>() { 4096 };
                }
                else
                {
                    _palette[y] = new List<NamespacedBlock>();
                    _paletteCountUse[y] = new List<int>();
                }
            }
        }

        protected void LoadSection(int y)
        {
            _blocks[y] = null;
            _palette[y]?.Clear();
            _palette[y] = null;
            _paletteCountUse[y]?.Clear();
            _paletteCountUse[y] = null;

            unchecked
            {
                if (_chunk._blockStates[y] != null)
                {
                    EnsureSection(y, false);
                    _palette[y].AddRange(_chunk._palette[y].Select(block => (NamespacedBlock)block.Clone()));
                    _paletteCountUse[y].AddRange(Enumerable.Repeat(0, _palette[y].Count));

                    for (var i = 0; i < 4096; i++)
                    {
                        var id = _chunk._blockStates[y][i];
                        _paletteCountUse[y][id]++;
                        _blocks[y][i] = (ushort)id;
                    }
                }
            }

            _changed[y] = false;
            _paletteChanged[y] = false;
        }

        /// <summary>
        /// Return to the original data defined in chunk
        /// </summary>
        public void InitializeData()
        {
            for (var i = 0; i < 16; i++)
                LoadSection(i);

            IsModified = false;
            IsAbandoned = false;
            Array.Copy(_chunk._blockVersion, _preversion, 16);
        }

        private int FindOrCreateInternal(int section, NamespacedBlock block)
        {
            var blocks = _palette[section];

            for (var i = 0; i < blocks.Count; i++)
                if (blocks[i] == block)
                    return i;

            blocks.Add((NamespacedBlock)block.Clone());
            _paletteCountUse[section].Add(0);

            _paletteChanged[section] = true;

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
            var inArrayIndex = Chunk.GetBlockIndexByCoord(x, y, z);
            EnsureSection(sec);
            var index = FindOrCreateInternal(sec, block);

            _paletteCountUse[sec][_blocks[sec][inArrayIndex]]--;
            _paletteCountUse[sec][index]++;

            _blocks[sec][inArrayIndex] = unchecked((ushort)index);
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

        protected void Set(IEnumerable<ChangeBlockRequest> requests, ISequenceAccessor<NamespacedBlock> customPalette)
        {
            if (IsAbandoned)
                throw new InvalidOperationException();

            var paletteMapping = new int[customPalette.Length];

            foreach (var grp in requests.GroupBy(req => req.Y >> 4))
            {
                EnsureSection(grp.Key);
                var blocks = _blocks[grp.Key];
                var blockChanges = grp.ToArray();

                foreach (var rq in blockChanges)
                {
                    paletteMapping[rq.InListIndex] = FindOrCreateInternal(grp.Key, customPalette[rq.InListIndex]);
                }

                var paletteCount = _paletteCountUse[grp.Key];

                foreach (var rq in blockChanges)
                {
                    var mappedId = paletteMapping[rq.InListIndex];
                    var inArrayId = Chunk.GetBlockIndexByCoord(rq.X, rq.Y, rq.Z);

                    paletteCount[blocks[inArrayId]]--;
                    paletteCount[mappedId]++;

                    blocks[inArrayId] = (ushort)mappedId;
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

            _blocks[sec][Chunk.GetBlockIndexByCoord(x, y, z)] = paletteId;

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

            return _blocks[sec][Chunk.GetBlockIndexByCoord(x, y, z)];
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

            return _palette[sec][_blocks[sec][Chunk.GetBlockIndexByCoord(x, y, z)]];
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

        protected void ModifiedSection(int id)
        {
            _changed[id] = true;
        }

        protected void Modified()
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
        public bool IsValid => !IsAbandoned 
            && (ConcurrencyMode == ConcurrencyStrategy.Ignore || !IsUpdatedOutside);

        /// <summary>
        /// Whether the chunk has been updated of the transaction.
        /// </summary>
        public bool IsUpdatedOutside => _chunk._blockVersion.Zip(_preversion, (a, b) => a == b).Any(e => !e);

        private void ThrowChunkUpdatedOutside()
        {
            throw new InvalidOperationException("The chunk has been updated outside.");
        }

        /// <summary>
        /// Commit all changes to the original chunk.
        /// You may continue on this object.
        /// </summary>
        public void CommitChanges()
        {
            if (!IsModified)
                return;

            switch (ConcurrencyMode)
            {
                case ConcurrencyStrategy.OneTimeSnapshot:
                    for (var i = 0; i < 16; i++)
                        if (_changed[i])
                        {
                            if (_chunk._blockVersion[i] != _preversion[i])
                            {
                                ThrowChunkUpdatedOutside();
                            }

                            _preversion[i]++;
                            _chunk._blockVersion[i]++;
                        }
                    break;
                case ConcurrencyStrategy.UpdateOtherSection:
                    for (var i = 0; i < 16; i++)
                        if (_changed[i])
                        {
                            if (_chunk._blockVersion[i] != _preversion[i])
                            {
                                ThrowChunkUpdatedOutside();
                            }

                            _preversion[i]++;
                            _chunk._blockVersion[i]++;
                        }
                        else
                        {
                            if(_preversion[i] != _chunk._blockVersion[i])
                            {
                                _preversion[i] = _chunk._blockVersion[i];
                                LoadSection(i);
                            }
                        }
                    break;
            }

            for (var i = 0; i < 16; i++)
                if (_changed[i])
                {
                    CompactSection(i);
                    SaveSection(i);
                    _changed[i] = false;
                    _paletteChanged[i] = false;
                }

            IsModified = false;
        }

        /// <summary>
        /// Control if palette should be compacted by removing unused entries.
        /// By default <see langword="true" />.
        /// </summary>
        public bool CompactBeforeCommit = true;

        protected void CompactSection(int section)
        {
            if (!CompactBeforeCommit)
                return;

            var countUse = _paletteCountUse[section];

            var mapfrom = new int[countUse.Count];

            var startId = 0;
            var removed = 0;

            for (var i = 0; i < countUse.Count; i++)
            {
                if (countUse[i] == 0)
                {
                    mapfrom[i] = -1;
                    ++removed;
                }
                else
                {
                    mapfrom[i] = startId;
                    ++startId;
                }
            }

            if (removed == 0) return; // Nothing to compact

            _paletteChanged[section] = true;

            if (removed == countUse.Count - 1)
            {
                // Check all air situation
                for (var i = 0; i < mapfrom.Length; i++)
                {
                    if (mapfrom[i] == -1) continue;
                    if (_palette[section][i] == NamespacedBlock.AirBlock)
                    {
                        // All air section

                        _paletteCountUse[section]?.Clear();
                        _paletteCountUse[section] = null;
                        _palette[section]?.Clear();
                        _palette[section] = null;
                        _blocks[section] = null;

                        return;
                    }
                }
            }

            var newCountUse = new List<int>(countUse.Count - removed);
            var newPalette = new List<NamespacedBlock>(countUse.Count - removed);
            for (var i = 0; i < countUse.Count; i++)
            {
                if (countUse[i] != 0)
                {
                    newPalette.Add(_palette[section][i]);
                    newCountUse.Add(countUse[i]);
                }
            }

            var blocks = _blocks[section];

            for (var i = 0; i < 4096; i++)
            {
                var v = mapfrom[blocks[i]];
#if DEBUG
                if (v == -1)
                    throw new InvalidOperationException("v Shouldn't be -1. Check the source.");
#endif
                blocks[i] = (ushort)v;
            }

            _palette[section] = newPalette;
            _paletteCountUse[section] = newCountUse;
        }

        /// <summary>
        /// Control if block bits are compacted during save, if possible.
        /// By default <see langword="true"/>.
        /// </summary>
        public bool CompactBlockBitsIfPossible = true;

        protected void SaveSection(int section)
        {
            // Clear the section in chunk if it is cleared in this snapshot
            if (_palette[section] == null || _palette[section].Count == 0)
            {
                _chunk._palette[section]?.Clear();
                _chunk._palette[section] = null;
                _chunk._blockStates[section] = null;
                return;
            }

            // It's possible the section has not been created in chunk
            var oldCellSize = _chunk._blockStates[section] == null ? -1 
                : _chunk._blockStates[section].CellSize;

            // Only update the palette when it is changed
            if (_paletteChanged[section])
            {
                _chunk._palette[section]?.Clear();
                _chunk._palette[section] = _palette[section].Clone();
            }

            var newCellSize = Math.Max(4, NumericUtility.GetRequiredBitLength(_palette[section].Count));

            if ((newCellSize == oldCellSize) || (!CompactBlockBitsIfPossible && (newCellSize < oldCellSize)))
            {
                // Do not create new DynArray if the cellSize doesn't change to improve performance.

                var dyn = _chunk._blockStates[section];
                for (var i = 0; i < 4096; i++)
                {
                    dyn[i] = _blocks[section][i];
                }
            }
            else
            {
                // Create new DynArray when cellSize is changed

                var dyn = DynBitArray.CreateEmpty(newCellSize, 4096);

                for (var i = 0; i < 4096; i++)
                {
                    // Because the assignment of dynArray[i] can be time-consuming,
                    // index(0) is skipped because DynBitArray.CreateEmpty is guaranteed to create a zero-filled array

                    if (_blocks[section][i] == 0)
                        continue;

                    dyn[i] = _blocks[section][i];
                }

                _chunk._blockStates[section]?.Clear();
                _chunk._blockStates[section] = dyn;
            }
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

        internal void ModifyAll()
        {
            for (var i = 0; i < 16; i++)
                if (_blocks[i] != null)
                    ModifiedSection(i);

            Modified();
        }
    }
}
